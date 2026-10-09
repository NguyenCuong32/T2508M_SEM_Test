using Microsoft.Extensions.Configuration;

namespace BattleGame.Data;

public static class DatabaseConfiguration
{
    public static string GetConnectionString(IConfiguration configuration)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? configuration["Values:ConnectionStrings__DefaultConnection"]
            ?? configuration.GetConnectionString("DefaultConnection");
        return !string.IsNullOrWhiteSpace(connectionString)
            ? connectionString
            : throw new InvalidOperationException("DefaultConnection is not configured.");
    }
}
