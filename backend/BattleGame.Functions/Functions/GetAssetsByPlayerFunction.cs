using BattleGame.Functions.Data;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.EntityFrameworkCore;

namespace BattleGame.Functions.Functions;

public sealed class GetAssetsByPlayerFunction(BattleGameDbContext db)
{
    [Function("getassetsbyplayer")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "getassetsbyplayer")] HttpRequestData request)
    {
        int? playerId = null;
        var query = request.Url.Query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries);
        var playerIdParameter = query.FirstOrDefault(part => part.StartsWith("playerId=", StringComparison.OrdinalIgnoreCase));
        if (playerIdParameter is not null)
        {
            var value = Uri.UnescapeDataString(playerIdParameter[(playerIdParameter.IndexOf('=') + 1)..]);
            if (!int.TryParse(value, out var parsed) || parsed < 1)
                return await FunctionResponses.JsonAsync(request, System.Net.HttpStatusCode.BadRequest, new { message = "playerId must be a positive integer." });
            playerId = parsed;
        }

        var rows = await db.PlayerAssets.AsNoTracking()
            .Where(link => playerId == null || link.PlayerId == playerId.Value)
            .OrderBy(link => link.Player.PlayerName)
            .ThenBy(link => link.Asset.Name)
            .Select(link => new
            {
                playerId = link.Player.Id,
                playerName = link.Player.PlayerName,
                level = link.Player.CurrentLevel,
                age = link.Player.Age,
                assetId = link.Asset.Id,
                assetName = link.Asset.Name,
                assetType = link.Asset.Type
            })
            .ToListAsync();

        return await FunctionResponses.JsonAsync(request, System.Net.HttpStatusCode.OK, rows);
    }
}
