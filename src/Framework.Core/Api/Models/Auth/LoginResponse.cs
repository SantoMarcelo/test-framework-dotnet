namespace Framework.Core.Api.Models.Auth;

/// <summary>Resposta de POST /auth/login.</summary>
public sealed record LoginResponse(string Token, int ExpiresIn, DateTimeOffset ExpiresAt, UserResponse User);
