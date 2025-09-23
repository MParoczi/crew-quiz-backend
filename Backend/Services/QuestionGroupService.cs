using AutoMapper;
using Backend.Interfaces.Data;
using Backend.Interfaces.Services;
using Backend.Models.Domains;
using Backend.Models.DTOs;
using Backend.Models.Exceptions;

namespace Backend.Services;

public class QuestionGroupService(IHttpContextAccessor httpContextAccessor, IUnitOfWork unitOfWork, IMapper mapper)
    : ServiceBase(httpContextAccessor), IQuestionGroupService
{
    public async Task<IEnumerable<QuestionGroupDto>> GetQuestionGroupsForCurrentUser()
    {
        var userId = GetUserId();

        var questionGroups = await unitOfWork.QuestionGroups.GetQuestionGroupsByUserId(userId);
        var result = questionGroups.Select(mapper.Map<QuestionGroupDto>).ToList();

        return result;
    }

    public async Task<IEnumerable<QuestionGroupDto>> GetQuestionGroupsByQuizId(long quizId)
    {
        var userId = GetUserId();

        var questionGroups = await unitOfWork.QuestionGroups.GetQuestionGroupsByQuizId(quizId);
        if (questionGroups.Any(qg => qg.CreatedByUserId != userId))
            throw new BusinessValidationException("You can only access question groups that you created");

        var result = questionGroups.Select(mapper.Map<QuestionGroupDto>).ToList();

        return result;
    }

    public async Task<QuestionGroupDto> GetQuestionGroup(long questionGroupId)
    {
        var userId = GetUserId();

        var questionGroup = await unitOfWork.QuestionGroups.GetByIdAsync(questionGroupId);
        if (questionGroup == default) throw new BusinessValidationException("Question group was not found");

        if (questionGroup.CreatedByUserId != userId) throw new BusinessValidationException("You can only access question groups that you created");

        var result = mapper.Map<QuestionGroupDto>(questionGroup);

        return result;
    }

    public async Task CreateQuestionGroup(QuestionGroupDto questionGroup)
    {
        var userId = GetUserId();

        await unitOfWork.QuestionGroups.AddAsync(mapper.Map<QuestionGroup>(questionGroup));
        await unitOfWork.CompleteAsync();
    }

    public async Task UpdateQuestionGroup(QuestionGroupDto questionGroup)
    {
        var userId = GetUserId();

        var questionGroupToUpdate = await unitOfWork.QuestionGroups.GetByIdAsync(questionGroup.QuestionGroupId);
        if (questionGroupToUpdate == default) throw new BusinessValidationException("Question group was not found");

        if (questionGroupToUpdate.CreatedByUserId != userId) throw new BusinessValidationException("You can only update question groups that you created");

        await unitOfWork.QuestionGroups.UpdateAsync(mapper.Map<QuestionGroup>(questionGroup));

        foreach (var question in questionGroup.Questions)
            await unitOfWork.Questions.UpdateAsync(mapper.Map<Question>(question));

        await unitOfWork.CompleteAsync();
    }

    public async Task DeleteQuestionGroup(long questionGroupId)
    {
        var userId = GetUserId();

        var questionGroupToDelete = await unitOfWork.QuestionGroups.GetByIdAsync(questionGroupId);
        if (questionGroupToDelete == default) throw new BusinessValidationException("Question group was not found");

        if (questionGroupToDelete.CreatedByUserId != userId) throw new BusinessValidationException("You can only delete question groups that you created");

        await unitOfWork.QuestionGroups.RemoveAsync(questionGroupToDelete);
        await unitOfWork.CompleteAsync();
    }
}