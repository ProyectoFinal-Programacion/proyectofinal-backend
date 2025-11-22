using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ManoVecinaAPI.Data;
using ManoVecinaAPI.DTOs.Orders;
using ManoVecinaAPI.Models;

namespace ManoVecinaAPI.Controllers;

[ApiController]
[Route("orders")]
public class OrdersController : ControllerBase
{
    private readonly AppDbContext _db;

    public OrdersController(AppDbContext db)
    {
        _db = db;
    }

    // ✔ CREAR ORDEN (CLIENTE CONTRATA A TRABAJADOR)
    [Authorize(Roles = "client")]
    [HttpPost]
    public IActionResult CreateOrder([FromBody] CreateOrderRequestDto dto)
    {
        var clientId = int.Parse(User.FindFirst("userId")!.Value);

        var gig = _db.Gigs.FirstOrDefault(g => g.Id == dto.GigId && g.IsActive);
        if (gig == null)
            return NotFound(new { message = "El gig no existe." });

        var worker = _db.Users.FirstOrDefault(u => u.Id == gig.WorkerId);
        if (worker == null)
            return NotFound(new { message = "Trabajador no encontrado." });

        var order = new Order
        {
            ClientId = clientId,
            WorkerId = gig.WorkerId,
            GigId = gig.Id,
            TotalAmount = gig.Price,
            Status = OrderStatus.Pending
        };

        _db.Orders.Add(order);
        _db.SaveChanges();

        return Ok(new { message = "Orden creada exitosamente.", orderId = order.Id });
    }

    // ✔ CLIENTE OBTIENE SUS ÓRDENES
    [Authorize(Roles = "client")]
    [HttpGet("my")]
    public IActionResult GetMyOrders()
    {
        var clientId = int.Parse(User.FindFirst("userId")!.Value);

        var orders = _db.Orders
            .Where(o => o.ClientId == clientId)
            .Select(o => new OrderResponseDto
            {
                Id = o.Id,
                ClientName = o.Client!.Name,
                WorkerName = o.Worker!.Name,
                GigTitle = o.Gig!.Title,
                TotalAmount = o.TotalAmount,
                Status = o.Status.ToString()
            })
            .ToList();

        return Ok(orders);
    }

    // ✔ TRABAJADOR OBTIENE SUS ÓRDENES RECIBIDAS
    [Authorize(Roles = "worker")]
    [HttpGet("received")]
    public IActionResult GetWorkerOrders()
    {
        var workerId = int.Parse(User.FindFirst("userId")!.Value);

        var orders = _db.Orders
            .Where(o => o.WorkerId == workerId)
            .Select(o => new OrderResponseDto
            {
                Id = o.Id,
                ClientName = o.Client!.Name,
                WorkerName = o.Worker!.Name,
                GigTitle = o.Gig!.Title,
                TotalAmount = o.TotalAmount,
                Status = o.Status.ToString()
            })
            .ToList();

        return Ok(orders);
    }

    // ✔ OBTENER UNA ORDEN POR ID (CLIENTE O WORKER)
    [Authorize]
    [HttpGet("{id}")]
    public IActionResult GetOrderById(int id)
    {
        var userId = int.Parse(User.FindFirst("userId")!.Value);

        var order = _db.Orders.FirstOrDefault(o => o.Id == id);
        if (order == null)
            return NotFound(new { message = "Orden no encontrada." });

        if (order.ClientId != userId && order.WorkerId != userId)
            return Forbid();

        return Ok(new OrderResponseDto
        {
            Id = order.Id,
            ClientName = order.Client!.Name,
            WorkerName = order.Worker!.Name,
            GigTitle = order.Gig!.Title,
            TotalAmount = order.TotalAmount,
            Status = order.Status.ToString()
        });
    }

    // ✔ CAMBIAR ESTADO (worker o cliente dependiendo el caso)
    [Authorize]
    [HttpPut("{id}/status")]
    public IActionResult UpdateStatus(int id, [FromQuery] OrderStatus status)
    {
        var userId = int.Parse(User.FindFirst("userId")!.Value);

        var order = _db.Orders.FirstOrDefault(o => o.Id == id);
        if (order == null)
            return NotFound(new { message = "Orden no encontrada." });

        // CLIENTE solo puede marcar Delivered/Completed/Cancelled
        // WORKER solo puede marcar InProgress/Delivered
        if (User.IsInRole("client") && order.ClientId != userId)
            return Forbid();

        if (User.IsInRole("worker") && order.WorkerId != userId)
            return Forbid();

        // Validaciones de flujo real
        if (order.Status == OrderStatus.Completed)
            return BadRequest(new { message = "La orden ya está completada." });

        order.Status = status;
        order.UpdatedAt = DateTime.UtcNow;

        _db.SaveChanges();

        return Ok(new { message = "Estado actualizado.", newStatus = status.ToString() });
    }
}
