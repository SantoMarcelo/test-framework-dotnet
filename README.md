# Test Framework .NET — Loja QA

Automated test framework in .NET for **Loja QA** ([qa-store-training](https://github.com/SantoMarcelo/portifolio)), with two kinds of tests in the same solution:

- **API tests**: RestSharp + NUnit
- **E2E (UI) tests**: Playwright + NUnit

Both kinds of tests share the same configuration (URLs and users), the same authentication (login via API, with the token reused) and the same API Clients.

---

## Contents

- [Stack](#stack)
- [Prerequisites](#prerequisites)
- [Getting started](#getting-started)
- [Configuration](#configuration)
- [Project structure](#project-structure)
- [Architecture](#architecture)
- [Authentication](#authentication)
- [Writing API tests](#writing-api-tests)
- [Writing E2E tests](#writing-e2e-tests)
- [Evidence (trace)](#evidence-trace)
- [Useful commands](#useful-commands)
- [Conventions](#conventions)

---

## Stack

| Item | Version |
|---|---|
| .NET SDK | 10.0 |
| NUnit | 4.3.2 |
| Microsoft.Playwright.NUnit | 1.64.1 |
| RestSharp | 114.1.0 |
| Microsoft.Extensions.Configuration | 10.0.0 |

## Prerequisites

1. **.NET SDK 10**: `dotnet --version`
2. **PowerShell** (required to install the Playwright browsers): `brew install powershell` (without `--cask`)
3. **Loja QA running** ([GitHub repository](https://github.com/SantoMarcelo/portifolio)):

   ```bash
   git clone https://github.com/SantoMarcelo/portifolio.git qa-store-training
   cd qa-store-training
   docker compose up --build
   ```

   | Service | URL |
   |---|---|
   | Frontend | http://localhost:5173 |
   | API | http://localhost:3333/api/v1 |
   | Swagger | http://localhost:3333/api/docs |

## Getting started

```bash
dotnet build
pwsh tests/PlaywrightTests/bin/Debug/net10.0/playwright.ps1 install   # first time only
dotnet test
```

---

## Configuration

URLs and users live in a single file, used by both the API tests and the Playwright tests:

**[src/Framework.Core/appsettings.json](src/Framework.Core/appsettings.json)**

```json
{
  "App": {
    "WebUrl": "http://localhost:5173",
    "ApiUrl": "http://localhost:3333/api/v1"
  },
  "Users": {
    "Admin":    { "Email": "admin@loja.com",   "Password": "Admin@123" },
    "Customer": { "Email": "cliente@loja.com", "Password": "Cliente@123" }
  }
}
```

On build, the file is copied automatically to the output folder of each test project. In tests, use the `Settings` property of the base classes (`Settings.App.WebUrl`, `Settings.Users.Admin`...).

### Overriding values

Values can be overridden in this order (the last one wins):

| # | Source | Use |
|---|---|---|
| 1 | `appsettings.json` | Default values for the local environment. **Committed to git.** |
| 2 | `appsettings.local.json` (same folder) | Values for your machine only. **Ignored by git.** |
| 3 | Environment variables with the `QASTORE_` prefix | CI and other environments |

In environment variables, each JSON level is separated by `__` (two underscores):

```bash
QASTORE_App__WebUrl=https://staging.loja.com \
QASTORE_Users__Admin__Password='another-password' \
dotnet test
```

If a required setting is missing, the test fails right at startup with `Configuração obrigatória ausente: '<key>'` ("required setting missing").

### Adding a new setting

1. Add the key to `appsettings.json`.
2. Add the matching property to [TestSettings.cs](src/Framework.Core/Config/TestSettings.cs).
3. If it is required, add the key to the `Validate()` method.

---

## Project structure

```
TestFramework.slnx
├── src/
│   ├── Framework.Core/                 Shared infrastructure (no Playwright)
│   │   ├── appsettings.json            URLs and users
│   │   ├── Config/                     TestSettings: reads the configuration
│   │   ├── Http/                       RestClientFactory and ApiResponse<T>
│   │   ├── Auth/                       AuthService: login via API and token cache
│   │   ├── Api/
│   │   │   ├── StoreApi.cs             API entry point (groups the Clients)
│   │   │   ├── Clients/                One Client per API module (AuthClient...)
│   │   │   └── Models/<Module>/        Request/response DTOs
│   │   └── Data/                       Test data generation
│   │
│   └── Framework.Web/                  UI infrastructure (Core + Playwright)
│       ├── Browser/BaseUiTest.cs       Base class for E2E tests
│       ├── Auth/SessionInjector.cs     Writes the token to the browser's localStorage
│       └── Evidence/TraceRecorder.cs   Playwright trace when a test fails
│
└── tests/
    ├── ApiTests/                       → references Framework.Core
    │   ├── Utils/BaseApiTest.cs        Base class for API tests
    │   └── Tests/<Module>/             API tests
    │
    └── PlaywrightTests/                → references Framework.Web
        ├── Pages/<Area>/<Screen>/      Page Objects
        │   ├── Elements.cs             Locators only
        │   └── Actions.cs              Business actions for the screen
        ├── Components/                 UI parts used across screens (e.g. AdminNav)
        ├── Utils/                      UI-specific helpers
        └── E2E/<Area>/<Screen>/        E2E tests
```

---

## Architecture

### Layers and dependency rule

```
Tests  →  Actions (UI) / Clients (API)  →  Framework (infra)  →  Playwright / RestSharp
```

- **Tests** hold the scenario and the assertions. They do not import `RestSharp` or call the Playwright API directly. The exception is Playwright's `Expect`, which may be used in tests.
- **Actions and Clients** are the layer that talks to the system, one class per screen or per API module. They use Playwright and RestSharp **directly, with no wrappers**.
- **Framework** handles infrastructure only: configuration, authentication, creating the browser and the HTTP client, and evidence.

No layer skips the one below it, and no layer depends on the one above it.

### Why two framework projects

| Project | Depends on | Used by |
|---|---|---|
| `Framework.Core` | RestSharp, Configuration | ApiTests **and** PlaywrightTests |
| `Framework.Web` | Core + Playwright | PlaywrightTests only |

With this split, the API project does not depend on Playwright.

### Why the API Clients live in Core

E2E tests also use the API, to **set up data** before opening the browser: create a product, adjust stock, fill a cart. Setting up through the API is faster and more stable than through the UI. With the Clients in Core, both projects use the same code.

### Why there are no wrappers over Playwright

Playwright already has auto-waiting, locators that retry on their own, and assertions that retry until they pass (`Expect`). Generic functions like `ClickAsync` or `FillAsync` on top of it tend to lose these features. Isolation is done **by feature** (Actions), not by tool.

---

## Authentication

Login happens **via API**, once per user for the whole run. The token, which lasts 24h, is reused by every test.

```
AuthService.GetTokenAsync(user)
    │  POST /auth/login (only on the first call for each user)
    ├──► ApiTests: header  Authorization: Bearer <token>
    └──► PlaywrightTests: localStorage["loja_token"] = <token>
```

| Where | How to use |
|---|---|
| API test | `using var api = await ApiAsAsync(Settings.Users.Admin);` |
| E2E test | `await LoginAsAsync(Settings.Users.Admin);` (before the first navigation) |
| E2E setting up data | `using var api = await ApiAsAsync(Settings.Users.Admin);` |

> **Important:** login via API exists so other tests don't waste time on the form. The login flow **through the UI** needs its own tests in `E2E/Auth/`.

If a test changes a user's credentials (e.g. changes the password), call `AuthService.ClearCache()` after it.

---

## Writing API tests

### 1. Test using an existing Client

```csharp
using System.Net;
using ApiTests.Utils;
using Framework.Core.Api.Models.Auth;

namespace ApiTests.Tests.Auth;

public class AuthTests : BaseApiTest
{
    [Test]
    public async Task Login_WithCustomer_ShouldReturnToken()
    {
        var customer = Settings.Users.Customer;

        var response = await Api.Auth.LoginAsync(new LoginRequest(customer.Email, customer.Password));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task Me_WithAdminToken_ShouldReturnAdmin()
    {
        using var api = await ApiAsAsync(Settings.Users.Admin);   // authenticated

        var response = await api.Auth.MeAsync();

        Assert.That(response.Data?.Role, Is.EqualTo("ADMIN"));
    }
}
```

- `Api`: **unauthenticated** instance, created and disposed for each test.
- `ApiAsAsync(user)`: **authenticated** instance. Always use it with `using`.
- Clients return `ApiResponse<T>`, with `StatusCode`, `Data` (the body already converted to type `T`), `Content` (the body as text) and `IsSuccess`.

### 2. Adding a new Client (e.g. Products)

1. **Models**: in `src/Framework.Core/Api/Models/Products/`, create the DTOs as `record`s, using the JSON field names (matching is case-insensitive):

   ```csharp
   public sealed record ProductResponse(int Id, string Name, decimal Price, int Stock);
   ```

2. **Client**: in `src/Framework.Core/Api/Clients/ProductsClient.cs`:

   ```csharp
   public class ProductsClient(RestClient client)
   {
       public async Task<ApiResponse<ProductResponse>> GetAsync(int id) =>
           ApiResponse<ProductResponse>.From(
               await client.ExecuteAsync<ProductResponse>(
                   new RestRequest("products/{id}").AddUrlSegment("id", id)));
   }
   ```

3. **StoreApi**: in [StoreApi.cs](src/Framework.Core/Api/StoreApi.cs), expose the new Client:

   ```csharp
   public ProductsClient Products { get; }
   // in the constructor:
   Products = new ProductsClient(client);
   ```

4. **Tests**: create them in `tests/ApiTests/Tests/Products/`.

The Loja QA endpoints are documented in Swagger (http://localhost:3333/api/docs) and in the [qa-store-training README](https://github.com/SantoMarcelo/portifolio#readme).

---

## Writing E2E tests

### 1. Page Object: one folder per screen

Real example: [Pages/Admin/Dashboard/](tests/PlaywrightTests/Pages/Admin/Dashboard/)

**`Elements.cs`**: locators only, preferring `GetByTestId` (the frontend puts `data-testid` on every relevant element) or `GetByRole`.

```csharp
public class DashboardElements(IPage page)
{
    public ILocator Title => page.GetByRole(AriaRole.Heading, new() { Name = "Dashboard", Level = 1 });
    public ILocator TotalProducts => page.GetByTestId("dashboard-total-products");
}
```

**`Actions.cs`**: what the user does on the screen. Exposes the `Elements` for assertions.

```csharp
public class DashboardActions(IPage page)
{
    public const string Path = "/admin";
    public DashboardElements Elements { get; } = new(page);

    public async Task GotoAsync()
    {
        await page.GotoAsync(Path);   // relative: the base URL comes from the configuration
        await WaitForLoadAsync();
    }
}
```

**`Components/`**: UI parts that appear on several screens (header, admin menu, modal). Follow the same model, as in [AdminNav.cs](tests/PlaywrightTests/Components/AdminNav.cs).

The list of `data-testid` values is in the selectors section of the [qa-store-training README](https://github.com/SantoMarcelo/portifolio#readme).

### 2. Test

```csharp
using Framework.Web.Browser;
using PlaywrightTests.Pages.Admin.Dashboard;

namespace PlaywrightTests.E2E.Admin.Dashboard;

public class DashboardTests : BaseUiTest
{
    private DashboardActions _dashboard = null!;

    [SetUp]
    public async Task SetUp()
    {
        await LoginAsAsync(Settings.Users.Admin);   // logs in via API
        _dashboard = new DashboardActions(Page);
    }

    [Test]
    public async Task Admin_ShouldSeeDashboard()
    {
        await _dashboard.GotoAsync();

        await Expect(_dashboard.Elements.Title).ToBeVisibleAsync();
    }
}
```

### 3. Setting up data through the API before the test

```csharp
[Test]
public async Task Customer_ShouldSeeProductOnHome()
{
    using var adminApi = await ApiAsAsync(Settings.Users.Admin);
    var product = await adminApi.Products.CreateAsync(/* ... */);   // once ProductsClient exists

    await Page.GotoAsync("/");

    await Expect(Page.GetByTestId($"product-card-{product.Data!.Id}")).ToBeVisibleAsync();
}
```

### What `BaseUiTest` does

- Each test gets a new `Page` and `Context` (full isolation between tests).
- Sets the `BaseURL`, so `Page.GotoAsync("/admin")` already uses the configured `WebUrl`.
- `LoginAsAsync(user)` authenticates the browser.
- `ApiAsAsync(user)` returns the authenticated API.
- Records the trace and saves it when the test fails.

---

## Evidence (trace)

Every E2E test records a Playwright trace, with screenshots, DOM snapshots and source code. The file **is only saved when the test fails**, at:

```
tests/PlaywrightTests/bin/Debug/net10.0/playwright-traces/<FullTestName>.zip
```

The path also shows up as an attachment in the test result. To open it:

```bash
pwsh tests/PlaywrightTests/bin/Debug/net10.0/playwright.ps1 show-trace <file.zip>
```

---

## Useful commands

```bash
# Everything
dotnet test

# By project
dotnet test tests/ApiTests
dotnet test tests/PlaywrightTests

# By class or by test
dotnet test --filter "FullyQualifiedName~DashboardTests"
dotnet test --filter "Name=Admin_ShouldSeeDashboard"

# Watch the browser
HEADED=1 dotnet test tests/PlaywrightTests

# Another browser (chromium | firefox | webkit)
BROWSER=firefox dotnet test tests/PlaywrightTests

# Record a flow and generate code (useful for finding locators)
pwsh tests/PlaywrightTests/bin/Debug/net10.0/playwright.ps1 codegen http://localhost:5173
```

---

## Conventions

| Item | Pattern | Example |
|---|---|---|
| Folders | PascalCase; they become the namespace | `Pages/Admin/Dashboard` → `PlaywrightTests.Pages.Admin.Dashboard` |
| Page Object | One folder per screen, with `Elements.cs` + `Actions.cs` | `Pages/Admin/Products/` |
| Classes | `<Screen>Elements`, `<Screen>Actions`, `<Module>Client` | `DashboardActions`, `AuthClient` |
| Test class | `<Screen or Module>Tests` | `DashboardTests`, `AuthTests` |
| Test name | `Scenario_Condition_ExpectedResult` | `Login_WithCustomer_ShouldReturnToken` |
| Async methods | `Async` suffix | `GotoAsync`, `LoginAsync` |
| DTOs | `record`, `Request` / `Response` suffix | `LoginRequest`, `UserResponse` |

**Rules:**

- Tests do not use locators directly; they use `Elements` through `Actions`.
- API tests do not build `RestRequest`s; they use the Clients through `StoreApi`.
- Each test is independent: it does not depend on execution order or on data created by another test.
- Set up data through the API, not the UI, unless the UI is what is being tested.
- Use Playwright's `Expect(...)` for UI assertions, because it waits for the element before failing. Avoid `Assert` on values read from the page when an equivalent `Expect` exists.
