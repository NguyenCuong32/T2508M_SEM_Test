using System.Net;
using System.Text.Json;
using BattleGame.Core.Dtos;
using BattleGame.Core.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace BattleGame.Functions.Functions;

public class RegisterPlayerFunction
{
    private readonly IBattleGameService _service;
    private readonly ILogger<RegisterPlayerFunction> _logger;

    public RegisterPlayerFunction(IBattleGameService service, ILogger<RegisterPlayerFunction> logger)
    {
        _service = service;
        _logger = logger;
    }

    [Function("registerplayer")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "registerplayer")] HttpRequestData req)
    {
        _logger.LogInformation("Processing registerplayer request.");

        try
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var dto = JsonSerializer.Deserialize<RegisterPlayerDto>(requestBody, options);

            if (dto == null || string.IsNullOrWhiteSpace(dto.PlayerName))
            {
                var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResponse.WriteAsJsonAsync(new { message = "Invalid player payload. PlayerName is required." });
                return badResponse;
            }

            var newPlayer = await _service.RegisterPlayerAsync(dto);

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(new
            {
                message = "Player registered successfully",
                data = newPlayer
            });
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in registerplayer function.");
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteAsJsonAsync(new { message = "Server error processing registration", error = ex.Message });
            return errorResponse;
        }
    }
}
