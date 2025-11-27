using ManoVecinaAPI.Data;
using ManoVecinaAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ManoVecinaAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly AppDbContext _db;

    public AdminController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet("users")]
    public async Task<ActionResult<IEnumerable<object>>> GetUsers()
    {
        var users = await _db.Users
            .Select(u => new
            {
                u.Id,
                u.Name,
                u.Email,
                u.Role,
                u.IsBanned,
                u.IsSuspended,
                u.CreatedAt
            })
            .ToListAsync();

        return Ok(users);
    }

    [HttpPut("users/{id}/ban")]
    public async Task<IActionResult> BanUser(int id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound();

        user.IsBanned = true;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPut("users/{id}/unban")]
    public async Task<IActionResult> UnbanUser(int id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound();

        user.IsBanned = false;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<object>> GetDashboard()
    {
        var totalUsers = await _db.Users.CountAsync();
        var totalWorkers = await _db.Users.CountAsync(u => u.Role == UserRole.Worker);
        var totalClients = await _db.Users.CountAsync(u => u.Role == UserRole.Client);
        var totalOrders = await _db.Orders.CountAsync();
        var completedOrders = await _db.Orders.CountAsync(o => o.Status == Models.Enums.OrderStatus.Completed);

        return Ok(new
        {
            totalUsers,
            totalWorkers,
            totalClients,
            totalOrders,
            completedOrders
        });
    }
}
