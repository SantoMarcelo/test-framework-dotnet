using Framework.Core.Api.Models.Auth;
using Framework.Core.Http;
using RestSharp;

namespace Framework.Core.Api.Clients;

/// <summary>Endpoints de /auth.</summary>
public class AuthClient(RestClient client)
{
    public async Task<ApiResponse<LoginResponse>> LoginAsync(LoginRequest request) =>
        ApiResponse<LoginResponse>.From(
            await client.ExecuteAsync<LoginResponse>(new RestRequest("auth/login", Method.Post).AddJsonBody(request)));

    public async Task<ApiResponse<UserResponse>> MeAsync() =>
        ApiResponse<UserResponse>.From(await client.ExecuteAsync<UserResponse>(new RestRequest("auth/me")));
}
