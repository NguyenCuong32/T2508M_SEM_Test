using Microsoft.Extensions.Configuration;
using BattleGame.Api.Data;
using BattleGame.Api.Dtos;
using BattleGame.Api.Models;
using BattleGame.Api.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

if (args.Contains("--standalone") || Environment.GetEnvironmentVariable("STANDALONE") == "true")
{
    var appBuilder = WebApplication.CreateBuilder(args);
    var conn = appBuilder.Configuration.GetConnectionString("BattleGameDb")
        ?? appBuilder.Configuration["SQLAZURECONNSTR_BattleGameDb"]
        ?? "Server=(localdb)\\MSSQLLocalDB;Database=BATTLEGAME;Trusted_Connection=True;TrustServerCertificate=True";

    appBuilder.Services.AddDbContext<BattleGameDbContext>(o => o.UseSqlServer(conn));
    appBuilder.Services.AddCors(o => o.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

    var app = appBuilder.Build();
    app.UseCors();

    using (var scope = app.Services.CreateScope())
    {
        scope.ServiceProvider.GetRequiredService<BattleGameDbContext>().Database.Migrate();
    }

    app.MapPost("/api/registerplayer", async (RegisterPlayerRequest body, BattleGameDbContext db) =>
    {
        var errors = Validation.Validate(body);
        if (errors.Count > 0) return Results.BadRequest(new ErrorResponse("Validation failed.", errors));

        var name = body.PlayerName!.Trim();
        var email = body.Email!.Trim();
        if (await db.Players.AnyAsync(p => p.PlayerName == name || p.Email == email))
            return Results.Json(new ErrorResponse("PlayerName or Email already exists."), statusCode: StatusCodes.Status409Conflict);

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

        return Results.Created($"/api/players/{player.PlayerId}", new { player.PlayerId, player.PlayerName, player.FullName, player.Age, player.Level, player.Email });
    });

    app.MapPost("/api/createasset", async (CreateAssetRequest body, BattleGameDbContext db) =>
    {
        var errors = Validation.Validate(body);
        if (errors.Count > 0) return Results.BadRequest(new ErrorResponse("Validation failed.", errors));

        var asset = new Asset
        {
            AssetId = Guid.NewGuid(),
            AssetName = body.AssetName!.Trim(),
            LevelRequire = body.LevelRequire!.Value
        };
        db.Assets.Add(asset);
        await db.SaveChangesAsync();

        return Results.Created($"/api/assets/{asset.AssetId}", new { asset.AssetId, asset.AssetName, asset.LevelRequire });
    });

    app.MapGet("/", () => Results.Content("<h1>BattleGame API is Running</h1><p>Frontend Web: <a href='http://localhost:5173'>http://localhost:5173</a></p><p>GET Report API: <a href='/api/getassetsbyplayer'>/api/getassetsbyplayer</a></p><p>POST API: <code>/api/registerplayer</code>, <code>/api/createasset</code></p>", "text/html"));

    app.MapGet("/api/registerplayer", () => Results.Json(new
    {
        message = "Endpoint này yêu cầu method POST.",
        exampleBody = new
        {
            playerName = "Player 4",
            fullName = "Nguyen Van D",
            age = "21",
            level = 5,
            email = "p4@game.com"
        }
    }));

    app.MapGet("/api/createasset", () => Results.Json(new
    {
        message = "Endpoint này yêu cầu method POST.",
        exampleBody = new
        {
            assetName = "Hero 3",
            levelRequire = 5
        }
    }));

    app.MapGet("/api/getassetsbyplayer", async (BattleGameDbContext db) =>
    {
        var rows = await db.PlayerAssets
            .AsNoTracking()
            .OrderBy(pa => pa.Player.PlayerName).ThenBy(pa => pa.Asset.AssetName)
            .Select(pa => new { pa.Player.PlayerName, pa.Player.Level, pa.Player.Age, pa.Asset.AssetName })
            .ToListAsync();

        var report = rows
            .Select((r, i) => new PlayerAssetReportRow(i + 1, r.PlayerName, r.Level, r.Age, r.AssetName))
            .ToList();
        return Results.Ok(report);
    });

    app.Run("http://localhost:7071");
    return;
}

var builder = FunctionsApplication.CreateBuilder(args);
builder.ConfigureFunctionsWebApplication();

var connectionString = builder.Configuration.GetConnectionString("BattleGameDb")
    ?? builder.Configuration["SQLAZURECONNSTR_BattleGameDb"]
    ?? throw new InvalidOperationException("Connection string 'BattleGameDb' is not configured.");

builder.Services.AddDbContext<BattleGameDbContext>(o => o.UseSqlServer(connectionString));

var host = builder.Build();

// Apply pending EF migrations (creates the BATTLEGAME schema + sample data on first run).
using (var scope = host.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<BattleGameDbContext>().Database.Migrate();
}

host.Run();
