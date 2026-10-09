using System.Net;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Data.SqlClient;

namespace BATTLEGAME;

public class CreateAsset
{
    [Function("CreateAsset")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
    {
        var input = await JsonSerializer.DeserializeAsync<AssetInput>(req.Body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        if (input == null || string.IsNullOrWhiteSpace(input.AssetName) || input.AssetName.Length > 64 || input.LevelRequire < 1)
        {
            var bad = req.CreateResponse(HttpStatusCode.BadRequest);
            await bad.WriteStringAsync("Invalid input. Please provide valid asset information.");
            return bad;
        }

        try
        {
            var connectionString = Environment.GetEnvironmentVariable("SqlConnectionString");
            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();
            const string sql = "INSERT INTO Assets (AssetName, LevelRequire) OUTPUT INSERTED.AssetId VALUES (@AssetName, @LevelRequire)";
            await using var cmd = new SqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@AssetName", input.AssetName);
            cmd.Parameters.AddWithValue("@LevelRequire", input.LevelRequire);
            var AssetId = (Guid)await cmd.ExecuteScalarAsync();
            var response = req.CreateResponse(HttpStatusCode.Created);
            await response.WriteStringAsync($"Asset created successfully with AssetId: {AssetId}");
            return response;

        }
        catch (SqlException ex) when (ex.Number == 2601 || ex.Number == 2627)
        {
            var conflict = req.CreateResponse(HttpStatusCode.Conflict);
            await conflict.WriteStringAsync("An asset with the same name already exists.");
            return conflict;
        }
    }
}
public class AssetInput
{
    public string AssetName { get; set; } = "";
    public int LevelRequire { get; set; } = 1;
}