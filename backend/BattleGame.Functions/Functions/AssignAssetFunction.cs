using System.Net;
using BattleGame.Functions.Contracts;
using BattleGame.Functions.Data;
using BattleGame.Functions.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.EntityFrameworkCore;

namespace BattleGame.Functions.Functions;

public sealed class AssignAssetFunction(BattleGameDbContext db)
{
    [Function("assignasset")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "assignasset")] HttpRequestData request)
    {
        var input = await request.ReadFromJsonAsync<AssignAssetRequest>();
        if (input is null)
            return await FunctionResponses.JsonAsync(request, HttpStatusCode.BadRequest, new { message = "A JSON request body is required." });
        if (!await db.Players.AnyAsync(player => player.Id == input.PlayerId))
            return await FunctionResponses.JsonAsync(request, HttpStatusCode.NotFound, new { message = "Player was not found." });
        if (!await db.Assets.AnyAsync(asset => asset.Id == input.AssetId))
            return await FunctionResponses.JsonAsync(request, HttpStatusCode.NotFound, new { message = "Asset was not found." });
        if (await db.PlayerAssets.AnyAsync(link => link.PlayerId == input.PlayerId && link.AssetId == input.AssetId))
            return await FunctionResponses.JsonAsync(request, HttpStatusCode.Conflict, new { message = "This player already owns the asset." });

        var link = new PlayerAsset { PlayerId = input.PlayerId, AssetId = input.AssetId, AcquiredAt = DateTime.UtcNow };
        db.PlayerAssets.Add(link);
        await db.SaveChangesAsync();
        return await FunctionResponses.JsonAsync(request, HttpStatusCode.Created, new { link.Id, link.PlayerId, link.AssetId });
    }
}
