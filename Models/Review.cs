using System.ComponentModel.DataAnnotations;

namespace ManoVecinaAPI.Models;

public class Review
{
    public int Id { get; set; }

    public int FromUserId { get; set; }
    public User FromUser { get; set; } = null!;

    public int ToUserId { get; set; }
    public User ToUser { get; set; } = null!;

    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;

    [Range(1, 5)]
    public int Rating { get; set; }

    [MaxLength(1000)]
    public string? Comment { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
