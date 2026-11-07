using System.Net;
using RestSharp;

namespace Framework.Core.Http;

/// <summary>
/// Resposta de uma chamada à API, independente do RestSharp. É o que os Clients devolvem para os testes,
/// assim os testes não dependem da biblioteca HTTP usada por baixo.
/// </summary>
public sealed record ApiResponse<T>(HttpStatusCode StatusCode, T? Data, string? Content)
{
    public bool IsSuccess => (int)StatusCode is >= 200 and <= 299;

    internal static ApiResponse<T> From(RestResponse<T> response) =>
        new(response.StatusCode, response.Data, response.Content);
}

/// <summary>Resposta sem corpo tipado (ex.: 204 No Content, ou quando só o status interessa).</summary>
public sealed record ApiResponse(HttpStatusCode StatusCode, string? Content)
{
    public bool IsSuccess => (int)StatusCode is >= 200 and <= 299;

    internal static ApiResponse From(RestResponse response) => new(response.StatusCode, response.Content);
}
