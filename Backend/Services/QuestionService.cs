using AutoMapper;
using Backend.Interfaces.Data;
using Backend.Interfaces.Services;
using Backend.Models.Domains;
using Backend.Models.DTOs;
using Backend.Models.Exceptions;

namespace Backend.Services;

public class QuestionService(IHttpContextAccessor httpContextAccessor, IUnitOfWork unitOfWork, IMapper mapper)
    : ServiceBase(httpContextAccessor), IQuestionService
{
    public async Task<IEnumerable<QuestionDto>> GetQuestionsForCurrentUser()
    {
        var userId = GetUserId();

        var questions = await unitOfWork.Questions.GetQuestionsByUserId(userId);
        var result = questions.Select(mapper.Map<QuestionDto>).ToList();

        return result;
    }

    public async Task<IEnumerable<QuestionDto>> GetQuestionsByQuestionGroupId(long questionGroupId)
    {
        var userId = GetUserId();

        var questions = await unitOfWork.Questions.GetQuestionsByQuestionGroupId(questionGroupId);
        if (questions.Any(q => q.CreatedByUserId != userId)) throw new BusinessValidationException("You can only access questions that you created");

        var result = questions.Select(mapper.Map<QuestionDto>).ToList();

        return result;
    }

    public async Task<QuestionDto> GetQuestion(long questionId)
    {
        var userId = GetUserId();

        var question = await unitOfWork.Questions.GetByIdAsync(questionId);
        if (question == default) throw new BusinessValidationException("Question was not found");

        if (question.CreatedByUserId != userId) throw new BusinessValidationException("You can only access questions that you created");

        var result = mapper.Map<QuestionDto>(question);

        return result;
    }

    public async Task CreateQuestion(QuestionDto questionDto)
    {
        var userId = GetUserId();

        await unitOfWork.Questions.AddAsync(mapper.Map<Question>(questionDto));
        await unitOfWork.CompleteAsync();
    }

    public async Task UpdateQuestion(QuestionDto questionDto)
    {
        var userId = GetUserId();

        var questionToUpdate = await unitOfWork.Questions.GetByIdAsync(questionDto.QuestionId);
        if (questionToUpdate == default) throw new BusinessValidationException("Question was not found");

        if (questionToUpdate.CreatedByUserId != userId) throw new BusinessValidationException("You can only update questions that you created");

        await unitOfWork.Questions.UpdateAsync(mapper.Map<Question>(questionDto));
        await unitOfWork.CompleteAsync();
    }

    public async Task DeleteQuestion(long questionId)
    {
        var userId = GetUserId();

        var questionToDelete = await unitOfWork.Questions.GetByIdAsync(questionId);
        if (questionToDelete == default) throw new BusinessValidationException("Question was not found");

        if (questionToDelete.CreatedByUserId != userId) throw new BusinessValidationException("You can only delete questions that you created");

        await unitOfWork.Questions.RemoveAsync(questionToDelete);
        await unitOfWork.CompleteAsync();
    }
}