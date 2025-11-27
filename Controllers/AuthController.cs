using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AutoMapper;
using ManoVecinaAPI.Data;
using ManoVecinaAPI.DTOs.Auth;
using ManoVecinaAPI.DTOs.Users;
using ManoVecinaAPI.Models;
using ManoVecinaAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace ManoVecinaAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IMapper _mapper;
    private readonly IConfiguration _config;
    private readonly FirebaseService _firebaseService;

    public AuthController(AppDbContext db, IMapper mapper, IConfiguration config, FirebaseService firebaseService)
    {
        _db = db;
        _mapper = mapper;
        _config = config;
        _firebaseService = firebaseService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register(RegisterRequestDto dto)
    {
        if (await _db.Users.AnyAsync(u => u.Email == dto.Email))
        {
            return BadRequest("Email already in use");
        }

        var user = new User
        {
            Name = dto.Name,
            Email = dto.Email,
            PasswordHash = HashPassword(dto.Password),
            Role = dto.Role,
            Phone = dto.Phone
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var token = GenerateJwtToken(user, out var expiresAt);

        return new AuthResponseDto
        {
            Token = token,
            ExpiresAt = expiresAt,
            User = _mapper.Map<UserProfileDto>(user)
        };
    }

    [HttpPost("register-admin")]
    public async Task<ActionResult<AuthResponseDto>> RegisterAdmin(RegisterRequestDto dto, [FromQuery] string adminKey)
    {
        var configKey = _config.GetSection("Admin")["RegistrationKey"];
        if (configKey == null || adminKey != configKey)
        {
            return Unauthorized("Invalid admin registration key.");
        }

        dto.Role = UserRole.Admin;
        return await Register(dto);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginRequestDto dto)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (user == null)
        {
            return Unauthorized("Invalid credentials");
        }

        if (!VerifyPassword(dto.Password, user.PasswordHash))
        {
            return Unauthorized("Invalid credentials");
        }

        if (user.IsBanned || user.IsSuspended)
        {
            return Forbid("User is banned or suspended");
        }

        var token = GenerateJwtToken(user, out var expiresAt);

        return new AuthResponseDto
        {
            Token = token,
            ExpiresAt = expiresAt,
            User = _mapper.Map<UserProfileDto>(user)
        };
    }

    [HttpPost("firebase-token")]
    public async Task<ActionResult<string>> GetFirebaseToken()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized();

        var token = await _firebaseService.CreateCustomTokenAsync(userId);
        return Ok(token);
    }

    private string GenerateJwtToken(User user, out DateTime expiresAt)
    {
        var jwtSection = _config.GetSection("Jwt");
        var key = jwtSection.GetValue<string>("Key") ?? "CHANGE_THIS_KEY";
        var issuer = jwtSection.GetValue<string>("Issuer");
        var audience = jwtSection.GetValue<string>("Audience");
        var expiresInMinutes = jwtSection.GetValue<int>("ExpiresInMinutes");

        var keyBytes = Encoding.UTF8.GetBytes(key);
        var creds = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        expiresAt = DateTime.UtcNow.AddMinutes(expiresInMinutes);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string HashPassword(string password)
    {
        using var sha = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        var hashBytes = sha.ComputeHash(bytes);
        return Convert.ToBase64String(hashBytes);
    }

    private static bool VerifyPassword(string password, string hash)
    {
        var hashedInput = HashPassword(password);
        return hashedInput == hash;
    }
}
