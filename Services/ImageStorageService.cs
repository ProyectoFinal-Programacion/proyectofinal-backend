using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace ManoVecinaAPI.Services;

public class ImageStorageService
{
    private readonly IWebHostEnvironment _env;

    public ImageStorageService(IWebHostEnvironment env)
    {
        _env = env;
    }

    // ============================================================
    // AVATAR
    // ============================================================
    public Task<string> SaveAvatarAsync(int userId, IFormFile file)
        => SaveImageAsync(file, "uploads/avatars", $"user_{userId}");


    // ============================================================
    // GIG IMAGE
    // ============================================================
    public Task<string> SaveGigImageAsync(int gigId, IFormFile file)
        => SaveImageAsync(file, "uploads/gigs", $"gig_{gigId}");


    // ============================================================
    // CHAT IMAGE
    // ============================================================
    public Task<string> SaveChatImageAsync(int conversationId, IFormFile file)
        => SaveImageAsync(file, "uploads/chat", $"chat_{conversationId}");


    // ============================================================
    // BASE IMAGE SAVE (100% Azure-friendly)
    // ============================================================
    private async Task<string> SaveImageAsync(IFormFile file, string relativeFolder, string prefix)
    {
        // Asegurar que siempre usemos slashes correctos
        relativeFolder = relativeFolder.Replace("\\", "/");

        // wwwroot siempre existe en Azure
        var wwwroot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

        // Ruta absoluta real en disco
        var folderPath = Path.Combine(wwwroot, relativeFolder.Replace("/", Path.DirectorySeparatorChar.ToString()));

        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        // Obtener extensión
        var ext = Path.GetExtension(file.FileName).ToLower();

        // Validar extensiones (mínimo)
        var allowed = new[] { ".png", ".jpg", ".jpeg", ".webp" };

        if (!allowed.Contains(ext))
            ext = ".jpg"; // fallback seguro

        // Nombre único (sin espacios)
        var fileName = $"{prefix}_{Guid.NewGuid():N}{ext}";
        var fullPath = Path.Combine(folderPath, fileName);

        // Guardar archivo
        using (var stream = new FileStream(fullPath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // Construir ruta pública con slashes correctos
        var publicPath = "/" + $"{relativeFolder}/{fileName}"
            .Replace("\\", "/")
            .Replace("//", "/");

        return publicPath;
    }
}
