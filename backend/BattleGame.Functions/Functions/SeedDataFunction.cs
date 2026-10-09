using System;
using System.Threading.Tasks;
using BattleGame.Functions.DTOs;
using BattleGame.Functions.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace BattleGame.Functions.Functions
{
    public class SeedDataFunction
    {
        private readonly IPlayerAssetService _playerAssetService;
        private readonly ILogger<SeedDataFunction> _logger;

        public SeedDataFunction(IPlayerAssetService playerAssetService, ILogger<SeedDataFunction> logger)
        {
            _playerAssetService = playerAssetService;
            _logger = logger;
        }

        [Function("seeddata")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "seeddata")] HttpRequest req)
        {
            _logger.LogInformation("Processing seeddata API request.");

            try
            {
                var msg = await _playerAssetService.SeedDataAsync();
                return new OkObjectResult(ApiResponse<string>.Ok(msg, "Seeding complete."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while seeding database.");
                return new ObjectResult(ApiResponse<string>.Fail($"Seeding error: {ex.Message}"))
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }
        }
    }
}
