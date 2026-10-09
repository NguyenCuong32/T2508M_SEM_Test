using BattleGame.Functions.Data;
using BattleGame.Functions.DTOs;
using BattleGame.Functions.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BattleGame.Functions.Functions;

public sealed class CreateAssetFunction(
    BattleGameDbContext dbContext,
    ILogger<CreateAssetFunction> logger)
{
    [Function("createasset")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "createasset")]
        HttpRequest request)
    {
        CreateAssetRequest? input;
        try
        {
            input = await request.ReadFromJsonAsync<CreateAssetRequest>(request.HttpContext.RequestAborted);
        }
        catch (Exception exception) when (exception is BadHttpRequestException or System.Text.Json.JsonException)
        {
            return new BadRequestObjectResult(new ApiError("Request body is not valid JSON."));
        }

        if (input is null)
            return new BadRequestObjectResult(new ApiError("Request body is required."));

        if (string.IsNullOrWhiteSpace(input.AssetName) || input.AssetName.Trim().Length > 64)
        {
            return new BadRequestObjectResult(new ApiError(
                "Validation failed.",
                new Dictionary<string, string[]>
                {
                    ["assetName"] = ["Asset name is required and must not exceed 64 characters."]
                }));
        }

        if (input.LevelRequire < 0)
        {
            return new BadRequestObjectResult(new ApiError(
                "Validation failed.",
                new Dictionary<string, string[]>
                {
                    ["levelRequire"] = ["Required level must be zero or greater."]
                }));
        }

        var assetName = input.AssetName.Trim();
        var cancellationToken = request.HttpContext.RequestAborted;

        if (await dbContext.Assets.AnyAsync(asset => asset.AssetName == assetName, cancellationToken))
            return new ConflictObjectResult(new ApiError("Asset name already exists."));

        var asset = new Asset
        {
            AssetId = Guid.NewGuid(),
            AssetName = assetName,
            LevelRequire = input.LevelRequire
        };

        dbContext.Assets.Add(asset);
        await dbContext.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Created asset {AssetId}", asset.AssetId);

        return new ObjectResult(new
        {
            asset.AssetId,
            asset.AssetName,
            asset.LevelRequire
        })
        {
            StatusCode = StatusCodes.Status201Created
        };
    }
}
