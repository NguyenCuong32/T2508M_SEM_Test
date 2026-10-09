using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Net;

namespace BattleGame.Functions.Functions;

public sealed class HealthFunction
{
    [Function("health")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "health")] HttpRequestData request) =>
        await FunctionResponses.JsonAsync(request, HttpStatusCode.OK, new { status = "ok" });
}
