using System.Net;
using BattleGame.DTOs;
using BattleGame.Services.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace BattleGame.Functions;

public sealed class RegisterPlayerFunction(IPlayerService service, ILogger<RegisterPlayerFunction> logger)
{
    [Function("registerplayer")]
    public Task<HttpResponseData> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "registerplayer")] HttpRequestData request) =>
        FunctionResponse.ExecuteAsync(request, async () =>
        {
            var dto = await FunctionResponse.ReadAsync<RegisterPlayerDto>(request);
            var player = await service.RegisterPlayerAsync(dto);
            return new { player.PlayerId, player.PlayerName, player.FullName, player.Age, player.Level, player.Email };
        }, HttpStatusCode.Created, "Player registered successfully", logger);
}
