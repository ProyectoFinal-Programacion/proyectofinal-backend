using System.ComponentModel.DataAnnotations;

namespace ManoVecinaAPI.Models;

public enum UserRole
{
    Client,
    Worker,
    Admin
}

public class User
{
    public int Id { get; set; }

    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(500)]
    public string PasswordHash { get; set; } = string.Empty;

    public UserRole Role { get; set; } = UserRole.Client;

    [MaxLength(30)]
    public string? Phone { get; set; }

    [MaxLength(500)]
    public string? Bio { get; set; }

    [MaxLength(300)]
    public string? ImageUrl { get; set; }

    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    [MaxLength(300)]
    public string? Address { get; set; }

    public decimal? BasePrice { get; set; }

    public bool IsBanned { get; set; }
    public bool IsSuspended { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Gig> Gigs { get; set; } = new List<Gig>();
    public ICollection<Order> OrdersAsClient { get; set; } = new List<Order>();
    public ICollection<Order> OrdersAsWorker { get; set; } = new List<Order>();
    public ICollection<Review> ReviewsFrom { get; set; } = new List<Review>();
    public ICollection<Review> ReviewsTo { get; set; } = new List<Review>();
}
