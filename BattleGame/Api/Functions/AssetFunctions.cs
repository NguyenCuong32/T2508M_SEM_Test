using BattleGame.Api.Data;
using BattleGame.Api.Dtos;
using BattleGame.Api.Models;
using BattleGame.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace BattleGame.Api.Functions;

public class AssetFunctions(BattleGameDbContext db, ILogger<AssetFunctions> logger)
{
    [Function("createasset")]
    public async Task<IActionResult> CreateAsset(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "createasset")] HttpRequest req)
    {
        CreateAssetRequest? body;
        try { body = await req.ReadFromJsonAsync<CreateAssetRequest>(); }
        catch (Exception) { return new BadRequestObjectResult(new ErrorResponse("Invalid JSON body.")); }

        var errors = Validation.Validate(body);
        if (errors.Count > 0)
            return new BadRequestObjectResult(new ErrorResponse("Validation failed.", errors));

        var asset = new Asset
        {
            AssetId = Guid.NewGuid(),
            AssetName = body!.AssetName!.Trim(),
            LevelRequire = body.LevelRequire!.Value
        };
        db.Assets.Add(asset);
        await db.SaveChangesAsync();
        logger.LogInformation("Created asset {AssetId}", asset.AssetId);

        return new ObjectResult(new { asset.AssetId, asset.AssetName, asset.LevelRequire })
        { StatusCode = StatusCodes.Status201Created };
    }
}
