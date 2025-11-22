using ManoVecinaAPI.DTOs.Gigs;

namespace ManoVecinaAPI.DTOs.Workers;

public class WorkerSearchResultDto
{
    public int WorkerId { get; set; }
    public string Name { get; set; } = "";
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    public double? DistanceKm { get; set; }
    public double? DurationMinutes { get; set; }

    public double AverageRating { get; set; }
    public int ReviewsCount { get; set; }

    public List<GigSummaryDto> Gigs { get; set; } = new();
}