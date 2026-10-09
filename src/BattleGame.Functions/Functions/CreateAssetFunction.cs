using System.Net;
using System.Text.Json;
using BattleGame.Core.Dtos;
using BattleGame.Core.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace BattleGame.Functions.Functions;

public class CreateAssetFunction
{
    private readonly IBattleGameService _service;
    private readonly ILogger<CreateAssetFunction> _logger;

    public CreateAssetFunction(IBattleGameService service, ILogger<CreateAssetFunction> logger)
    {
        _service = service;
        _logger = logger;
    }

    [Function("createasset")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "createasset")] HttpRequestData req)
    {
        _logger.LogInformation("Processing createasset request.");

        try
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var dto = JsonSerializer.Deserialize<CreateAssetDto>(requestBody, options);

            if (dto == null || string.IsNullOrWhiteSpace(dto.AssetName))
            {
                var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResponse.WriteAsJsonAsync(new { message = "Invalid asset payload. AssetName is required." });
                return badResponse;
            }

            var newAsset = await _service.CreateAssetAsync(dto);

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(new
            {
                message = "Asset created successfully",
                data = newAsset
            });
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in createasset function.");
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteAsJsonAsync(new { message = "Server error creating asset", error = ex.Message });
            return errorResponse;
        }
    }
}
