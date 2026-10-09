using System.Net;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Data.SqlClient;

namespace BATTLEGAME;

public class RegisterPlayer
{
    [Function("RegisterPlayer")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
    {
        var input = await JsonSerializer.DeserializeAsync<PlayerInput>(req.Body, new JsonSerializerOptions {PropertyNameCaseInsensitive = true});
        if (input == null || string.IsNullOrWhiteSpace(input.PlayerName) || string.IsNullOrWhiteSpace(input.Email) || string.IsNullOrWhiteSpace(input.Fullname) || input.Age <= 0 || input.Level < 1)
        {
            var bad = req.CreateResponse(HttpStatusCode.BadRequest);
            await bad.WriteStringAsync("Invalid input. Please provide valid player information.");
            return bad;
        }

        try
        {
            var connectionString = Environment.GetEnvironmentVariable("SqlConnectionString");
            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            const string sql = "INSERT INTO Players (PlayerName, Email, Fullname, Age, Level) OUTPUT INSERTED.PlayerId VALUES (@PlayerName, @Email, @Fullname, @Age, @Level)";
            await using var cmd = new SqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@PlayerName", input.PlayerName);
            cmd.Parameters.AddWithValue("@Email", input.Email);
            cmd.Parameters.AddWithValue("@Fullname", input.Fullname);
            cmd.Parameters.AddWithValue("@Age", input.Age);
            cmd.Parameters.AddWithValue("@Level", input.Level);

            var PlayerId = (Guid)await cmd.ExecuteScalarAsync();
            var response = req.CreateResponse(HttpStatusCode.Created);
            await response.WriteStringAsync($"Player registered successfully with PlayerId: {PlayerId}");
            return response;
        }
        catch (SqlConnection ex) when (ex.Number == 2601 || ex.Number == 2627)
        {
            var conflict = req.CreateResponse(HttpStatusCode.Conflict);
            await conflict.WriteStringAsync("A player with the same email already exists.");
            return conflict;
        }
    }
}

public class PlayerInput
{
    public string PlayerName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Fullname { get; set; } = "";
    public int Age { get; set; }
    public int Level { get; set; } = 1;
}