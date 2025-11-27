using System.Security.Claims;
using AutoMapper;
using ManoVecinaAPI.Data;
using ManoVecinaAPI.DTOs.Users;
using ManoVecinaAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ManoVecinaAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IMapper _mapper;
    private readonly ImageStorageService _imageService;

    public UsersController(AppDbContext db, IMapper mapper, ImageStorageService imageService)
    {
        _db = db;
        _mapper = mapper;
        _imageService = imageService;
    }

    private int GetUserId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // ============================================================
    // GET: MI PERFIL
    // ============================================================
    [HttpGet("me")]
    public async Task<ActionResult<UserProfileDto>> GetMe()
    {
        var id = GetUserId();
        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound();

        return _mapper.Map<UserProfileDto>(user);
    }

    // ============================================================
    // PUT: ACTUALIZAR MI PERFIL
    // ============================================================
    [HttpPut("me")]
    public async Task<ActionResult<UserProfileDto>> UpdateProfile(UpdateProfileRequest dto)
    {
        var id = GetUserId();
        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound();

        if (dto.Name != null) user.Name = dto.Name;
        if (dto.Phone != null) user.Phone = dto.Phone;
        if (dto.Bio != null) user.Bio = dto.Bio;
        if (dto.BasePrice.HasValue) user.BasePrice = dto.BasePrice;
        if (dto.Address != null) user.Address = dto.Address;
        if (dto.Latitude.HasValue) user.Latitude = dto.Latitude;
        if (dto.Longitude.HasValue) user.Longitude = dto.Longitude;

        await _db.SaveChangesAsync();

        return _mapper.Map<UserProfileDto>(user);
    }

    // ============================================================
    // POST: SUBIR AVATAR
    // ============================================================
    [HttpPost("me/avatar")]
    public async Task<ActionResult<UserProfileDto>> UploadAvatar(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("File is required");

        var id = GetUserId();
        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound();

        var url = await _imageService.SaveAvatarAsync(id, file);
        user.ImageUrl = url;

        await _db.SaveChangesAsync();
        return _mapper.Map<UserProfileDto>(user);
    }

    // ============================================================
    // 🔥 NUEVO ENDPOINT: VER PERFIL DE CUALQUIER USUARIO
    // ============================================================
    // GET /api/Users/{id}
    [HttpGet("{id:int}")]
    public async Task<ActionResult<UserProfileDto>> GetUserById(int id)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
            return NotFound();

        return _mapper.Map<UserProfileDto>(user);
    }
}
