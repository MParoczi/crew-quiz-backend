using Backend.Models.DTOs;

namespace Backend.Interfaces.ServiceUtils;

public interface IQuizServiceUtil : IServiceUtilBase
{
    public Task AddQuestionGroupsToQuiz(IEnumerable<QuestionGroupDto> questionGroupDtos, long quizId);
}