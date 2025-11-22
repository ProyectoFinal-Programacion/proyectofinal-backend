namespace ManoVecinaAPI.DTOs.Gigs;

public class GigResponseDto
{
    public int Id { get; set; }
    public int WorkerId { get; set; }
    public string WorkerName { get; set; } = "";
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public decimal Price { get; set; }
    public string Category { get; set; } = "";
}