using BattleGame.Data;
using BattleGame.Repositories;
using BattleGame.Repositories.Interfaces;
using BattleGame.Services;
using BattleGame.Services.Interfaces;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.json", optional: true)
    .AddJsonFile("local.settings.json", optional: true).AddEnvironmentVariables();
builder.Services.AddDbContext<BattleGameDbContext>(options =>
    options.UseSqlServer(DatabaseConfiguration.GetConnectionString(builder.Configuration)));
builder.Services.AddScoped<IPlayerRepository, PlayerRepository>();
builder.Services.AddScoped<IAssetRepository, AssetRepository>();
builder.Services.AddScoped<IPlayerAssetRepository, PlayerAssetRepository>();
builder.Services.AddScoped<IPlayerService, PlayerService>();
builder.Services.AddScoped<IAssetService, AssetService>();
builder.Services.AddScoped<IPlayerAssetService, PlayerAssetService>();
builder.Build().Run();
