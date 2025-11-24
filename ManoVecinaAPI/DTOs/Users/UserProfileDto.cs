using ManoVecinaAPI.Models;

namespace ManoVecinaAPI.DTOs.Users;

public class UserProfileDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public string? Phone { get; set; }
    public string? Bio { get; set; }
    public string? ImageUrl { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? Address { get; set; }
    public decimal? BasePrice { get; set; }
    public bool IsBanned { get; set; }
    public bool IsSuspended { get; set; }
}
