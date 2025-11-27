using AutoMapper;
using ManoVecinaAPI.Data;
using ManoVecinaAPI.DTOs.Workers;
using ManoVecinaAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ManoVecinaAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WorkersController : ControllerBase
{
    private readonly AppDbContext _db;

    public WorkersController(AppDbContext db)
    {
        _db = db;
    }

    // ========================================================
    // SEARCH WORKERS
    // ========================================================
    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<WorkerSearchResultDto>>> SearchWorkers(
        [FromQuery] double lat,
        [FromQuery] double lon,
        [FromQuery] double radiusKm = 10)
    {
        var workers = await _db.Users
            .Where(u => u.Role == UserRole.Worker &&
                        u.Latitude.HasValue &&
                        u.Longitude.HasValue &&
                        !u.IsBanned)
            .ToListAsync();

        var workerIds = workers.Select(w => w.Id).ToList();

        var reviews = await _db.Reviews
            .Where(r => workerIds.Contains(r.ToUserId))
            .GroupBy(r => r.ToUserId)
            .Select(g => new { WorkerId = g.Key, AvgRating = g.Average(r => r.Rating) })
            .ToListAsync();

        var reviewDict = reviews.ToDictionary(r => r.WorkerId, r => r.AvgRating);

        var results = new List<WorkerSearchResultDto>();

        foreach (var w in workers)
        {
            var distance = CalculateDistanceKm(lat, lon, w.Latitude!.Value, w.Longitude!.Value);
            if (distance <= radiusKm)
            {
                results.Add(new WorkerSearchResultDto
                {
                    WorkerId = w.Id,
                    Name = w.Name,
                    ImageUrl = w.ImageUrl,
                    BasePrice = w.BasePrice,
                    DistanceKm = distance,
                    AverageRating = reviewDict.TryGetValue(w.Id, out var rating) ? rating : 0,
                    Latitude = w.Latitude,
                    Longitude = w.Longitude
                });
            }
        }

        return Ok(results.OrderBy(r => r.DistanceKm));
    }

    // ========================================================
    // GET WORKER PROFILE + GIGS
    // ========================================================
    [HttpGet("{id}/profile")]
    public async Task<ActionResult<object>> GetWorkerProfile(int id)
    {
        var worker = await _db.Users
            .FirstOrDefaultAsync(u => u.Id == id && u.Role == UserRole.Worker);

        if (worker == null)
            return NotFound();

        var gigs = await _db.Gigs
            .Where(g => g.WorkerId == id)
            .ToListAsync();

        var avgRating = await _db.Reviews
            .Where(r => r.ToUserId == id)
            .Select(r => (double)r.Rating)
            .DefaultIfEmpty(0)
            .AverageAsync();

        return Ok(new
        {
            worker.Id,
            worker.Name,
            worker.ImageUrl,
            worker.Bio,
            worker.BasePrice,
            worker.Address,
            worker.Latitude,
            worker.Longitude,
            AverageRating = avgRating,

            // ⬇ GIGS con múltiples imágenes
            Gigs = gigs.Select(g => new
            {
                g.Id,
                g.Title,
                g.Description,
                g.Category,
                g.Price,
                ImageUrls = g.ImageUrls   // ← CAMBIO IMPORTANTE
            })
        });
    }

    // ========================================================
    // UTILS
    // ========================================================
    private static double CalculateDistanceKm(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371;
        var dLat = DegreesToRadians(lat2 - lat1);
        var dLon = DegreesToRadians(lon2 - lon1);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(DegreesToRadians(lat1)) * Math.Cos(DegreesToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }

    private static double DegreesToRadians(double deg) => deg * (Math.PI / 180);
}
