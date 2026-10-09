using System.Net;
using System.Text.Json;
using BattleGame.Api.Data;
using BattleGame.Api.DTOs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BattleGame.Api.Functions;

public class GetAssetsByPlayerFunction
{
    private readonly BattleGameDbContext _dbContext;
    private readonly ILogger<GetAssetsByPlayerFunction> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public GetAssetsByPlayerFunction(BattleGameDbContext dbContext, ILogger<GetAssetsByPlayerFunction> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    [Function("GetAssetsByPlayer")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "getassetsbyplayer")] HttpRequestData req)
    {
        _logger.LogInformation("Processing GetAssetsByPlayer report query...");

        try
        {
            // JOIN 3 tables: Player, PlayerAsset, Asset
            var rawQuery = from p in _dbContext.Players
                           join pa in _dbContext.PlayerAssets on p.PlayerId equals pa.PlayerId
                           join a in _dbContext.Assets on pa.AssetId equals a.AssetId
                           orderby p.PlayerName, a.AssetName
                           select new
                           {
                               p.PlayerName,
                               p.Level,
                               p.Age,
                               a.AssetName
                           };

            var queryResults = await rawQuery.ToListAsync();

            // Project to PlayerAssetReportDto with sequential 'No' (1-based index)
            var reportData = queryResults.Select((item, index) => new PlayerAssetReportDto
            {
                No = index + 1,
                PlayerName = item.PlayerName,
                Level = item.Level,
                Age = item.Age ?? "N/A",
                AssetName = item.AssetName
            }).ToList();

            _logger.LogInformation("Successfully retrieved {Count} player asset records.", reportData.Count);

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json; charset=utf-8");
            var json = JsonSerializer.Serialize(reportData, JsonOptions);
            await response.WriteStringAsync(json);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve player assets report");

            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            errorResponse.Headers.Add("Content-Type", "application/json; charset=utf-8");
            var json = JsonSerializer.Serialize(new
            {
                error = "Internal server error occurred while retrieving report",
                details = ex.Message
            }, JsonOptions);
            await errorResponse.WriteStringAsync(json);
            return errorResponse;
        }
    }
}
