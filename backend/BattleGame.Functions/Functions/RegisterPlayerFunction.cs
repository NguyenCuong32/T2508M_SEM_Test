using System.Net.Mail;
using BattleGame.Functions.Data;
using BattleGame.Functions.DTOs;
using BattleGame.Functions.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BattleGame.Functions.Functions;

public sealed class RegisterPlayerFunction(
    BattleGameDbContext dbContext,
    ILogger<RegisterPlayerFunction> logger)
{
    [Function("registerplayer")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "registerplayer")]
        HttpRequest request)
    {
        RegisterPlayerRequest? input;
        try
        {
            input = await request.ReadFromJsonAsync<RegisterPlayerRequest>(request.HttpContext.RequestAborted);
        }
        catch (Exception exception) when (exception is BadHttpRequestException or System.Text.Json.JsonException)
        {
            return new BadRequestObjectResult(new ApiError("Request body is not valid JSON."));
        }

        if (input is null)
        {
            return new BadRequestObjectResult(new ApiError("Request body is required."));
        }

        var errors = Validate(input);
        if (errors.Count > 0)
        {
            return new BadRequestObjectResult(new ApiError("Validation failed.", errors));
        }

        var playerName = input.PlayerName!.Trim();
        var email = input.Email!.Trim().ToLowerInvariant();
        var cancellationToken = request.HttpContext.RequestAborted;

        if (await dbContext.Players.AnyAsync(
                player => player.PlayerName == playerName || player.Email == email,
                cancellationToken))
        {
            return new ConflictObjectResult(
                new ApiError("Player name or email already exists."));
        }

        var player = new Player
        {
            PlayerId = Guid.NewGuid(),
            PlayerName = playerName,
            FullName = input.FullName!.Trim(),
            Age = input.Age,
            Level = input.Level,
            Email = email
        };

        dbContext.Players.Add(player);
        await dbContext.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Registered player {PlayerId}", player.PlayerId);

        return new ObjectResult(new
        {
            player.PlayerId,
            player.PlayerName,
            player.FullName,
            player.Age,
            player.Level,
            player.Email
        })
        {
            StatusCode = StatusCodes.Status201Created
        };
    }

    private static Dictionary<string, string[]> Validate(RegisterPlayerRequest input)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(input.PlayerName) || input.PlayerName.Trim().Length > 64)
            errors["playerName"] = ["Player name is required and must not exceed 64 characters."];

        if (string.IsNullOrWhiteSpace(input.FullName) || input.FullName.Trim().Length > 128)
            errors["fullName"] = ["Full name is required and must not exceed 128 characters."];

        if (input.Age is < 1 or > 120)
            errors["age"] = ["Age must be between 1 and 120."];

        if (input.Level < 1)
            errors["level"] = ["Level must be at least 1."];

        if (string.IsNullOrWhiteSpace(input.Email) || input.Email.Trim().Length > 64 || !IsValidEmail(input.Email))
            errors["email"] = ["A valid email of at most 64 characters is required."];

        return errors;
    }

    private static bool IsValidEmail(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return false;
        try
        {
            return new MailAddress(value).Address.Equals(value.Trim(), StringComparison.OrdinalIgnoreCase);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
