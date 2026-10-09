using System.Net;
using BattleGame.Functions.Contracts;
using BattleGame.Functions.Data;
using BattleGame.Functions.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.EntityFrameworkCore;

namespace BattleGame.Functions.Functions;

public sealed class CreateAssetFunction(BattleGameDbContext db)
{
    [Function("createasset")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "createasset")] HttpRequestData request)
    {
        var input = await request.ReadFromJsonAsync<CreateAssetRequest>();
        if (input is null)
            return await FunctionResponses.JsonAsync(request, HttpStatusCode.BadRequest, new { message = "A JSON request body is required." });

        var name = input.Name?.Trim();
        var type = input.Type?.Trim();
        if (string.IsNullOrWhiteSpace(name) || name.Length > 100)
            return await FunctionResponses.JsonAsync(request, HttpStatusCode.BadRequest, new { message = "Asset name is required and must be 100 characters or fewer." });
        if (string.IsNullOrWhiteSpace(type) || type.Length > 30)
            return await FunctionResponses.JsonAsync(request, HttpStatusCode.BadRequest, new { message = "Type is required and must be 30 characters or fewer (for example Hero or Equipment)." });
        if (input.Description?.Length > 500)
            return await FunctionResponses.JsonAsync(request, HttpStatusCode.BadRequest, new { message = "Description must be 500 characters or fewer." });
        if (await db.Assets.AnyAsync(asset => asset.Name == name && asset.Type == type))
            return await FunctionResponses.JsonAsync(request, HttpStatusCode.Conflict, new { message = "An asset with this name and type already exists." });

        var asset = new Asset { Name = name, Type = type, Description = input.Description?.Trim(), CreatedAt = DateTime.UtcNow };
        db.Assets.Add(asset);
        await db.SaveChangesAsync();
        return await FunctionResponses.JsonAsync(request, HttpStatusCode.Created, asset);
    }
}
