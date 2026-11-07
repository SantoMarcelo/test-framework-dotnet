using Framework.Core.Api.Clients;
using Framework.Core.Auth;
using Framework.Core.Config;
using Framework.Core.Http;
using RestSharp;

namespace Framework.Core.Api;

/// <summary>
/// Porta de entrada para a API da Loja QA. Agrupa os Clients de cada módulo sobre um mesmo RestClient.
/// Usado pelos testes de API e pelos testes E2E (para preparar dados).
/// </summary>
public sealed class StoreApi : IDisposable
{
    private readonly RestClient _client;

    private StoreApi(RestClient client)
    {
        _client = client;
        Auth = new AuthClient(client);
    }

    public AuthClient Auth { get; }

    /// <summary>API sem autenticação.</summary>
    public static StoreApi Anonymous() => new(RestClientFactory.Create());

    /// <summary>API autenticada como <paramref name="user"/> (o login é feito uma vez por usuário e reaproveitado).</summary>
    public static async Task<StoreApi> AsAsync(UserCredentials user) =>
        new(RestClientFactory.Create(await AuthService.GetTokenAsync(user)));

    public void Dispose() => _client.Dispose();
}
