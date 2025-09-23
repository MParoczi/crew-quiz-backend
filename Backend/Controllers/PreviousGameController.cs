using Backend.Interfaces.Services;
using Backend.Interfaces.Utils;
using Backend.Models.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]/[action]")]
public class PreviousGameController(IServiceDispatcher serviceDispatcher) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PreviousGameDto>>> GetPreviousGamesForCurrentUser()
    {
        var previousGames = await serviceDispatcher.For<IPreviousGameService>().DispatchAsync(s => s.GetPreviousGamesForCurrentUser());
        return Ok(previousGames);
    }

    [HttpGet("{sessionId}")]
    public async Task<ActionResult<PreviousGameDto>> GetPreviousGameBySessionId(string sessionId)
    {
        var previousGame = await serviceDispatcher.For<IPreviousGameService>().DispatchAsync(s => s.GetPreviousGame(sessionId));
        return Ok(previousGame);
    }
}