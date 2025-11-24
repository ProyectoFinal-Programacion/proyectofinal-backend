namespace ManoVecinaAPI.DTOs.Gigs;

public class GigResponseDto
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public decimal Price { get; set; }

    // LISTA DE IMÁGENES (normalizada SIEMPRE)
    private List<string> _imageUrls = new();

    public List<string> ImageUrls
    {
        get => _imageUrls;
        set
        {
            _imageUrls = value?
                             .Select(url =>
                                 (url ?? string.Empty)
                                 .Replace("\\", "/")        // 🔥 Azure fix
                                 .Trim()
                             )
                             .Where(url => !string.IsNullOrWhiteSpace(url))
                             .ToList()
                         ?? new List<string>();
        }
    }

    public int WorkerId { get; set; }

    public string WorkerName { get; set; } = string.Empty;
}