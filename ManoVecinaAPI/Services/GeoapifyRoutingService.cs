using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace ManoVecinaAPI.Services;

public class GeoapifyRoutingService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;

    public GeoapifyRoutingService(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _config = config;
        _httpClient.BaseAddress = new Uri("https://api.geoapify.com/");
    }

    public async Task<(double? distanceKm, double? durationMinutes)> GetDrivingRouteAsync(
        double originLat,
        double originLng,
        double destLat,
        double destLng)
    {
        var apiKey = _config["Geoapify:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
            return (null, null);

        var waypoints = $"{originLat},{originLng}|{destLat},{destLng}";
        var url = $"v1/routing?waypoints={waypoints}&mode=drive&apiKey={apiKey}";

        using var response = await _httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode)
            return (null, null);

        using var stream = await response.Content.ReadAsStreamAsync();
        using var doc = await JsonDocument.ParseAsync(stream);

        var features = doc.RootElement.GetProperty("features");
        if (features.GetArrayLength() == 0)
            return (null, null);

        var props = features[0].GetProperty("properties");
        var distanceMeters = props.GetProperty("distance").GetDouble();
        var timeSeconds = props.GetProperty("time").GetDouble();

        return (distanceMeters / 1000.0, timeSeconds / 60.0);
    }
}