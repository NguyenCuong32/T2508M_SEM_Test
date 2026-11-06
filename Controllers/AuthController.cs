using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace baithi.Controllers;

public class LoginRequest
{
    public string? Username { get; set; } = "admin";
    public string? Password { get; set; } = "123456";
}

[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public AuthController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// Tạo JWT Token mẫu để xác thực trong Swagger UI (Bấm Authorize và dán token này vào)
    /// </summary>
    /// <param name="request">Thông tin đăng nhập mẫu (mặc định username: admin, password: 123456)</param>
    [HttpPost("token")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GenerateToken([FromBody] LoginRequest? request)
    {
        var username = string.IsNullOrWhiteSpace(request?.Username) ? "admin" : request.Username;

        var jwtKey = _configuration["Jwt:Key"] ?? "SunriseHotel_Secret_Key_For_Jwt_Authentication_2026_Set02!";
        var issuer = _configuration["Jwt:Issuer"] ?? "SunriseHotel";
        var audience = _configuration["Jwt:Audience"] ?? "SunriseHotelApi";

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, "Admin"),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: creds
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        return Ok(new
        {
            token = tokenString,
            tokenType = "Bearer",
            expiresIn = 604800, // 7 days in seconds
            message = "Copy token này và bấm nút Authorize trên đầu trang Swagger UI để xác thực!"
        });
    }
}

