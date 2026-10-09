using BattleGame.Functions.Data;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace BattleGame.Functions.Functions;

public sealed class GetPlayersFunction(BattleGameDbContext db)
{
    [Function("getplayers")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "players")] HttpRequestData request)
    {
        var players = await db.Players.AsNoTracking()
            .OrderBy(player => player.PlayerName)
            .Select(player => new
            {
                id = player.Id,
                playerName = player.PlayerName,
                fullName = player.FullName,
                age = player.Age,
                currentLevel = player.CurrentLevel
            })
            .ToListAsync();
        return await FunctionResponses.JsonAsync(request, HttpStatusCode.OK, players);
    }
}
