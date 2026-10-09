using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BattleGame.Functions.DTOs;
using BattleGame.Functions.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace BattleGame.Functions.Functions
{
    public class GetAssetsByPlayerFunction
    {
        private readonly IPlayerAssetService _playerAssetService;
        private readonly ILogger<GetAssetsByPlayerFunction> _logger;

        public GetAssetsByPlayerFunction(IPlayerAssetService playerAssetService, ILogger<GetAssetsByPlayerFunction> logger)
        {
            _playerAssetService = playerAssetService;
            _logger = logger;
        }

        [Function("getassetsbyplayer")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "getassetsbyplayer")] HttpRequest req)
        {
            _logger.LogInformation("Processing getassetsbyplayer API request.");

            try
            {
                var report = await _playerAssetService.GetReportAsync();
                _logger.LogInformation("Retrieved {Count} report records.", report.Count);

                string? rawQuery = req.Query["raw"];
                if (string.Equals(rawQuery, "false", StringComparison.OrdinalIgnoreCase))
                {
                    return new OkObjectResult(ApiResponse<List<PlayerAssetReportItem>>.Ok(report));
                }

                return new OkObjectResult(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching player assets report.");
                return new ObjectResult(ApiResponse<string>.Fail($"Internal Server Error: {ex.Message}"))
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }
        }
    }
}
