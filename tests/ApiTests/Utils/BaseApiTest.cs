using Framework.Core.Api;
using Framework.Core.Config;

namespace ApiTests.Utils;

/// <summary>
/// Classe base dos testes de API.
/// <see cref="Api"/> é anônima; para chamadas autenticadas use <c>using var api = await ApiAsAsync(Settings.Users.Admin);</c>.
/// </summary>
public abstract class BaseApiTest
{
    protected static TestSettings Settings => TestSettings.Current;
    protected StoreApi Api { get; private set; } = null!;

    [SetUp]
    public void SetUpApi() => Api = StoreApi.Anonymous();

    [TearDown]
    public void TearDownApi() => Api.Dispose();

    protected static Task<StoreApi> ApiAsAsync(UserCredentials user) => StoreApi.AsAsync(user);
}
