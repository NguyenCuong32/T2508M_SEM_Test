using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using ComicSystem.ViewModels;

namespace ComicSystem.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public AuthController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // POST: api/auth/login
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Tải thông tin tài khoản mẫu (Ví dụ: admin/admin123 hoặc user/user123)
            if ((model.Username == "admin" && model.Password == "admin123") ||
                (model.Username == "user" && model.Password == "user123"))
            {
                var jwtSettings = _configuration.GetSection("Jwt");
                var keyString = jwtSettings["Key"] ?? "ManhConDepTraiNhatTheGioiDungKhongMoiNguoi!";
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var expireMinutes = int.TryParse(jwtSettings["ExpireMinutes"], out var mins) ? mins : 60;
                var expiration = DateTime.UtcNow.AddMinutes(expireMinutes);

                var claims = new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, model.Username),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(ClaimTypes.Name, model.Username),
                    new Claim(ClaimTypes.Role, model.Username == "admin" ? "Admin" : "User")
                };

                var token = new JwtSecurityToken(
                    issuer: jwtSettings["Issuer"] ?? "ComicSystemServer",
                    audience: jwtSettings["Audience"] ?? "ComicSystemClient",
                    claims: claims,
                    expires: expiration,
                    signingCredentials: creds
                );

                var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

                return Ok(new AuthResponseDto
                {
                    Token = tokenString,
                    Expiration = expiration,
                    Username = model.Username,
                    Message = "Đăng nhập thành công!"
                });
            }

            return Unauthorized(new { message = "Tên đăng nhập hoặc mật khẩu không chính xác!" });
        }
    }
}
