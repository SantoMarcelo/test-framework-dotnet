using System.Collections.Concurrent;
using Framework.Core.Api.Clients;
using Framework.Core.Api.Models.Auth;
using Framework.Core.Config;
using Framework.Core.Http;

namespace Framework.Core.Auth;

/// <summary>
/// Faz login via API e guarda o token por usuário durante toda a execução (o token da Loja QA vale 24h).
/// Compartilhado por ApiTests (header Bearer) e PlaywrightTests (localStorage).
/// </summary>
public static class AuthService
{
    private static readonly ConcurrentDictionary<string, Lazy<Task<string>>> _tokens = new(StringComparer.OrdinalIgnoreCase);

    public static Task<string> GetTokenAsync(UserCredentials user) =>
        _tokens.GetOrAdd(user.Email, _ => new Lazy<Task<string>>(() => LoginAsync(user))).Value;

    /// <summary>Descarta os tokens guardados (ex.: depois de um teste que troca a senha de um usuário).</summary>
    public static void ClearCache() => _tokens.Clear();

    private static async Task<string> LoginAsync(UserCredentials user)
    {
        using var client = RestClientFactory.Create();
        var response = await new AuthClient(client).LoginAsync(new LoginRequest(user.Email, user.Password));

        if (!response.IsSuccess || string.IsNullOrEmpty(response.Data?.Token))
        {
            _tokens.TryRemove(user.Email, out _);  // não guarda falha: a próxima chamada tenta de novo
            throw new InvalidOperationException(
                $"Login via API falhou para '{user.Email}': {(int)response.StatusCode} {response.Content}");
        }

        return response.Data.Token;
    }
}
