using System.Net;
using System.Text.Json;
using BattleGame.Api.Data;
using BattleGame.Api.DTOs;
using BattleGame.Api.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BattleGame.Api.Functions;

public class RegisterPlayerFunction
{
    private readonly BattleGameDbContext _dbContext;
    private readonly ILogger<RegisterPlayerFunction> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public RegisterPlayerFunction(BattleGameDbContext dbContext, ILogger<RegisterPlayerFunction> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    [Function("RegisterPlayer")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "registerplayer")] HttpRequestData req)
    {
        _logger.LogInformation("Processing RegisterPlayer request...");

        try
        {
            using var reader = new StreamReader(req.Body);
            var requestBody = await reader.ReadToEndAsync();

            if (string.IsNullOrWhiteSpace(requestBody))
            {
                return await CreateJsonResponse(req, HttpStatusCode.BadRequest, 
                    ApiResponse<object>.Fail("Request body cannot be empty"));
            }

            var request = JsonSerializer.Deserialize<RegisterPlayerRequest>(requestBody, JsonOptions);
            if (request == null || string.IsNullOrWhiteSpace(request.PlayerName))
            {
                return await CreateJsonResponse(req, HttpStatusCode.BadRequest, 
                    ApiResponse<object>.Fail("PlayerName is required"));
            }

            // Check if player name already exists
            var existingPlayer = await _dbContext.Players
                .FirstOrDefaultAsync(p => p.PlayerName.ToLower() == request.PlayerName.Trim().ToLower());

            if (existingPlayer != null)
            {
                return await CreateJsonResponse(req, HttpStatusCode.Conflict, 
                    ApiResponse<object>.Fail($"Player with name '{request.PlayerName}' already exists"));
            }

            // Generate GUID if not provided or empty
            var playerId = (request.PlayerId.HasValue && request.PlayerId.Value != Guid.Empty)
                ? request.PlayerId.Value
                : Guid.NewGuid();

            var newPlayer = new Player
            {
                PlayerId = playerId,
                PlayerName = request.PlayerName.Trim(),
                FullName = request.FullName?.Trim(),
                Age = request.Age?.Trim(),
                Level = request.Level.GetValueOrDefault(1) < 1 ? 1 : request.Level.GetValueOrDefault(1),
                Email = request.Email?.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            await _dbContext.Players.AddAsync(newPlayer);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Player registered successfully: {PlayerId} ({PlayerName})", newPlayer.PlayerId, newPlayer.PlayerName);

            return await CreateJsonResponse(req, HttpStatusCode.Created, 
                ApiResponse<Player>.Ok(newPlayer, "Player registered successfully"));
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Invalid JSON format in RegisterPlayer");
            return await CreateJsonResponse(req, HttpStatusCode.BadRequest, 
                ApiResponse<object>.Fail($"Invalid JSON format: {ex.Message}"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred during player registration");
            return await CreateJsonResponse(req, HttpStatusCode.InternalServerError, 
                ApiResponse<object>.Fail($"Internal server error: {ex.Message}"));
        }
    }

    private static async Task<HttpResponseData> CreateJsonResponse<T>(HttpRequestData req, HttpStatusCode statusCode, T data)
    {
        var response = req.CreateResponse(statusCode);
        response.Headers.Add("Content-Type", "application/json; charset=utf-8");
        var json = JsonSerializer.Serialize(data, JsonOptions);
        await response.WriteStringAsync(json);
        return response;
    }
}
