using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ManoVecinaAPI.Data;
using ManoVecinaAPI.DTOs.Reviews;
using ManoVecinaAPI.Models;

namespace ManoVecinaAPI.Controllers;

[ApiController]
[Route("reviews")]
public class ReviewsController : ControllerBase
{
    private readonly AppDbContext _db;

    public ReviewsController(AppDbContext db)
    {
        _db = db;
    }

    // ✔ CREAR REVIEW (CLIENTE O WORKER)
    [Authorize]
    [HttpPost]
    public IActionResult CreateReview([FromBody] ReviewRequestDto dto)
    {
        var userId = int.Parse(User.FindFirst("userId")!.Value);

        var order = _db.Orders.FirstOrDefault(o => o.Id == dto.OrderId);
        if (order == null)
            return NotFound(new { message = "Orden no encontrada." });

        // Verificar que el que califica participó en la orden
        if (order.ClientId != userId && order.WorkerId != userId)
            return Forbid();

        // Verificar que no haya review duplicada
        var exists = _db.Reviews.Any(r => r.OrderId == dto.OrderId && r.FromUserId == userId);
        if (exists)
            return BadRequest(new { message = "Ya has calificado esta orden." });

        // Verificar que el rating sea válido
        if (dto.Rating < 1 || dto.Rating > 5)
            return BadRequest(new { message = "El rating debe ser entre 1 y 5." });

        // Crear review
        var review = new Review
        {
            OrderId = dto.OrderId,
            FromUserId = dto.FromUserId,
            ToUserId = dto.ToUserId,
            Rating = dto.Rating,
            Comment = dto.Comment,
            CreatedAt = DateTime.UtcNow
        };

        _db.Reviews.Add(review);
        _db.SaveChanges();

        return Ok(new { message = "Review creada exitosamente." });
    }

    // ✔ OBTENER REVIEWS DE UN USUARIO
    [HttpGet("user/{userId}")]
    public IActionResult GetUserReviews(int userId)
    {
        var reviews = _db.Reviews
            .Where(r => r.ToUserId == userId)
            .Select(r => new ReviewResponseDto
            {
                Id = r.Id,
                FromName = r.FromUser!.Name,
                ToName = r.ToUser!.Name,
                Rating = r.Rating,
                Comment = r.Comment
            })
            .ToList();

        return Ok(reviews);
    }

    // ✔ OBTENER PROMEDIO DE RATING
    [HttpGet("rating/{userId}")]
    public IActionResult GetUserRating(int userId)
    {
        var reviews = _db.Reviews.Where(r => r.ToUserId == userId);

        if (!reviews.Any())
            return Ok(new { rating = 0.0, count = 0 });

        var avg = reviews.Average(r => r.Rating);
        var count = reviews.Count();

        return Ok(new { rating = avg, count });
    }
}
