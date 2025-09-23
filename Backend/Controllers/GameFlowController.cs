using System.Collections.Concurrent;
using Backend.Interfaces.Services;
using Backend.Interfaces.Utils;
using Backend.Models.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]/[action]")]
public class GameFlowController(IServiceDispatcher serviceDispatcher) : ControllerBase
{
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> SessionSemaphores = new();

    [HttpPost]
    public async Task<ActionResult> AddUserToCurrentGame(GameFlowDto gameFlowDto)
    {
        await serviceDispatcher.For<IGameFlowService>().DispatchAsync(s => s.AddUserToCurrentGame(gameFlowDto));
        return Ok();
    }

    [HttpPost]
    public async Task<ActionResult> StartGame(GameFlowDto gameFlowDto)
    {
        await serviceDispatcher.For<IGameFlowService>().DispatchAsync(s => s.StartGame(gameFlowDto));
        return Ok();
    }

    [HttpPost]
    public async Task<ActionResult> SelectQuestion(GameFlowDto gameFlowDto)
    {
        await serviceDispatcher.For<IGameFlowService>().DispatchAsync(s => s.SelectQuestion(gameFlowDto));
        return Ok();
    }

    [HttpPost]
    public async Task<ActionResult> SubmitAnswer(GameFlowDto gameFlowDto)
    {
        var semaphore = SessionSemaphores.GetOrAdd(gameFlowDto.SessionId, _ => new SemaphoreSlim(1, 1));

        await semaphore.WaitAsync();
        try
        {
            await serviceDispatcher.For<IGameFlowService>().DispatchAsync(s => s.SubmitAnswer(gameFlowDto));
            return Ok();
        }
        finally
        {
            semaphore.Release();

            if (semaphore.CurrentCount == 1) SessionSemaphores.TryRemove(gameFlowDto.SessionId, out _);
        }
    }

    [HttpPost]
    public async Task<ActionResult> RobQuestion(GameFlowDto gameFlowDto)
    {
        var semaphore = SessionSemaphores.GetOrAdd(gameFlowDto.SessionId, _ => new SemaphoreSlim(1, 1));

        await semaphore.WaitAsync();
        try
        {
            await serviceDispatcher.For<IGameFlowService>().DispatchAsync(s => s.RobQuestion(gameFlowDto));
            return Ok();
        }
        finally
        {
            semaphore.Release();

            if (semaphore.CurrentCount == 1) SessionSemaphores.TryRemove(gameFlowDto.SessionId, out _);
        }
    }

    [HttpPost]
    public async Task<ActionResult> LeaveGame(GameFlowDto gameFlowDto)
    {
        await serviceDispatcher.For<IGameFlowService>().DispatchAsync(s => s.LeaveGame(gameFlowDto));
        return Ok();
    }

    [HttpPost]
    public async Task<ActionResult> CancelGame(GameFlowDto gameFlowDto)
    {
        await serviceDispatcher.For<IGameFlowService>().DispatchAsync(s => s.CancelGame(gameFlowDto));
        return Ok();
    }
    
    [HttpPost]
    public async Task<ActionResult> SelectNextPlayer(GameFlowDto gameFlowDto)
    {
        await serviceDispatcher.For<IGameFlowService>().DispatchAsync(s => s.SelectNextPlayer(gameFlowDto));
        return Ok();
    }
    
    [HttpPost]
    public async Task<ActionResult> AllowRobbing(GameFlowDto gameFlowDto)
    {
        await serviceDispatcher.For<IGameFlowService>().DispatchAsync(s => s.AllowRobbing(gameFlowDto));
        return Ok();
    }
}