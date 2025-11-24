namespace ManoVecinaAPI.DTOs.Workers
{
    public class WorkerSearchResultDto
    {
        public int WorkerId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public decimal? BasePrice { get; set; }
        public double? DistanceKm { get; set; }
        public double AverageRating { get; set; }

        // 👇 NUEVO: ubicación real del trabajador
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }
}