using System.Text.Json;
using Microsoft.Playwright;

namespace Framework.Web.Auth;

/// <summary>Coloca o token no localStorage antes de qualquer página carregar, como o frontend faz após o login.</summary>
public static class SessionInjector
{
    /// <summary>Chave usada pelo frontend da Loja QA (frontend/src/api/client.ts).</summary>
    public const string TokenStorageKey = "loja_token";

    public static Task InjectTokenAsync(IBrowserContext context, string token) =>
        context.AddInitScriptAsync(
            $"window.localStorage.setItem({JsonSerializer.Serialize(TokenStorageKey)}, {JsonSerializer.Serialize(token)});");
}
