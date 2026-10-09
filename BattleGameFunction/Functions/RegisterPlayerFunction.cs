using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using BattleGameFunction.DTOs;
using BattleGameFunction.Models;
using BattleGameFunction.Services;

namespace BattleGameFunction.Functions;

public class RegisterPlayerFunction
{
    private readonly IPlayerService _playerService;
    private readonly ILogger<RegisterPlayerFunction> _logger;

    public RegisterPlayerFunction(IPlayerService playerService, ILogger<RegisterPlayerFunction> logger)
    {
        _playerService = playerService;
        _logger = logger;
    }

    [Function("registerplayer")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "registerplayer")] HttpRequest req)
    {
        _logger.LogInformation("Processing registerplayer request via PlayerService.");

        try
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var data = JsonSerializer.Deserialize<RegisterPlayerRequest>(requestBody, options);

            if (data == null || string.IsNullOrWhiteSpace(data.PlayerName))
            {
                return new BadRequestObjectResult(ApiResponse<string>.Fail("PlayerName is required and request body cannot be empty."));
            }

            var createdPlayer = await _playerService.RegisterPlayerAsync(data);

            _logger.LogInformation("Player created successfully with ID: {PlayerId}", createdPlayer.PlayerId);

            return new CreatedResult($"/api/players/{createdPlayer.PlayerId}", ApiResponse<Player>.Ok(createdPlayer, "Player registered successfully."));
        }
        catch (ArgumentException aEx)
        {
            return new BadRequestObjectResult(ApiResponse<string>.Fail(aEx.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while registering player.");
            return new ObjectResult(ApiResponse<string>.Fail($"Internal server error: {ex.Message}"))
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }
    }
}
