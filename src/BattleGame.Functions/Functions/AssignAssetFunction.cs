using System.Net;
using System.Text.Json;
using BattleGame.Core.Dtos;
using BattleGame.Core.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace BattleGame.Functions.Functions;

public class AssignAssetFunction
{
    private readonly IBattleGameService _service;
    private readonly ILogger<AssignAssetFunction> _logger;

    public AssignAssetFunction(IBattleGameService service, ILogger<AssignAssetFunction> logger)
    {
        _service = service;
        _logger = logger;
    }

    [Function("assignasset")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "assignasset")] HttpRequestData req)
    {
        _logger.LogInformation("Processing assignasset request.");

        try
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var dto = JsonSerializer.Deserialize<AssignAssetDto>(requestBody, options);

            if (dto == null || dto.PlayerId == Guid.Empty || dto.AssetId == Guid.Empty)
            {
                var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResponse.WriteAsJsonAsync(new { message = "Invalid payload. PlayerId and AssetId are required." });
                return badResponse;
            }

            var success = await _service.AssignAssetAsync(dto);

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(new
            {
                message = "Asset assigned to player successfully",
                success
            });
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in assignasset function.");
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteAsJsonAsync(new { message = "Server error assigning asset", error = ex.Message });
            return errorResponse;
        }
    }

    [Function("getplayers")]
    public async Task<HttpResponseData> GetPlayers(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "players")] HttpRequestData req)
    {
        var players = await _service.GetAllPlayersAsync();
        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(players);
        return response;
    }

    [Function("getassets")]
    public async Task<HttpResponseData> GetAssets(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "assets")] HttpRequestData req)
    {
        var assets = await _service.GetAllAssetsAsync();
        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(assets);
        return response;
    }
}
