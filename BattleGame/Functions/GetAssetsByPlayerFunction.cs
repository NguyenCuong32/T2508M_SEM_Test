using System.Net;
using BattleGame.Services.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace BattleGame.Functions;

public sealed class GetAssetsByPlayerFunction(IPlayerAssetService service, ILogger<GetAssetsByPlayerFunction> logger)
{
    [Function("getassetsbyplayer")]
    public Task<HttpResponseData> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "getassetsbyplayer")] HttpRequestData request) =>
        FunctionResponse.ExecuteAsync(request, service.GetAssetsByPlayerAsync,
            HttpStatusCode.OK, "Get player assets successfully", logger);
}
