using BattleGame.Core.Interfaces;
using BattleGame.Infrastructure.Data;
using BattleGame.Infrastructure.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Enable CORS for Frontend SPA
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Dependency Injection Setup (Supports MySQL / phpMyAdmin & SQL Server & InMemory)
builder.Services.AddDbContext<BattleGameDbContext>((provider, options) =>
{
    var dbProvider = builder.Configuration["DbProvider"] ?? "MySQL";
    var useInMemory = builder.Configuration["UseInMemoryDatabase"];
    var mysqlConn = builder.Configuration["MySqlConnectionString"] 
        ?? "Server=localhost;Port=3306;Database=BATTLEGAME;Uid=root;Pwd=;";
    var sqlServerConn = builder.Configuration["SqlConnectionString"];

    if (string.Equals(useInMemory, "true", StringComparison.OrdinalIgnoreCase))
    {
        options.UseInMemoryDatabase("BATTLEGAME");
    }
    else if (string.Equals(dbProvider, "SqlServer", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(sqlServerConn))
    {
        options.UseSqlServer(sqlServerConn);
    }
    else
    {
        try
        {
            var serverVersion = ServerVersion.AutoDetect(mysqlConn);
            options.UseMySql(mysqlConn, serverVersion);
        }
        catch
        {
            // Fallback to InMemory if local MySQL server is not running
            options.UseInMemoryDatabase("BATTLEGAME");
        }
    }
});

builder.Services.AddScoped<IBattleGameService, BattleGameService>();

var app = builder.Build();

app.UseCors("AllowAll");

// API Route Endpoints (Matching Requirement 1, 2, 3)
app.MapPost("/api/registerplayer", async (IBattleGameService service, BattleGame.Core.Dtos.RegisterPlayerDto dto) =>
{
    if (dto == null || string.IsNullOrWhiteSpace(dto.PlayerName))
        return Results.BadRequest(new { message = "Invalid player payload. PlayerName is required." });
    
    var newPlayer = await service.RegisterPlayerAsync(dto);
    return Results.Ok(new { message = "Player registered successfully", data = newPlayer });
});

app.MapPost("/api/createasset", async (IBattleGameService service, BattleGame.Core.Dtos.CreateAssetDto dto) =>
{
    if (dto == null || string.IsNullOrWhiteSpace(dto.AssetName))
        return Results.BadRequest(new { message = "Invalid asset payload. AssetName is required." });
    
    var newAsset = await service.CreateAssetAsync(dto);
    return Results.Ok(new { message = "Asset created successfully", data = newAsset });
});

app.MapGet("/api/getassetsbyplayer", async (IBattleGameService service) =>
{
    var report = await service.GetAssetsByPlayerAsync();
    return Results.Ok(report);
});

app.MapPost("/api/assignasset", async (IBattleGameService service, BattleGame.Core.Dtos.AssignAssetDto dto) =>
{
    if (dto == null || dto.PlayerId == Guid.Empty || dto.AssetId == Guid.Empty)
        return Results.BadRequest(new { message = "Invalid payload. PlayerId and AssetId are required." });
    
    var success = await service.AssignAssetAsync(dto);
    return Results.Ok(new { message = "Asset assigned to player successfully", success });
});

app.MapGet("/api/players", async (IBattleGameService service) =>
{
    return Results.Ok(await service.GetAllPlayersAsync());
});

app.MapGet("/api/assets", async (IBattleGameService service) =>
{
    return Results.Ok(await service.GetAllAssetsAsync());
});

// Auto seed initial data on startup
using (var scope = app.Services.CreateScope())
{
    var service = scope.ServiceProvider.GetRequiredService<IBattleGameService>();
    await service.SeedInitialDataAsync();
}

app.Urls.Add("http://localhost:7071");

Console.WriteLine("=================================================");
Console.WriteLine("🚀 BATTLEGAME API Server running on http://localhost:7071");
Console.WriteLine("Endpoints:");
Console.WriteLine(" - POST http://localhost:7071/api/registerplayer");
Console.WriteLine(" - POST http://localhost:7071/api/createasset");
Console.WriteLine(" - GET  http://localhost:7071/api/getassetsbyplayer");
Console.WriteLine("=================================================");

app.Run();
