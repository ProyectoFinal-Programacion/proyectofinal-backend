using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ManoVecinaAPI.Data;
using ManoVecinaAPI.DTOs.Workers;
using ManoVecinaAPI.DTOs.Gigs;
using ManoVecinaAPI.Services;

namespace ManoVecinaAPI.Controllers;

[ApiController]
[Route("workers")]
public class WorkersController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly GeoapifyRoutingService _geo;

    public WorkersController(AppDbContext db, GeoapifyRoutingService geo)
    {
        _db = db;
        _geo = geo;
    }

    // ✔ BÚSQUEDA COMPLETA DE TRABAJADORES
    [HttpGet("search")]
    public async Task<IActionResult> SearchWorkers(
        [FromQuery] double? lat,
        [FromQuery] double? lng,
        [FromQuery] string? category,
        [FromQuery] string? sortBy = "distance",
        [FromQuery] int? minRating = null,
        [FromQuery] decimal? minPrice = null,
        [FromQuery] decimal? maxPrice = null,
        [FromQuery] bool? available = null
    )
    {
        // 1️⃣ Base query: solo trabajadores
        var query = _db.Users
            .Where(u => u.Role == "worker")
            .Include(u => u.Gigs)
            .AsQueryable();

        // 2️⃣ Filtro por disponibilidad
        if (available != null)
            query = query.Where(u => u.IsAvailable == available);

        // 3️⃣ Filtro por categoría
        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(u => u.Gigs.Any(g => g.Category == category && g.IsActive));

        // 4️⃣ Filtro por rango de precios
        if (minPrice != null)
            query = query.Where(u => u.Gigs.Any(g => g.Price >= minPrice));

        if (maxPrice != null)
            query = query.Where(u => u.Gigs.Any(g => g.Price <= maxPrice));

        var workers = query.ToList();

        List<WorkerSearchResultDto> result = new();

        // 5️⃣ Calcular ratings
        var ratings = _db.Reviews
            .GroupBy(r => r.ToUserId)
            .Select(g => new
            {
                UserId = g.Key,
                Avg = g.Average(r => r.Rating),
                Count = g.Count()
            })
            .ToDictionary(k => k.UserId, v => (v.Avg, v.Count));

        // 6️⃣ Calcular distancia si lat/lng están disponibles
        foreach (var w in workers)
        {
            double? distanceKm = null;
            double? durationMin = null;

            if (lat != null && lng != null && w.Latitude != null && w.Longitude != null)
            {
                var (dKm, dMin) = await _geo.GetDrivingRouteAsync(
                    lat.Value, lng.Value,
                    w.Latitude.Value, w.Longitude.Value
                );

                distanceKm = dKm;
                durationMin = dMin;
            }

            var avgRating = ratings.ContainsKey(w.Id) ? ratings[w.Id].Avg : 0.0;
            var count = ratings.ContainsKey(w.Id) ? ratings[w.Id].Count : 0;

            result.Add(new WorkerSearchResultDto
            {
                WorkerId = w.Id,
                Name = w.Name,
                Latitude = w.Latitude,
                Longitude = w.Longitude,
                DistanceKm = distanceKm,
                DurationMinutes = durationMin,
                AverageRating = avgRating,
                ReviewsCount = count,
                Gigs = w.Gigs.Where(g => g.IsActive)
                    .Select(g => new GigSummaryDto
                    {
                        GigId = g.Id,
                        Title = g.Title,
                        Category = g.Category,
                        Price = g.Price
                    }).ToList()
            });
        }

        // 7️⃣ Filtros por rating
        if (minRating != null)
            result = result.Where(r => r.AverageRating >= minRating).ToList();

        // 8️⃣ Ordenamiento
        result = sortBy?.ToLower() switch
        {
            "rating" => result.OrderByDescending(r => r.AverageRating).ToList(),
            "price_low" => result.OrderBy(r => r.Gigs.Min(g => g.Price)).ToList(),
            "price_high" => result.OrderByDescending(r => r.Gigs.Max(g => g.Price)).ToList(),
            _ => result.OrderBy(r => r.DistanceKm ?? double.MaxValue).ToList()
        };

        return Ok(result);
    }

    // ✔ OBTENER WORKER POR ID (perfil público)
    [HttpGet("{id}")]
    public IActionResult GetWorker(int id)
    {
        var worker = _db.Users
            .Where(u => u.Id == id && u.Role == "worker")
            .Include(u => u.Gigs)
            .FirstOrDefault();

        if (worker == null)
            return NotFound(new { message = "Trabajador no encontrado." });

        return Ok(new
        {
            worker.Id,
            worker.Name,
            worker.Email,
            worker.Latitude,
            worker.Longitude,
            worker.IsAvailable,
            Gigs = worker.Gigs.Where(g => g.IsActive)
                .Select(g => new GigSummaryDto
                {
                    GigId = g.Id,
                    Title = g.Title,
                    Category = g.Category,
                    Price = g.Price
                })
        });
    }
}
