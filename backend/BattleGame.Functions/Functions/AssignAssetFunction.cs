using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using BattleGame.Functions.DTOs;
using BattleGame.Functions.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace BattleGame.Functions.Functions
{
    public class AssignAssetFunction
    {
        private readonly IPlayerAssetService _playerAssetService;
        private readonly ILogger<AssignAssetFunction> _logger;
        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

        public AssignAssetFunction(IPlayerAssetService playerAssetService, ILogger<AssignAssetFunction> logger)
        {
            _playerAssetService = playerAssetService;
            _logger = logger;
        }

        [Function("assignasset")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "assignasset")] HttpRequest req)
        {
            _logger.LogInformation("Processing assignasset API request.");

            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                if (string.IsNullOrWhiteSpace(requestBody))
                {
                    return new BadRequestObjectResult(ApiResponse<string>.Fail("Request body cannot be empty."));
                }

                var dto = JsonSerializer.Deserialize<AssignAssetRequest>(requestBody, JsonOptions);
                if (dto == null)
                {
                    return new BadRequestObjectResult(ApiResponse<string>.Fail("Invalid JSON payload."));
                }

                var result = await _playerAssetService.AssignAssetAsync(dto);
                return new OkObjectResult(ApiResponse<object>.Ok(result, "Asset successfully assigned to player."));
            }
            catch (KeyNotFoundException ex)
            {
                return new NotFoundObjectResult(ApiResponse<string>.Fail(ex.Message));
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
                _logger.LogError(ex, "Error occurred while assigning asset.");
                return new ObjectResult(ApiResponse<string>.Fail($"Internal Server Error: {ex.Message}"))
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }
        }
    }
}
