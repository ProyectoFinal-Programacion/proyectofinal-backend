using System.ComponentModel.DataAnnotations;

namespace ManoVecinaAPI.Models;

public class Gig
{
    public int Id { get; set; }

    [MaxLength(150)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Category { get; set; } = string.Empty;

    public decimal Price { get; set; }

    // ⬇ NUEVO CAMPO
    public List<string> ImageUrls { get; set; } = new();

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int WorkerId { get; set; }
    public User Worker { get; set; } = null!;

    public ICollection<Order> Orders { get; set; } = new List<Order>();
}