using Framework.Core.Config;
using RestSharp;
using RestSharp.Authenticators;

namespace Framework.Core.Http;

/// <summary>Único ponto que cria RestClient: URL base vem da configuração e o token, se houver, vira Bearer.</summary>
public static class RestClientFactory
{
    public static RestClient Create(string? token = null)
    {
        var options = new RestClientOptions(TestSettings.Current.App.ApiUrl)
        {
            Authenticator = token is null ? null : new JwtAuthenticator(token),
        };

        return new RestClient(options);
    }
}
