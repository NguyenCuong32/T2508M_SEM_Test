using System.Net;
using BattleGame.Functions.Contracts;
using BattleGame.Functions.Data;
using BattleGame.Functions.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.EntityFrameworkCore;

namespace BattleGame.Functions.Functions;

public sealed class RegisterPlayerFunction(BattleGameDbContext db)
{
    [Function("registerplayer")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "registerplayer")] HttpRequestData request)
    {
        var input = await request.ReadFromJsonAsync<RegisterPlayerRequest>();
        if (input is null)
            return await FunctionResponses.JsonAsync(request, HttpStatusCode.BadRequest, new { message = "A JSON request body is required." });

        var playerName = input.PlayerName?.Trim();
        var fullName = input.FullName?.Trim();
        if (string.IsNullOrWhiteSpace(playerName) || playerName.Length > 50)
            return await FunctionResponses.JsonAsync(request, HttpStatusCode.BadRequest, new { message = "Player name is required and must be 50 characters or fewer." });
        if (string.IsNullOrWhiteSpace(fullName) || fullName.Length > 120)
            return await FunctionResponses.JsonAsync(request, HttpStatusCode.BadRequest, new { message = "Full name is required and must be 120 characters or fewer." });
        if (input.Age is < 1 or > 120 || input.CurrentLevel < 1)
            return await FunctionResponses.JsonAsync(request, HttpStatusCode.BadRequest, new { message = "Age must be between 1 and 120 and level must be at least 1." });
        if (await db.Players.AnyAsync(player => player.PlayerName == playerName))
            return await FunctionResponses.JsonAsync(request, HttpStatusCode.Conflict, new { message = "Player name is already registered." });

        var player = new Player
        {
            PlayerName = playerName,
            FullName = fullName,
            Age = input.Age,
            CurrentLevel = input.CurrentLevel
        };
        db.Players.Add(player);
        await db.SaveChangesAsync();

        return await FunctionResponses.JsonAsync(request, HttpStatusCode.Created, player);
    }
}
