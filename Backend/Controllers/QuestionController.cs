using Backend.Interfaces.Services;
using Backend.Interfaces.Utils;
using Backend.Models.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]/[action]")]
public class QuestionController(IServiceDispatcher serviceDispatcher) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<QuestionDto>>> GetQuestionsForCurrentUser()
    {
        var questions = await serviceDispatcher.For<IQuestionService>().DispatchAsync(s => s.GetQuestionsForCurrentUser());
        return Ok(questions);
    }

    [HttpGet("{questionGroupId:long}")]
    public async Task<ActionResult<IEnumerable<QuestionDto>>> GetQuestionsByQuestionGroupId(long questionGroupId)
    {
        var questions = await serviceDispatcher.For<IQuestionService>().DispatchAsync(s => s.GetQuestionsByQuestionGroupId(questionGroupId));
        return Ok(questions);
    }

    [HttpGet("{questionId:long}")]
    public async Task<ActionResult<QuestionDto>> GetQuestion(long questionId)
    {
        var question = await serviceDispatcher.For<IQuestionService>().DispatchAsync(s => s.GetQuestion(questionId));
        return Ok(question);
    }

    [HttpPost]
    public async Task<ActionResult> CreateQuestion(QuestionDto questionDto)
    {
        await serviceDispatcher.For<IQuestionService>().DispatchAsync(s => s.CreateQuestion(questionDto));
        return Ok();
    }

    [HttpPut]
    public async Task<ActionResult> UpdateQuestion(QuestionDto questionDto)
    {
        await serviceDispatcher.For<IQuestionService>().DispatchAsync(s => s.UpdateQuestion(questionDto));
        return Ok();
    }

    [HttpDelete("{questionId:long}")]
    public async Task<ActionResult> DeleteQuestion(long questionId)
    {
        await serviceDispatcher.For<IQuestionService>().DispatchAsync(s => s.DeleteQuestion(questionId));
        return Ok();
    }
}