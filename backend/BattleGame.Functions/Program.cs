using BattleGame.Functions.Data;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("BattleGame")
    ?? throw new InvalidOperationException("Connection string 'BattleGame' was not configured.");

builder.Services.AddDbContext<BattleGameDbContext>(options =>
    options.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure()));

var host = builder.Build();

// Bootstrap an empty database from the EF Core model before the Functions host accepts requests.
await using (var scope = host.Services.CreateAsyncScope())
{
    var database = scope.ServiceProvider.GetRequiredService<BattleGameDbContext>();
    await database.Database.EnsureCreatedAsync();
}

await host.RunAsync();
