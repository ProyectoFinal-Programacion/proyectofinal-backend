using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using ManoVecinaAPI.Data;
using ManoVecinaAPI.DTOs.Auth;
using ManoVecinaAPI.Models;

namespace ManoVecinaAPI.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    public AuthController(AppDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    [HttpPost("register")]
    public IActionResult Register([FromBody] RegisterRequestDto dto)
    {
        if (_db.Users.Any(u => u.Email == dto.Email))
            return Conflict(new { message = "Email ya registrado" });

        if (dto.Role == "admin")
        {
            var adminKey = _config["AdminCreationKey"];
            if (dto.AdminKey != adminKey)
                return Unauthorized(new { message = "Clave de creación de admin inválida" });
        }

        var user = new User
        {
            Name = dto.Name,
            Email = dto.Email,
            Role = dto.Role,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
        };

        _db.Users.Add(user);
        _db.SaveChanges();

        return Ok(new { message = "Usuario registrado exitosamente" });
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequestDto dto)
    {
        var user = _db.Users.FirstOrDefault(u => u.Email == dto.Email);
        if (user == null)
            return Unauthorized(new { message = "Credenciales inválidas" });

        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return Unauthorized(new { message = "Credenciales inválidas" });

        var jwt = GenerateJwt(user);

        return Ok(new AuthResponseDto
        {
            Token = jwt,
            Role = user.Role,
            UserId = user.Id,
            Name = user.Name
        });
    }

    private string GenerateJwt(User user)
    {
        var jwt = _config.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim("userId", user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var token = new JwtSecurityToken(
            issuer: jwt["Issuer"],
            audience: jwt["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(5),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
