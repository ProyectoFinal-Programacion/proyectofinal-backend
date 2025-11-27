using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;

namespace ManoVecinaAPI.Services;

public class GeoapifyService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public GeoapifyService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiKey = configuration.GetSection("Geoapify")["ApiKey"] ?? string.Empty;
    }

    // Ejemplo simple para reverse geocoding
    public async Task<string?> ReverseGeocodeAsync(double lat, double lon)
    {
        if (string.IsNullOrEmpty(_apiKey))
            return null;

        var url = $"https://api.geoapify.com/v1/geocode/reverse?lat={lat}&lon={lon}&apiKey={_apiKey}";
        var response = await _httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode)
            return null;

        var json = await response.Content.ReadFromJsonAsync<dynamic>();
        try
        {
            return json?["features"]?[0]?["properties"]?["formatted"]?.ToString();
        }
        catch
        {
            return null;
        }
    }
}
