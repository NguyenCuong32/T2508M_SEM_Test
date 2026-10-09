using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using BattleGameFunction.DTOs;
using BattleGameFunction.Services;

namespace BattleGameFunction.Functions;

public class GetAssetsByPlayerFunction
{
    private readonly IReportService _reportService;
    private readonly ILogger<GetAssetsByPlayerFunction> _logger;

    public GetAssetsByPlayerFunction(IReportService reportService, ILogger<GetAssetsByPlayerFunction> logger)
    {
        _reportService = reportService;
        _logger = logger;
    }

    [Function("getassetsbyplayer")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "getassetsbyplayer")] HttpRequest req)
    {
        _logger.LogInformation("Processing getassetsbyplayer request via ReportService.");

        try
        {
            var reportList = await _reportService.GetAssetsByPlayerReportAsync();
            _logger.LogInformation("Successfully retrieved {Count} player asset records via ReportService.", reportList.Count);

            return new OkObjectResult(reportList);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching player assets.");
            return new ObjectResult(ApiResponse<string>.Fail($"Internal server error: {ex.Message}"))
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }
    }
}
