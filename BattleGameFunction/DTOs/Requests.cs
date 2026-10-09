namespace BattleGameFunction.DTOs;

public class RegisterPlayerRequest
{
    public string? PlayerId { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Age { get; set; } = string.Empty;
    public int Level { get; set; }
    public string Email { get; set; } = string.Empty;
}

public class CreateAssetRequest
{
    public string? AssetId { get; set; }
    public string AssetName { get; set; } = string.Empty;
    public int LevelRequire { get; set; }
}

public class AssignAssetRequest
{
    public string PlayerId { get; set; } = string.Empty;
    public string AssetId { get; set; } = string.Empty;
}

public class PlayerAssetReportDto
{
    public int No { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public int Level { get; set; }
    public string Age { get; set; } = string.Empty;
    public string AssetName { get; set; } = string.Empty;
}

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }

    public static ApiResponse<T> Ok(T data, string message = "Success") =>
        new() { Success = true, Message = message, Data = data };

    public static ApiResponse<T> Fail(string message) =>
        new() { Success = false, Message = message, Data = default };
}
