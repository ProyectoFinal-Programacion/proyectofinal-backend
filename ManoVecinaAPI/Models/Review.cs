namespace ManoVecinaAPI.Models;

public class Review
{
    public int Id { get; set; }

    public int OrderId { get; set; }
    public Order? Order { get; set; }

    public int FromUserId { get; set; }
    public User? FromUser { get; set; }

    public int ToUserId { get; set; }
    public User? ToUser { get; set; }

    public int Rating { get; set; } // 1-5
    public string Comment { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}