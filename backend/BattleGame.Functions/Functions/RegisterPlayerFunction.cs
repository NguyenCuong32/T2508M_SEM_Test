using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using BattleGame.Functions.DTOs;
using BattleGame.Functions.Models;
using BattleGame.Functions.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace BattleGame.Functions.Functions
{
    public class RegisterPlayerFunction
    {
        private readonly IPlayerService _playerService;
        private readonly ILogger<RegisterPlayerFunction> _logger;
        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

        public RegisterPlayerFunction(IPlayerService playerService, ILogger<RegisterPlayerFunction> logger)
        {
            _playerService = playerService;
            _logger = logger;
        }

        [Function("registerplayer")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "registerplayer")] HttpRequest req)
        {
            _logger.LogInformation("Processing registerplayer API request.");

            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                if (string.IsNullOrWhiteSpace(requestBody))
                {
                    return new BadRequestObjectResult(ApiResponse<string>.Fail("Request body cannot be empty."));
                }

                var dto = JsonSerializer.Deserialize<RegisterPlayerRequest>(requestBody, JsonOptions);
                if (dto == null)
                {
                    return new BadRequestObjectResult(ApiResponse<string>.Fail("Invalid JSON payload."));
                }

                var player = await _playerService.RegisterPlayerAsync(dto);
                _logger.LogInformation("Player registered successfully: ID {PlayerId}", player.PlayerId);

                return new CreatedResult($"/api/registerplayer/{player.PlayerId}", ApiResponse<Player>.Ok(player, "Player registered successfully."));
            }
            catch (ArgumentException ex)
            {
                return new BadRequestObjectResult(ApiResponse<string>.Fail(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return new ConflictObjectResult(ApiResponse<string>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during player registration.");
                return new ObjectResult(ApiResponse<string>.Fail($"Internal Server Error: {ex.Message}"))
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }
        }
    }
}
