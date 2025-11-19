namespace ManoVecinaAPI.Models;

public enum OrderStatus
{
    Pending,
    InProgress,
    Delivered,
    Completed,
    Cancelled
}

public class Order
{
    public int Id { get; set; }

    public int ClientId { get; set; }
    public User? Client { get; set; }

    public int WorkerId { get; set; }
    public User? Worker { get; set; }

    public int GigId { get; set; }
    public Gig? Gig { get; set; }

    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public decimal TotalAmount { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}