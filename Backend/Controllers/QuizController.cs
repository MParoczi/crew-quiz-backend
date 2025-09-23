using Backend.Interfaces.Services;
using Backend.Interfaces.Utils;
using Backend.Models.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]/[action]")]
public class QuizController(IServiceDispatcher serviceDispatcher) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<QuizDto>>> GetQuizzesForCurrentUser()
    {
        var quizzes = await serviceDispatcher.For<IQuizService>().DispatchAsync(s => s.GetQuizzesForCurrentUser());
        return Ok(quizzes);
    }

    [HttpGet("{currentGameId:long}")]
    public async Task<ActionResult<QuizDto>> GetQuizByCurrentGameId(long currentGameId)
    {
        var quiz = await serviceDispatcher.For<IQuizService>().DispatchAsync(s => s.GetQuizByCurrentGameId(currentGameId));
        return Ok(quiz);
    }

    [HttpGet("{quizId:long}")]
    public async Task<ActionResult<QuizDto>> GetQuiz(long quizId)
    {
        var quiz = await serviceDispatcher.For<IQuizService>().DispatchAsync(s => s.GetQuiz(quizId));
        return Ok(quiz);
    }

    [HttpPost]
    public async Task<ActionResult> CreateQuiz(QuizDto quizDto)
    {
        await serviceDispatcher.For<IQuizService>().DispatchAsync(s => s.CreateQuiz(quizDto));
        return Ok();
    }

    [HttpPut]
    public async Task<ActionResult> UpdateQuiz(QuizDto quizDto)
    {
        await serviceDispatcher.For<IQuizService>().DispatchAsync(s => s.UpdateQuiz(quizDto));
        return Ok();
    }

    [HttpDelete("{quizId:long}")]
    public async Task<ActionResult> DeleteQuiz(long quizId)
    {
        await serviceDispatcher.For<IQuizService>().DispatchAsync(s => s.DeleteQuiz(quizId));
        return Ok();
    }
}