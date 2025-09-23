using AutoMapper;
using Backend.Interfaces.Data;
using Backend.Interfaces.Services;
using Backend.Interfaces.ServiceUtils;
using Backend.Models.Domains;
using Backend.Models.DTOs;
using Backend.Models.Exceptions;

namespace Backend.Services;

public class QuizService(
    IHttpContextAccessor httpContextAccessor,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    IQuizServiceUtil serviceUtil)
    : ServiceBase(httpContextAccessor), IQuizService
{
    public async Task<IEnumerable<QuizDto>> GetQuizzesForCurrentUser()
    {
        var userId = GetUserId();

        var quizzes = await unitOfWork.Quizzes.GetAllQuizzesByUserId(userId);
        var result = quizzes.Select(mapper.Map<QuizDto>).ToList();

        return result;
    }

    public async Task<QuizDto> GetQuizByCurrentGameId(long currentGameId)
    {
        var userId = GetUserId();

        var quiz = await unitOfWork.Quizzes.GetQuizByCurrentGameId(currentGameId);
        if (quiz == default) throw new BusinessValidationException("Quiz was not found");

        var currentGameUser = await unitOfWork.CurrentGameUsers.FirstOrDefaultAsync(cgu => cgu.CurrentGameId == currentGameId && cgu.UserId == userId);
        if (currentGameUser == default) throw new BusinessValidationException("User is not part of this game");

        var result = mapper.Map<QuizDto>(quiz);

        return result;
    }

    public async Task<QuizDto> GetQuiz(long quizId)
    {
        var currentUserId = GetUserId();

        var quiz = await unitOfWork.Quizzes.GetByIdAsync(quizId);
        if (quiz == default) throw new BusinessValidationException("Quiz was not found");

        if (currentUserId != quiz.CreatedByUserId) throw new BusinessValidationException("Quiz cannot be accessed by someone else");

        var result = mapper.Map<QuizDto>(quiz);

        return result;
    }

    public async Task CreateQuiz(QuizDto quizDto)
    {
        var userId = GetUserId();

        var quiz = mapper.Map<Quiz>(quizDto);
        quiz.QuestionGroups.Clear();

        var createdQuiz = await unitOfWork.Quizzes.AddAsync(quiz);
        await unitOfWork.CompleteAsync();

        if (quizDto.QuestionGroups.Count == 0) return;

        await serviceUtil.AddQuestionGroupsToQuiz(quizDto.QuestionGroups, createdQuiz.QuizId);
        await unitOfWork.CompleteAsync();
    }

    public async Task UpdateQuiz(QuizDto quizDto)
    {
        var userId = GetUserId();

        var quizToUpdate = await unitOfWork.Quizzes.GetByIdAsync(quizDto.QuizId);
        if (quizToUpdate == default) throw new BusinessValidationException("Quiz was not found");

        if (quizToUpdate.CreatedByUserId != userId) throw new BusinessValidationException("Quiz cannot be updated by someone else");

        await unitOfWork.Quizzes.UpdateAsync(mapper.Map<Quiz>(quizDto));

        await serviceUtil.AddQuestionGroupsToQuiz(quizDto.QuestionGroups, quizDto.QuizId);
        await unitOfWork.CompleteAsync();
    }

    public async Task DeleteQuiz(long quizId)
    {
        var userId = GetUserId();

        var quizToDelete = await unitOfWork.Quizzes.GetByIdAsync(quizId);
        if (quizToDelete == default) throw new BusinessValidationException("Quiz was not found");

        if (quizToDelete.CreatedByUserId != userId) throw new BusinessValidationException("Quiz cannot be deleted by someone else");

        await unitOfWork.Quizzes.RemoveAsync(quizToDelete);
        await unitOfWork.CompleteAsync();
    }
}