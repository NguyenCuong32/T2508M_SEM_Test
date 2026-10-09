using BattleGame.Functions.Data;
using BattleGame.Functions.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;

namespace BattleGame.Functions.Functions;

public sealed class GetAssetsByPlayerFunction(BattleGameDbContext dbContext)
{
    [Function("getassetsbyplayer")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "getassetsbyplayer")]
        HttpRequest request)
    {
        Guid? playerId = null;
        var playerIdValue = request.Query["playerId"].ToString();

        if (!string.IsNullOrWhiteSpace(playerIdValue))
        {
            if (!Guid.TryParse(playerIdValue, out var parsedPlayerId))
                return new BadRequestObjectResult(new ApiError("playerId must be a valid GUID."));

            playerId = parsedPlayerId;
        }

        var query = dbContext.PlayerAssets
            .AsNoTracking()
            .Where(playerAsset => playerId == null || playerAsset.PlayerId == playerId)
            .OrderBy(playerAsset => playerAsset.Player.PlayerName)
            .ThenBy(playerAsset => playerAsset.Asset.AssetName)
            .Select(playerAsset => new
            {
                playerAsset.PlayerId,
                playerAsset.Player.PlayerName,
                playerAsset.Player.Level,
                playerAsset.Player.Age,
                playerAsset.AssetId,
                playerAsset.Asset.AssetName
            });

        var records = await query.ToListAsync(request.HttpContext.RequestAborted);
        var report = records.Select((record, index) => new PlayerAssetReportItem(
            index + 1,
            record.PlayerId,
            record.PlayerName,
            record.Level,
            record.Age,
            record.AssetId,
            record.AssetName));

        return new OkObjectResult(report);
    }
}
