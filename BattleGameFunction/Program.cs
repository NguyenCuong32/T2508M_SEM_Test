using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using BattleGameFunction.Data;
using BattleGameFunction.Repositories;
using BattleGameFunction.Services;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

// Configure connection string
var connectionString = Environment.GetEnvironmentVariable("MySqlConnectionString") 
    ?? builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=localhost;Port=3306;Database=BATTLEGAME;User=root;Password=;";

// 1. Register BattleGameDbContext with MySQL
builder.Services.AddDbContext<BattleGameDbContext>(options =>
{
    try
    {
        var serverVersion = ServerVersion.AutoDetect(connectionString);
        options.UseMySql(connectionString, serverVersion);
    }
    catch
    {
        // Fallback version if server cannot be auto-detected at startup
        options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 30)));
    }
});

// 2. Register Repositories (Repository Pattern)
builder.Services.AddScoped<IPlayerRepository, PlayerRepository>();
builder.Services.AddScoped<IAssetRepository, AssetRepository>();
builder.Services.AddScoped<IPlayerAssetRepository, PlayerAssetRepository>();

// 3. Register Services (Service Layer)
builder.Services.AddScoped<IPlayerService, PlayerService>();
builder.Services.AddScoped<IAssetService, AssetService>();
builder.Services.AddScoped<IReportService, ReportService>();

// 4. Configure CORS for Frontend access
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Build().Run();
