using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Data.SqlClient;

namespace BATTLEGAME;

public class GetAssetByPlayer
{
    [Function("GetAssetByPlayer")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestData req)
    {
        const string sql = @"
            SELECT p.PlayerName, p.[Level], p.Age, a.AssetName
            FROM Player p
            INNER JOIN PlayerAssets pa ON p.PlayerId = pa.PlayerId
            INNER JOIN Assets a ON pa.AssetId = a.AssetId
            ORDER BY p.PlayerName, a.AssetName";
        var rows = new List<object>();
        var connectionString = Environment.GetEnvironmentVariable("SqlConnectionString");
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        await using var cmd = new SqlCommand(sql, connection);
        await using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
           rows.Add(new
           {
               PlayerName = reader.GetString(0),
               Level = reader.GetInt32(1),
               Age = reader.GetInt32(2),
               AssetName = reader.GetString(3)
           });
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(rows);
            return response;
        }
    }
}