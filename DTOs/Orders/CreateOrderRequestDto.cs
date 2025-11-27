namespace ManoVecinaAPI.DTOs.Orders;

public class CreateOrderRequestDto
{
    public int WorkerId { get; set; }

    public int? GigId { get; set; }

    public string Description { get; set; } = string.Empty;

    public string? Address { get; set; }

    public double? Latitude { get; set; }

    public double? Longitude { get; set; }

    public decimal TotalPrice { get; set; }

    public DateTime? ScheduledAt { get; set; }
}