namespace ManoVecinaAPI.DTOs.Gigs;

public class GigSummaryDto
{
    public int GigId { get; set; }
    public string Title { get; set; } = "";
    public string Category { get; set; } = "";
    public decimal Price { get; set; }
}