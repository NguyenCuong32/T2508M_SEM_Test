using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using BattleGameFunction.DTOs;
using BattleGameFunction.Models;
using BattleGameFunction.Services;

namespace BattleGameFunction.Functions;

public class CreateAssetFunction
{
    private readonly IAssetService _assetService;
    private readonly ILogger<CreateAssetFunction> _logger;

    public CreateAssetFunction(IAssetService assetService, ILogger<CreateAssetFunction> logger)
    {
        _assetService = assetService;
        _logger = logger;
    }

    [Function("createasset")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "createasset")] HttpRequest req)
    {
        _logger.LogInformation("Processing createasset request via AssetService.");

        try
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var data = JsonSerializer.Deserialize<CreateAssetRequest>(requestBody, options);

            if (data == null || string.IsNullOrWhiteSpace(data.AssetName))
            {
                return new BadRequestObjectResult(ApiResponse<string>.Fail("AssetName is required and request body cannot be empty."));
            }

            var createdAsset = await _assetService.CreateAssetAsync(data);

            _logger.LogInformation("Asset created successfully with ID: {AssetId}", createdAsset.AssetId);

            return new CreatedResult($"/api/assets/{createdAsset.AssetId}", ApiResponse<Asset>.Ok(createdAsset, "Asset created successfully."));
        }
        catch (ArgumentException aEx)
        {
            return new BadRequestObjectResult(ApiResponse<string>.Fail(aEx.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating asset.");
            return new ObjectResult(ApiResponse<string>.Fail($"Internal server error: {ex.Message}"))
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }
    }
}
