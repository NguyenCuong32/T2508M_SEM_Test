using System.Net;
using BattleGame.Core.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace BattleGame.Functions.Functions;

public class GetAssetsByPlayerFunction
{
    private readonly IBattleGameService _service;
    private readonly ILogger<GetAssetsByPlayerFunction> _logger;

    public GetAssetsByPlayerFunction(IBattleGameService service, ILogger<GetAssetsByPlayerFunction> logger)
    {
        _service = service;
        _logger = logger;
    }

    [Function("getassetsbyplayer")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "getassetsbyplayer")] HttpRequestData req)
    {
        _logger.LogInformation("Processing getassetsbyplayer request.");

        try
        {
            var report = await _service.GetAssetsByPlayerAsync();

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(report);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in getassetsbyplayer function.");
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteAsJsonAsync(new { message = "Server error retrieving report", error = ex.Message });
            return errorResponse;
        }
    }
}
