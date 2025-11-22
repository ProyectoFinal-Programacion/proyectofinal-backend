namespace ManoVecinaAPI.DTOs.Users;

public class UserProfileDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string Role { get; set; } = "";

    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public bool IsAvailable { get; set; }
}