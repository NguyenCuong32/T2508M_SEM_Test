using BattleGame.Functions.Data;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace BattleGame.Functions.Functions;

public sealed class GetAssetsFunction(BattleGameDbContext db)
{
    [Function("getassets")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "assets")] HttpRequestData request)
    {
        var assets = await db.Assets.AsNoTracking()
            .OrderBy(asset => asset.Name)
            .Select(asset => new
            {
                id = asset.Id,
                name = asset.Name,
                type = asset.Type,
                description = asset.Description
            })
            .ToListAsync();
        return await FunctionResponses.JsonAsync(request, HttpStatusCode.OK, assets);
    }
}
