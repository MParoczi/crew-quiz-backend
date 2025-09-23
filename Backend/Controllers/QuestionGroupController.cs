using Backend.Interfaces.Services;
using Backend.Interfaces.Utils;
using Backend.Models.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]/[action]")]
public class QuestionGroupController(IServiceDispatcher serviceDispatcher) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<QuestionGroupDto>>> GetQuestionGroupsForCurrentUser()
    {
        var questionGroups = await serviceDispatcher.For<IQuestionGroupService>().DispatchAsync(s => s.GetQuestionGroupsForCurrentUser());
        return Ok(questionGroups);
    }

    [HttpGet("{quizId:long}")]
    public async Task<ActionResult<IEnumerable<QuestionGroupDto>>> GetQuestionGroupsByQuizId(long quizId)
    {
        var questionGroups = await serviceDispatcher.For<IQuestionGroupService>().DispatchAsync(s => s.GetQuestionGroupsByQuizId(quizId));
        return Ok(questionGroups);
    }

    [HttpGet("{questionGroupId:long}")]
    public async Task<ActionResult<QuestionGroupDto>> GetQuestionGroup(long questionGroupId)
    {
        var questionGroup = await serviceDispatcher.For<IQuestionGroupService>().DispatchAsync(s => s.GetQuestionGroup(questionGroupId));
        return Ok(questionGroup);
    }

    [HttpPost]
    public async Task<ActionResult> CreateQuestionGroup(QuestionGroupDto questionGroupDto)
    {
        await serviceDispatcher.For<IQuestionGroupService>().DispatchAsync(s => s.CreateQuestionGroup(questionGroupDto));
        return Ok();
    }

    [HttpPut]
    public async Task<ActionResult> UpdateQuestionGroup(QuestionGroupDto questionGroupDto)
    {
        await serviceDispatcher.For<IQuestionGroupService>().DispatchAsync(s => s.UpdateQuestionGroup(questionGroupDto));
        return Ok();
    }

    [HttpDelete("{questionGroupId:long}")]
    public async Task<ActionResult> DeleteQuestionGroup(long questionGroupId)
    {
        await serviceDispatcher.For<IQuestionGroupService>().DispatchAsync(s => s.DeleteQuestionGroup(questionGroupId));
        return Ok();
    }
}