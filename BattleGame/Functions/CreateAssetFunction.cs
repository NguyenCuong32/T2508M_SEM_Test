using System.Net;
using BattleGame.DTOs;
using BattleGame.Services.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace BattleGame.Functions;

public sealed class CreateAssetFunction(IAssetService service, ILogger<CreateAssetFunction> logger)
{
    [Function("createasset")]
    public Task<HttpResponseData> RunAsync(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "createasset")] HttpRequestData request) =>
        FunctionResponse.ExecuteAsync(request, async () =>
        {
            var dto = await FunctionResponse.ReadAsync<CreateAssetDto>(request);
            var asset = await service.CreateAssetAsync(dto);
            return new { asset.AssetId, asset.AssetName, asset.LevelRequire };
        }, HttpStatusCode.Created, "Asset created successfully", logger);
}
