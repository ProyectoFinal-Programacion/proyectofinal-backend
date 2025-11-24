using ManoVecinaAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ManoVecinaAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UploadsController : ControllerBase
{
    private readonly ImageStorageService _imageService;

    public UploadsController(ImageStorageService imageService)
    {
        _imageService = imageService;
    }

    [HttpPost("chat")]
    public async Task<ActionResult<string>> UploadChatImage([FromForm] int conversationId, IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("File is required");

        var url = await _imageService.SaveChatImageAsync(conversationId, file);
        return Ok(url);
    }
}
