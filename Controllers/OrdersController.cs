using System.Security.Claims;
using AutoMapper;
using ManoVecinaAPI.Data;
using ManoVecinaAPI.DTOs.Orders;
using ManoVecinaAPI.Models;
using ManoVecinaAPI.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ManoVecinaAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IMapper _mapper;

    public OrdersController(AppDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // ============================================================
    // CREATE ORDER
    // ============================================================
    [HttpPost]
    public async Task<ActionResult<OrderResponseDto>> CreateOrder(CreateOrderRequestDto dto)
    {
        var clientId = GetUserId();

        var worker = await _db.Users
            .FirstOrDefaultAsync(u => u.Id == dto.WorkerId && u.Role == UserRole.Worker);

        if (worker == null)
            return BadRequest("Worker not found");

        Gig? gig = null;
        if (dto.GigId.HasValue)
        {
            gig = await _db.Gigs.FindAsync(dto.GigId.Value);
            if (gig == null) return BadRequest("Gig not found");
        }

        var order = new Order
        {
            ClientId = clientId,
            WorkerId = worker.Id,
            GigId = dto.GigId,
            Description = dto.Description,
            Address = dto.Address,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            TotalPrice = dto.TotalPrice,
            ScheduledAt = dto.ScheduledAt,
            Status = OrderStatus.Pending
        };

        _db.Orders.Add(order);
        await _db.SaveChangesAsync();

        // 🔥 Recargar con relaciones incluidas
        var fullOrder = await _db.Orders
            .Include(o => o.Client)
            .Include(o => o.Worker)
            .Include(o => o.Gig)
            .FirstAsync(o => o.Id == order.Id);

        return _mapper.Map<OrderResponseDto>(fullOrder);
    }

    // ============================================================
    // CLIENT'S ORDERS
    // ============================================================
    [HttpGet("my")]
    public async Task<ActionResult<IEnumerable<OrderResponseDto>>> GetMyOrders()
    {
        var clientId = GetUserId();

        var orders = await _db.Orders
            .Include(o => o.Client)
            .Include(o => o.Worker)
            .Include(o => o.Gig)
            .Where(o => o.ClientId == clientId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return Ok(_mapper.Map<IEnumerable<OrderResponseDto>>(orders));
    }

    // ============================================================
    // WORKER RECEIVED ORDERS
    // ============================================================
    [HttpGet("received")]
    [Authorize(Roles = "Worker")]
    public async Task<ActionResult<IEnumerable<OrderResponseDto>>> GetReceivedOrders()
    {
        var workerId = GetUserId();

        var orders = await _db.Orders
            .Include(o => o.Client)
            .Include(o => o.Worker)
            .Include(o => o.Gig)
            .Where(o => o.WorkerId == workerId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return Ok(_mapper.Map<IEnumerable<OrderResponseDto>>(orders));
    }

    // ============================================================
    // UPDATE STATUS (Only Worker)
    // ============================================================
    [HttpPut("{id}/status")]
    [Authorize(Roles = "Worker")]
    public async Task<IActionResult> UpdateStatus(int id, [FromQuery] OrderStatus status)
    {
        var workerId = GetUserId();

        var order = await _db.Orders
            .Include(o => o.Client)
            .Include(o => o.Worker)
            .Include(o => o.Gig)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null) return NotFound();
        if (order.WorkerId != workerId) return Forbid();

        order.Status = status;
        order.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return NoContent();
    }
}
