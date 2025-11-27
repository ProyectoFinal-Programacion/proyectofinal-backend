namespace ManoVecinaAPI.DTOs.Gigs;

public class GigRequestDto
{
    public string Title { get; set; } = string.Empty;

    // ✔ Permite null, vacío o normal
    public string? Description { get; set; }

    public string? Category { get; set; }

    public decimal Price { get; set; }
}