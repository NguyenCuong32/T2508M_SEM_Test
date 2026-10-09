using System.Net;
using System.Text.Json;
using BattleGame.Api.Data;
using BattleGame.Api.DTOs;
using BattleGame.Api.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BattleGame.Api.Functions;

public class AssignAssetFunction
{
    private readonly BattleGameDbContext _dbContext;
    private readonly ILogger<AssignAssetFunction> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public AssignAssetFunction(BattleGameDbContext dbContext, ILogger<AssignAssetFunction> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    [Function("AssignAsset")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "assignasset")] HttpRequestData req)
    {
        _logger.LogInformation("Processing AssignAsset request...");

        try
        {
            using var reader = new StreamReader(req.Body);
            var requestBody = await reader.ReadToEndAsync();

            if (string.IsNullOrWhiteSpace(requestBody))
            {
                return await CreateJsonResponse(req, HttpStatusCode.BadRequest, 
                    ApiResponse<object>.Fail("Request body cannot be empty"));
            }

            var request = JsonSerializer.Deserialize<AssignAssetRequest>(requestBody, JsonOptions);
            if (request == null || request.PlayerId == Guid.Empty || request.AssetId == Guid.Empty)
            {
                return await CreateJsonResponse(req, HttpStatusCode.BadRequest, 
                    ApiResponse<object>.Fail("Valid PlayerId and AssetId are required"));
            }

            // Verify Player exists
            var playerExists = await _dbContext.Players.AnyAsync(p => p.PlayerId == request.PlayerId);
            if (!playerExists)
            {
                return await CreateJsonResponse(req, HttpStatusCode.NotFound, 
                    ApiResponse<object>.Fail($"Player with ID '{request.PlayerId}' not found"));
            }

            // Verify Asset exists
            var assetExists = await _dbContext.Assets.AnyAsync(a => a.AssetId == request.AssetId);
            if (!assetExists)
            {
                return await CreateJsonResponse(req, HttpStatusCode.NotFound, 
                    ApiResponse<object>.Fail($"Asset with ID '{request.AssetId}' not found"));
            }

            // Check if already assigned
            var alreadyAssigned = await _dbContext.PlayerAssets
                .AnyAsync(pa => pa.PlayerId == request.PlayerId && pa.AssetId == request.AssetId);
            if (alreadyAssigned)
            {
                return await CreateJsonResponse(req, HttpStatusCode.Conflict, 
                    ApiResponse<object>.Fail("This asset is already assigned to the player"));
            }

            var playerAsset = new PlayerAsset
            {
                PlayerId = request.PlayerId,
                AssetId = request.AssetId,
                AcquiredAt = DateTime.UtcNow
            };

            await _dbContext.PlayerAssets.AddAsync(playerAsset);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Successfully assigned Asset {AssetId} to Player {PlayerId}", request.AssetId, request.PlayerId);

            return await CreateJsonResponse(req, HttpStatusCode.OK, 
                ApiResponse<object>.Ok(new { request.PlayerId, request.AssetId }, "Asset successfully assigned to player"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in AssignAsset");
            return await CreateJsonResponse(req, HttpStatusCode.InternalServerError, 
                ApiResponse<object>.Fail($"Internal server error: {ex.Message}"));
        }
    }

    private static async Task<HttpResponseData> CreateJsonResponse<T>(HttpRequestData req, HttpStatusCode statusCode, T data)
    {
        var response = req.CreateResponse(statusCode);
        response.Headers.Add("Content-Type", "application/json; charset=utf-8");
        var json = JsonSerializer.Serialize(data, JsonOptions);
        await response.WriteStringAsync(json);
        return response;
    }
}
