using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ManoVecinaAPI.Data;
using ManoVecinaAPI.DTOs.Gigs;
using ManoVecinaAPI.Models;

namespace ManoVecinaAPI.Controllers;

[ApiController]
[Route("gigs")]
public class GigsController : ControllerBase
{
    private readonly AppDbContext _db;

    public GigsController(AppDbContext db)
    {
        _db = db;
    }

    // ✔ OBTENER TODOS LOS GIGS (público)
    [HttpGet]
    public IActionResult GetAllGigs()
    {
        var gigs = _db.Gigs
            .Where(g => g.IsActive)
            .Select(g => new GigResponseDto
            {
                Id = g.Id,
                WorkerId = g.WorkerId,
                WorkerName = g.Worker!.Name,
                Title = g.Title,
                Description = g.Description,
                Price = g.Price,
                Category = g.Category
            })
            .ToList();

        return Ok(gigs);
    }

    // ✔ OBTENER GIGS POR CATEGORÍA
    [HttpGet("category/{category}")]
    public IActionResult GetByCategory(string category)
    {
        var gigs = _db.Gigs
            .Where(g => g.Category == category && g.IsActive)
            .Select(g => new GigResponseDto
            {
                Id = g.Id,
                WorkerId = g.WorkerId,
                WorkerName = g.Worker!.Name,
                Title = g.Title,
                Description = g.Description,
                Price = g.Price,
                Category = g.Category
            })
            .ToList();

        return Ok(gigs);
    }

    // ✔ OBTENER GIGS DE UN WORKER ESPECÍFICO
    [HttpGet("worker/{workerId}")]
    public IActionResult GetWorkerGigs(int workerId)
    {
        var gigs = _db.Gigs
            .Where(g => g.WorkerId == workerId && g.IsActive)
            .Select(g => new GigResponseDto
            {
                Id = g.Id,
                WorkerId = g.WorkerId,
                WorkerName = g.Worker!.Name,
                Title = g.Title,
                Description = g.Description,
                Price = g.Price,
                Category = g.Category
            })
            .ToList();

        return Ok(gigs);
    }

    // ✔ CREAR GIG (solo worker)
    [Authorize(Roles = "worker")]
    [HttpPost]
    public IActionResult CreateGig([FromBody] GigRequestDto dto)
    {
        var workerId = int.Parse(User.FindFirst("userId")!.Value);

        var gig = new Gig
        {
            WorkerId = workerId,
            Title = dto.Title,
            Description = dto.Description,
            Price = dto.Price,
            Category = dto.Category
        };

        _db.Gigs.Add(gig);
        _db.SaveChanges();

        return Ok(new { message = "Gig creado exitosamente." });
    }

    // ✔ EDITAR GIG (solo owner)
    [Authorize(Roles = "worker")]
    [HttpPut("{id}")]
    public IActionResult UpdateGig(int id, [FromBody] GigRequestDto dto)
    {
        var workerId = int.Parse(User.FindFirst("userId")!.Value);
        var gig = _db.Gigs.FirstOrDefault(g => g.Id == id);

        if (gig == null)
            return NotFound(new { message = "Gig no encontrado." });

        if (gig.WorkerId != workerId)
            return Forbid();

        gig.Title = dto.Title;
        gig.Description = dto.Description;
        gig.Price = dto.Price;
        gig.Category = dto.Category;

        _db.SaveChanges();

        return Ok(new { message = "Gig actualizado exitosamente." });
    }

    // ✔ ELIMINAR GIG (soft delete)
    [Authorize(Roles = "worker")]
    [HttpDelete("{id}")]
    public IActionResult DeleteGig(int id)
    {
        var workerId = int.Parse(User.FindFirst("userId")!.Value);
        var gig = _db.Gigs.FirstOrDefault(g => g.Id == id);

        if (gig == null)
            return NotFound(new { message = "Gig no encontrado." });

        if (gig.WorkerId != workerId)
            return Forbid();

        gig.IsActive = false;
        _db.SaveChanges();

        return Ok(new { message = "Gig eliminado." });
    }
}
