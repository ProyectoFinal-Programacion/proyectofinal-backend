using System.Security.Claims;
using AutoMapper;
using ManoVecinaAPI.Data;
using ManoVecinaAPI.DTOs.Reviews;
using ManoVecinaAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ManoVecinaAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReviewsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IMapper _mapper;

    public ReviewsController(AppDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // ============================================================
    // CREATE REVIEW (CLIENTE O TRABAJADOR)
    // ============================================================
    [HttpPost]
    public async Task<ActionResult<ReviewResponseDto>> CreateReview(ReviewRequestDto dto)
    {
        int fromUserId = GetUserId();

        // Cargar orden con relaciones
        var order = await _db.Orders
            .Include(o => o.Reviews)
            .FirstOrDefaultAsync(o => o.Id == dto.OrderId);

        if (order == null)
            return BadRequest(new { error = "Order not found" });

        // REGLA 1 → La orden debe estar COMPLETED
        if (order.Status != Models.Enums.OrderStatus.Completed)
            return BadRequest(new { error = "Order must be completed before reviewing" });

        // REGLA 2 → Solo cliente o trabajador pueden reseñar
        bool isClient = order.ClientId == fromUserId;
        bool isWorker = order.WorkerId == fromUserId;

        if (!isClient && !isWorker)
            return Forbid("Only client or worker involved can review this order.");

        // REGLA 3 → Solo se puede reseñar a la otra persona de la orden
        if (dto.ToUserId != order.ClientId && dto.ToUserId != order.WorkerId)
            return BadRequest(new { error = "Invalid target user for this order" });

        if (dto.ToUserId == fromUserId)
            return BadRequest(new { error = "You cannot review yourself" });

        // REGLA 4 → No permitir más de 1 review POR USUARIO por orden
        bool alreadyReviewed = await _db.Reviews.AnyAsync(r =>
            r.OrderId == dto.OrderId &&
            r.FromUserId == fromUserId
        );

        if (alreadyReviewed)
            return BadRequest(new { error = "You already reviewed this order" });

        // Crear review
        var review = new Review
        {
            OrderId = dto.OrderId,
            FromUserId = fromUserId,
            ToUserId = dto.ToUserId,
            Rating = dto.Rating,
            Comment = dto.Comment
        };

        _db.Reviews.Add(review);
        await _db.SaveChangesAsync();

        return Ok(_mapper.Map<ReviewResponseDto>(review));
    }

    // ============================================================
    // GET REVIEWS FOR USER
    // ============================================================
    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IEnumerable<ReviewResponseDto>>> GetReviewsForUser(int userId)
    {
        var reviews = await _db.Reviews
            .Where(r => r.ToUserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return Ok(_mapper.Map<IEnumerable<ReviewResponseDto>>(reviews));
    }

    // ============================================================
    // AVERAGE RATING FOR WORKER / CLIENT
    // ============================================================
    [HttpGet("user/{userId}/average")]
    public async Task<ActionResult<double>> GetUserAverageRating(int userId)
    {
        var ratings = await _db.Reviews
            .Where(r => r.ToUserId == userId)
            .Select(r => (double)r.Rating)
            .ToListAsync();

        if (!ratings.Any())
            return Ok(0);

        return Ok(ratings.Average());
    }
}
