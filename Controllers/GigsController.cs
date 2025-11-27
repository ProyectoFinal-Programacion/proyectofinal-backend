using System.Security.Claims;
using AutoMapper;
using ManoVecinaAPI.Data;
using ManoVecinaAPI.DTOs.Gigs;
using ManoVecinaAPI.Models;
using ManoVecinaAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ManoVecinaAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GigsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IMapper _mapper;
    private readonly ImageStorageService _imageService;

    public GigsController(AppDbContext db, IMapper mapper, ImageStorageService imageService)
    {
        _db = db;
        _mapper = mapper;
        _imageService = imageService;
    }

    private int GetUserId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // ============================================================
    // GET: TODOS LOS GIGS
    // ============================================================
    [HttpGet]
    public async Task<ActionResult<IEnumerable<GigResponseDto>>> GetAll([FromQuery] string? category = null)
    {
        var query = _db.Gigs.Include(g => g.Worker).AsQueryable();

        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(g => g.Category == category);

        var gigs = await query.ToListAsync();

        return Ok(_mapper.Map<IEnumerable<GigResponseDto>>(gigs));
    }

    // ============================================================
    // GET: GIGS POR WORKER
    // ============================================================
    [HttpGet("worker/{workerId}")]
    public async Task<ActionResult<IEnumerable<GigResponseDto>>> GetByWorker(int workerId)
    {
        var gigs = await _db.Gigs
            .Include(g => g.Worker)
            .Where(g => g.WorkerId == workerId)
            .ToListAsync();

        return Ok(_mapper.Map<IEnumerable<GigResponseDto>>(gigs));
    }

    // ============================================================
    // ⭐ GET: GIG INDIVIDUAL — NECESARIO PARA EDITAR
    // ============================================================
    [HttpGet("{id}")]
    public async Task<ActionResult<GigResponseDto>> GetGig(int id)
    {
        var gig = await _db.Gigs
            .Include(g => g.Worker)
            .FirstOrDefaultAsync(g => g.Id == id);

        if (gig == null)
            return NotFound();

        return Ok(_mapper.Map<GigResponseDto>(gig));
    }

    // ============================================================
    // CREATE GIG
    // ============================================================
    [HttpPost]
    [Authorize(Roles = "Worker")]
    public async Task<ActionResult<GigResponseDto>> CreateGig(GigRequestDto dto)
    {
        var workerId = GetUserId();

        var gig = new Gig
        {
            Title = dto.Title,
            Description = dto.Description,
            Category = dto.Category,
            Price = dto.Price,
            WorkerId = workerId,
            ImageUrls = new List<string>()
        };

        _db.Gigs.Add(gig);
        await _db.SaveChangesAsync();

        await _db.Entry(gig).Reference(g => g.Worker).LoadAsync();

        return CreatedAtAction(nameof(GetGig), new { id = gig.Id },
            _mapper.Map<GigResponseDto>(gig));
    }

    // ============================================================
    // UPDATE GIG
    // ============================================================
    [HttpPut("{id}")]
    [Authorize(Roles = "Worker")]
    public async Task<IActionResult> UpdateGig(int id, GigRequestDto dto)
    {
        var workerId = GetUserId();

        var gig = await _db.Gigs.FindAsync(id);
        if (gig == null) return NotFound();
        if (gig.WorkerId != workerId) return Forbid();

        gig.Title = dto.Title;
        gig.Description = dto.Description;
        gig.Category = dto.Category;
        gig.Price = dto.Price;

        await _db.SaveChangesAsync();
        return NoContent(); // Flutter luego llama a GET /Gigs/{id}
    }

    // ============================================================
    // DELETE GIG
    // ============================================================
    [HttpDelete("{id}")]
    [Authorize(Roles = "Worker")]
    public async Task<IActionResult> DeleteGig(int id)
    {
        var workerId = GetUserId();
        var gig = await _db.Gigs.FindAsync(id);
        if (gig == null) return NotFound();
        if (gig.WorkerId != workerId) return Forbid();

        _db.Gigs.Remove(gig);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    // ============================================================
    // UPLOAD IMAGE — ❤️ VERSIÓN COMPATIBLE CON SWAGGER
    // ============================================================
    [HttpPost("{id}/image")]
    [Authorize(Roles = "Worker")]
    public async Task<ActionResult<GigResponseDto>> UploadGigImage(
        int id,
        [FromForm] UploadGigImageDto dto)
    {
        var file = dto.File;

        if (file == null || file.Length == 0)
            return BadRequest("Debe enviar un archivo");

        var workerId = GetUserId();

        var gig = await _db.Gigs
            .Include(g => g.Worker)
            .FirstOrDefaultAsync(g => g.Id == id);

        if (gig == null) return NotFound();
        if (gig.WorkerId != workerId) return Forbid();

        var url = await _imageService.SaveGigImageAsync(id, file);

        gig.ImageUrls ??= new List<string>();
        gig.ImageUrls.Add(url);

        await _db.SaveChangesAsync();

        return Ok(_mapper.Map<GigResponseDto>(gig));
    }
}
