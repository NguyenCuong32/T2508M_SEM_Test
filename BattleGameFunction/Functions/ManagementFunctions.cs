using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using BattleGameFunction.DTOs;
using BattleGameFunction.Services;

namespace BattleGameFunction.Functions;

public class ManagementFunctions
{
    private readonly IPlayerService _playerService;
    private readonly IAssetService _assetService;
    private readonly IReportService _reportService;
    private readonly ILogger<ManagementFunctions> _logger;

    public ManagementFunctions(
        IPlayerService playerService,
        IAssetService assetService,
        IReportService reportService,
        ILogger<ManagementFunctions> logger)
    {
        _playerService = playerService;
        _assetService = assetService;
        _reportService = reportService;
        _logger = logger;
    }

    [Function("assignasset")]
    public async Task<IActionResult> AssignAsset(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "assignasset")] HttpRequest req)
    {
        _logger.LogInformation("Processing assignasset request via ReportService.");

        try
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var data = JsonSerializer.Deserialize<AssignAssetRequest>(requestBody, options);

            if (data == null || string.IsNullOrWhiteSpace(data.PlayerId) || string.IsNullOrWhiteSpace(data.AssetId))
            {
                return new BadRequestObjectResult(ApiResponse<string>.Fail("PlayerId and AssetId are required."));
            }

            var assignmentResult = await _reportService.AssignAssetToPlayerAsync(data);
            _logger.LogInformation("Successfully assigned asset {AssetId} to player {PlayerId}.", data.AssetId, data.PlayerId);

            return new OkObjectResult(ApiResponse<object>.Ok(assignmentResult, "Asset assigned to player successfully."));
        }
        catch (KeyNotFoundException kEx)
        {
            return new NotFoundObjectResult(ApiResponse<string>.Fail(kEx.Message));
        }
        catch (InvalidOperationException iEx)
        {
            return new ConflictObjectResult(ApiResponse<string>.Fail(iEx.Message));
        }
        catch (ArgumentException aEx)
        {
            return new BadRequestObjectResult(ApiResponse<string>.Fail(aEx.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while assigning asset.");
            return new ObjectResult(ApiResponse<string>.Fail($"Internal server error: {ex.Message}"))
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }
    }

    [Function("getplayers")]
    public async Task<IActionResult> GetPlayers(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "players")] HttpRequest req)
    {
        try
        {
            var players = await _playerService.GetAllPlayersAsync();
            return new OkObjectResult(players);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching players.");
            return new ObjectResult(ApiResponse<string>.Fail(ex.Message))
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }
    }

    [Function("getassets")]
    public async Task<IActionResult> GetAssets(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "assets")] HttpRequest req)
    {
        try
        {
            var assets = await _assetService.GetAllAssetsAsync();
            return new OkObjectResult(assets);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching assets.");
            return new ObjectResult(ApiResponse<string>.Fail(ex.Message))
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }
    }
}
