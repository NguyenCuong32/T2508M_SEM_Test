using System.Net;
using Microsoft.Azure.Functions.Worker.Http;

namespace BattleGame.Functions.Functions;

internal static class FunctionResponses
{
    public static async Task<HttpResponseData> JsonAsync<T>(HttpRequestData request, HttpStatusCode status, T body)
    {
        var response = request.CreateResponse(status);
        await response.WriteAsJsonAsync(body);
        return response;
    }

    public static HttpResponseData Empty(HttpRequestData request, HttpStatusCode status) => request.CreateResponse(status);
}
