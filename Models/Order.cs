using System.ComponentModel.DataAnnotations;
using ManoVecinaAPI.Models.Enums;

namespace ManoVecinaAPI.Models;

public class Order
{
    public int Id { get; set; }

    public int ClientId { get; set; }
    public User Client { get; set; } = null!;

    public int WorkerId { get; set; }
    public User Worker { get; set; } = null!;

    public int? GigId { get; set; }
    public Gig? Gig { get; set; }

    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(300)]
    public string? Address { get; set; }

    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    public decimal TotalPrice { get; set; }

    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public DateTime? ScheduledAt { get; set; }

    public ICollection<Review> Reviews { get; set; } = new List<Review>();
}
