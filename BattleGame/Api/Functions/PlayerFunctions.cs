using BattleGame.Api.Data;
using BattleGame.Api.Dtos;
using BattleGame.Api.Models;
using BattleGame.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BattleGame.Api.Functions;

public class PlayerFunctions(BattleGameDbContext db, ILogger<PlayerFunctions> logger)
{
    [Function("registerplayer")]
    public async Task<IActionResult> RegisterPlayer(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "registerplayer")] HttpRequest req)
    {
        RegisterPlayerRequest? body;
        try { body = await req.ReadFromJsonAsync<RegisterPlayerRequest>(); }
        catch (Exception) { return Bad("Invalid JSON body."); }

        var errors = Validation.Validate(body);
        if (errors.Count > 0) return Bad("Validation failed.", errors);

        var name = body!.PlayerName!.Trim();
        var email = body.Email!.Trim();
        if (await db.Players.AnyAsync(p => p.PlayerName == name || p.Email == email))
            return new ObjectResult(new ErrorResponse("PlayerName or Email already exists."))
            { StatusCode = StatusCodes.Status409Conflict };

        var player = new Player
        {
            PlayerId = Guid.NewGuid(),
            PlayerName = name,
            FullName = body.FullName!.Trim(),
            Age = body.Age!.Trim(),
            Level = body.Level!.Value,
            Email = email
        };
        db.Players.Add(player);
        await db.SaveChangesAsync();
        logger.LogInformation("Registered player {PlayerId}", player.PlayerId);

        return new ObjectResult(new { player.PlayerId, player.PlayerName, player.FullName, player.Age, player.Level, player.Email })
        { StatusCode = StatusCodes.Status201Created };
    }

    [Function("getassetsbyplayer")]
    public async Task<IActionResult> GetAssetsByPlayer(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "getassetsbyplayer")] HttpRequest req)
    {
        var rows = await db.PlayerAssets
            .AsNoTracking()
            .OrderBy(pa => pa.Player.PlayerName).ThenBy(pa => pa.Asset.AssetName)
            .Select(pa => new { pa.Player.PlayerName, pa.Player.Level, pa.Player.Age, pa.Asset.AssetName })
            .ToListAsync();

        var report = rows
            .Select((r, i) => new PlayerAssetReportRow(i + 1, r.PlayerName, r.Level, r.Age, r.AssetName))
            .ToList();
        return new OkObjectResult(report);
    }

    private static BadRequestObjectResult Bad(string message, IReadOnlyList<string>? errors = null)
        => new(new ErrorResponse(message, errors));
}
