using Microsoft.Extensions.Configuration;

namespace Framework.Core.Config;

/// <summary>
/// Configuração do sistema sob teste (qa-store-training), compartilhada por ApiTests e PlaywrightTests.
/// Ordem de prioridade (o último vence):
///   1. appsettings.json         — valores padrão do ambiente local (versionado)
///   2. appsettings.local.json   — overrides pessoais (ignorado pelo git)
///   3. variáveis de ambiente    — prefixo QASTORE_, ex.: QASTORE_App__WebUrl, QASTORE_Users__Admin__Password
/// </summary>
public sealed class TestSettings
{
    private static readonly Lazy<TestSettings> _current = new(Load);

    public static TestSettings Current => _current.Value;

    public AppSettings App { get; init; } = new();
    public UsersSettings Users { get; init; } = new();

    private static TestSettings Load()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.local.json", optional: true)
            .AddEnvironmentVariables(prefix: "QASTORE_")
            .Build();

        var settings = configuration.Get<TestSettings>()
            ?? throw new InvalidOperationException("appsettings.json está vazio.");

        settings.Validate();
        return settings;
    }

    private void Validate()
    {
        Require(App.WebUrl, "App:WebUrl");
        Require(App.ApiUrl, "App:ApiUrl");
        Require(Users.Admin.Email, "Users:Admin:Email");
        Require(Users.Admin.Password, "Users:Admin:Password");
        Require(Users.Customer.Email, "Users:Customer:Email");
        Require(Users.Customer.Password, "Users:Customer:Password");
    }

    private static void Require(string value, string key)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException($"Configuração obrigatória ausente: '{key}'.");
    }
}

public sealed class AppSettings
{
    public string WebUrl { get; init; } = "";
    public string ApiUrl { get; init; } = "";
}

public sealed class UsersSettings
{
    public UserCredentials Admin { get; init; } = new();
    public UserCredentials Customer { get; init; } = new();
}

public sealed class UserCredentials
{
    public string Email { get; init; } = "";
    public string Password { get; init; } = "";
}
