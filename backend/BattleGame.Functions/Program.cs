using BattleGame.Functions.Data;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

var connectionString = builder.Configuration["BATTLEGAME_CONNECTION_STRING"]
    ?? builder.Configuration.GetConnectionString("BATTLEGAME")
    ?? throw new InvalidOperationException(
        "Database connection string is missing. Set BATTLEGAME_CONNECTION_STRING.");

builder.Services.AddDbContext<BattleGameDbContext>(options =>
    options.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure()));

builder.Build().Run();
