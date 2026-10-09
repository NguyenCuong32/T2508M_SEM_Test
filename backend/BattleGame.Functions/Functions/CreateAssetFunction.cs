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
    public class CreateAssetFunction
    {
        private readonly IAssetService _assetService;
        private readonly ILogger<CreateAssetFunction> _logger;
        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

        public CreateAssetFunction(IAssetService assetService, ILogger<CreateAssetFunction> logger)
        {
            _assetService = assetService;
            _logger = logger;
        }

        [Function("createasset")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "createasset")] HttpRequest req)
        {
            _logger.LogInformation("Processing createasset API request.");

            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                if (string.IsNullOrWhiteSpace(requestBody))
                {
                    return new BadRequestObjectResult(ApiResponse<string>.Fail("Request body cannot be empty."));
                }

                var dto = JsonSerializer.Deserialize<CreateAssetRequest>(requestBody, JsonOptions);
                if (dto == null)
                {
                    return new BadRequestObjectResult(ApiResponse<string>.Fail("Invalid JSON payload."));
                }

                var asset = await _assetService.CreateAssetAsync(dto);
                _logger.LogInformation("Asset created successfully: ID {AssetId}", asset.AssetId);

                return new CreatedResult($"/api/createasset/{asset.AssetId}", ApiResponse<Asset>.Ok(asset, "Asset created successfully."));
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
                _logger.LogError(ex, "Error occurred during asset creation.");
                return new ObjectResult(ApiResponse<string>.Fail($"Internal Server Error: {ex.Message}"))
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }
        }
    }
}
