using Backend.Interfaces.Data;
using Backend.Interfaces.ServiceUtils;
using Backend.Models.Domains;
using Backend.Models.DTOs;

namespace Backend.ServiceUtils;

public class QuizServiceUtil(IUnitOfWork unitOfWork) : ServiceUtilBase, IQuizServiceUtil
{
    public async Task AddQuestionGroupsToQuiz(IEnumerable<QuestionGroupDto> questionGroupDtos, long quizId)
    {
        var questionGroupList = questionGroupDtos.ToList();

        await unitOfWork.QuestionGroupQuizzes.ClearQuestionGroupsFromQuiz(quizId);

        var validQuestionGroups = questionGroupList.Where(qg => qg.QuestionGroupId != default).ToList();

        var addedCount = 0;
        foreach (var questionGroupDto in validQuestionGroups)
        {
            var existingQuestionGroup = await unitOfWork.QuestionGroups.GetByIdAsync(questionGroupDto.QuestionGroupId);
            if (existingQuestionGroup == default) continue;

            var questionGroupQuiz = new QuestionGroupQuiz
            {
                QuizId = quizId,
                QuestionGroupId = existingQuestionGroup.QuestionGroupId
            };

            await unitOfWork.QuestionGroupQuizzes.AddAsync(questionGroupQuiz);
            addedCount++;
        }
    }
}