using Backend.Interfaces.Services;
using Backend.Interfaces.Utils;
using Backend.Models.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]/[action]")]
public class CurrentGameController(IServiceDispatcher serviceDispatcher) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<CurrentGameDto>> GetCurrentGameForCurrentUser()
    {
        var currentGame = await serviceDispatcher.For<ICurrentGameService>().DispatchAsync(s => s.GetCurrentGameForCurrentUser());
        return Ok(currentGame);
    }

    [HttpGet("{sessionId}")]
    public async Task<ActionResult<CurrentGameDto>> GetCurrentGameBySessionId(string sessionId)
    {
        var currentGame = await serviceDispatcher.For<ICurrentGameService>().DispatchAsync(s => s.GetCurrentGame(sessionId));
        return Ok(currentGame);
    }

    [HttpPost]
    public async Task<ActionResult> CreateCurrentGame(CurrentGameDto currentGameDto)
    {
        await serviceDispatcher.For<ICurrentGameService>().DispatchAsync(s => s.CreateCurrentGame(currentGameDto));
        return Ok();
    }

    [HttpPut]
    public async Task<ActionResult> UpdateCurrentGame(CurrentGameDto currentGameDto)
    {
        await serviceDispatcher.For<ICurrentGameService>().DispatchAsync(s => s.UpdateCurrentGame(currentGameDto));
        return Ok();
    }

    [HttpDelete("{currentGameId:long}")]
    public async Task<ActionResult> DeleteCurrentGame(long currentGameId)
    {
        await serviceDispatcher.For<ICurrentGameService>().DispatchAsync(s => s.DeleteCurrentGame(currentGameId));
        return Ok();
    }
}