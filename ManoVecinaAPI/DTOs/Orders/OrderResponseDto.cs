using ManoVecinaAPI.Models.Enums;

namespace ManoVecinaAPI.DTOs.Orders;

public class OrderResponseDto
{
    public int Id { get; set; }

    // -----------------------------
    // CLIENTE
    // -----------------------------
    public int ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;

    // -----------------------------
    // TRABAJADOR
    // -----------------------------
    public int WorkerId { get; set; }
    public string WorkerName { get; set; } = string.Empty;

    // -----------------------------
    // GIG (opcional)
    // -----------------------------
    public int? GigId { get; set; }
    public string GigTitle { get; set; } = string.Empty;
    public string GigCategory { get; set; } = string.Empty;

    // -----------------------------
    // DETALLES DE LA ORDEN
    // -----------------------------
    public string Description { get; set; } = string.Empty;

    public string? Address { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    public decimal TotalPrice { get; set; }
    public OrderStatus Status { get; set; }

    // -----------------------------
    // FECHAS
    // -----------------------------
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}