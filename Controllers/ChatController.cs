using System.Security.Claims;
using ManoVecinaAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ManoVecinaAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ChatController : ControllerBase
{
    private readonly FirebaseService _firebaseService;

    public ChatController(FirebaseService firebaseService)
    {
        _firebaseService = firebaseService;
    }

    [HttpGet("token")]
    public async Task<ActionResult<string>> GetChatToken()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        var token = await _firebaseService.CreateCustomTokenAsync(userId);
        return Ok(token);
    }
}
