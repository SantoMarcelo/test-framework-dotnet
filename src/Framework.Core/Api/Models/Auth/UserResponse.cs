namespace Framework.Core.Api.Models.Auth;

/// <summary>Usuário público devolvido por /auth/login e /auth/me.</summary>
public sealed record UserResponse(int Id, string Name, string Email, string Role, DateTimeOffset CreatedAt);
