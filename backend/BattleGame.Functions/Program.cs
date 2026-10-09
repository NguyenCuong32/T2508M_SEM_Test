using System;
using BattleGame.Functions.Data;
using BattleGame.Functions.Repositories;
using BattleGame.Functions.Repositories.Interfaces;
using BattleGame.Functions.Services;
using BattleGame.Functions.Services.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

// Retrieve connection string with fallback for localdb
var connectionString = Environment.GetEnvironmentVariable("SqlConnectionString")
    ?? Environment.GetEnvironmentVariable("ConnectionStrings:SqlConnectionString")
    ?? builder.Configuration.GetConnectionString("SqlConnectionString")
    ?? "Server=(localdb)\\MSSQLLocalDB;Database=BATTLEGAME;Trusted_Connection=True;TrustServerCertificate=True;";

builder.Services.AddDbContext<BattleGameDbContext>(options =>
{
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorNumbersToAdd: null);
    });
});

// Register Repositories
builder.Services.AddScoped<IPlayerRepository, PlayerRepository>();
builder.Services.AddScoped<IAssetRepository, AssetRepository>();
builder.Services.AddScoped<IPlayerAssetRepository, PlayerAssetRepository>();

// Register Services
builder.Services.AddScoped<IPlayerService, PlayerService>();
builder.Services.AddScoped<IAssetService, AssetService>();
builder.Services.AddScoped<IPlayerAssetService, PlayerAssetService>();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Configure JSON serialization
builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
{
    options.SerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});

builder.Build().Run();
