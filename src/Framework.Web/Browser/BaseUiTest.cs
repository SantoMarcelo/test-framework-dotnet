using Framework.Core.Api;
using Framework.Core.Auth;
using Framework.Core.Config;
using Framework.Web.Auth;
using Framework.Web.Evidence;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace Framework.Web.Browser;

/// <summary>
/// Classe base dos testes E2E:
/// - aponta o browser para o frontend configurado (Page.GotoAsync("/admin") já usa a URL base);
/// - permite entrar logado via API com <see cref="LoginAsAsync"/>, sem passar pela tela de login;
/// - grava o trace e salva quando o teste falha.
/// Headless/browser são controlados pelo Playwright (ex.: HEADED=1, BROWSER=firefox).
/// </summary>
public abstract class BaseUiTest : PageTest
{
    protected static TestSettings Settings => TestSettings.Current;

    public override BrowserNewContextOptions ContextOptions() => new()
    {
        BaseURL = Settings.App.WebUrl,
    };

    [SetUp]
    public Task StartTraceAsync() => TraceRecorder.StartAsync(Context);

    [TearDown]
    public Task StopTraceAsync() => TraceRecorder.StopAsync(Context);

    /// <summary>Autentica o browser como <paramref name="user"/>. Chame antes da primeira navegação do teste.</summary>
    protected async Task LoginAsAsync(UserCredentials user) =>
        await SessionInjector.InjectTokenAsync(Context, await AuthService.GetTokenAsync(user));

    /// <summary>API autenticada como <paramref name="user"/>, para preparar dados antes de abrir a tela.</summary>
    protected static Task<StoreApi> ApiAsAsync(UserCredentials user) => StoreApi.AsAsync(user);
}
