using BattleGame.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((hostContext, services) =>
    {
        // Retrieve connection string from Environment Variables or Configuration
        var connectionString = Environment.GetEnvironmentVariable("SqlConnectionString")
            ?? hostContext.Configuration.GetConnectionString("SqlConnectionString")
            ?? hostContext.Configuration["SqlConnectionString"]
            ?? "Server=(localdb)\\mssqllocaldb;Database=BATTLEGAME;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

        // Register Entity Framework Core with SQL Server
        services.AddDbContext<BattleGameDbContext>(options =>
        {
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorNumbersToAdd: null);
            });
        });
    })
    .Build();

await host.RunAsync();
