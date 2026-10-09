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

public class CreateAssetFunction
{
    private readonly BattleGameDbContext _dbContext;
    private readonly ILogger<CreateAssetFunction> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public CreateAssetFunction(BattleGameDbContext dbContext, ILogger<CreateAssetFunction> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    [Function("CreateAsset")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "createasset")] HttpRequestData req)
    {
        _logger.LogInformation("Processing CreateAsset request...");

        try
        {
            using var reader = new StreamReader(req.Body);
            var requestBody = await reader.ReadToEndAsync();

            if (string.IsNullOrWhiteSpace(requestBody))
            {
                return await CreateJsonResponse(req, HttpStatusCode.BadRequest, 
                    ApiResponse<object>.Fail("Request body cannot be empty"));
            }

            var request = JsonSerializer.Deserialize<CreateAssetRequest>(requestBody, JsonOptions);
            if (request == null || string.IsNullOrWhiteSpace(request.AssetName))
            {
                return await CreateJsonResponse(req, HttpStatusCode.BadRequest, 
                    ApiResponse<object>.Fail("AssetName is required"));
            }

            // Check if asset name already exists
            var existingAsset = await _dbContext.Assets
                .FirstOrDefaultAsync(a => a.AssetName.ToLower() == request.AssetName.Trim().ToLower());

            if (existingAsset != null)
            {
                return await CreateJsonResponse(req, HttpStatusCode.Conflict, 
                    ApiResponse<object>.Fail($"Asset with name '{request.AssetName}' already exists"));
            }

            // Generate GUID if not provided or empty
            var assetId = (request.AssetId.HasValue && request.AssetId.Value != Guid.Empty)
                ? request.AssetId.Value
                : Guid.NewGuid();

            var newAsset = new Asset
            {
                AssetId = assetId,
                AssetName = request.AssetName.Trim(),
                LevelRequire = request.LevelRequire.GetValueOrDefault(1) < 1 ? 1 : request.LevelRequire.GetValueOrDefault(1),
                CreatedAt = DateTime.UtcNow
            };

            await _dbContext.Assets.AddAsync(newAsset);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Asset created successfully: {AssetId} ({AssetName})", newAsset.AssetId, newAsset.AssetName);

            return await CreateJsonResponse(req, HttpStatusCode.Created, 
                ApiResponse<Asset>.Ok(newAsset, "Asset created successfully"));
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Invalid JSON format in CreateAsset");
            return await CreateJsonResponse(req, HttpStatusCode.BadRequest, 
                ApiResponse<object>.Fail($"Invalid JSON format: {ex.Message}"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred during asset creation");
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
