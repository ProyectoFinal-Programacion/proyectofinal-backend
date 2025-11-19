namespace ManoVecinaAPI.Models;

public class Gig
{
    public int Id { get; set; }

    public int WorkerId { get; set; }
    public User? Worker { get; set; }

    public string Title { get; set; } = default!;
    public string Description { get; set; } = default!;
    public decimal Price { get; set; }
    public string Category { get; set; } = default!;
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}