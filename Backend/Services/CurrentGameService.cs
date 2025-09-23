using AutoMapper;
using Backend.Interfaces.Data;
using Backend.Interfaces.Services;
using Backend.Models.Domains;
using Backend.Models.DTOs;
using Backend.Models.Exceptions;

namespace Backend.Services;

public class CurrentGameService(IHttpContextAccessor httpContextAccessor, IUnitOfWork unitOfWork, IMapper mapper)
    : ServiceBase(httpContextAccessor), ICurrentGameService
{
    public async Task<CurrentGameDto> GetCurrentGameForCurrentUser()
    {
        var userId = GetUserId();

        var currentGame = await unitOfWork.CurrentGames.GetCurrentGameWithIncludesAsync(cg => cg.CurrentGameUsers.Any(u => u.UserId == userId));
        if (currentGame == default) return null;

        var result = mapper.Map<CurrentGameDto>(currentGame);

        return result;
    }

    public async Task<CurrentGameDto> GetCurrentGame(string sessionId)
    {
        var userId = GetUserId();

        var currentGame = await unitOfWork.CurrentGames.GetCurrentGameWithIncludesAsync(cg => cg.SessionId == sessionId);
        if (currentGame == default) throw new BusinessValidationException("Game was not found");

        if (currentGame.CurrentGameUsers.Any(u => u.UserId == GetUserId()))
        {
            var result = mapper.Map<CurrentGameDto>(currentGame);
            return result;
        }

        throw new BusinessValidationException("You are not part of this game");
    }

    public async Task CreateCurrentGame(CurrentGameDto currentGame)
    {
        var userId = GetUserId();

        var createdCurrentGame = await unitOfWork.CurrentGames.AddAsync(mapper.Map<CurrentGame>(currentGame));

        var questions = await unitOfWork.Questions.GetQuestionsByQuizId(createdCurrentGame.QuizId);

        var currentUser = await unitOfWork.Users.GetByIdAsync(GetUserId());
        if (currentUser == default) throw new BusinessValidationException("User was not found");

        foreach (var question in questions)
            createdCurrentGame.CurrentGameQuestions.Add(new CurrentGameQuestion
            {
                CurrentGameId = createdCurrentGame.CurrentGameId,
                QuestionId = question.QuestionId
            });

        createdCurrentGame.CurrentGameUsers.Add(new CurrentGameUser
        {
            CurrentGameId = createdCurrentGame.CurrentGameId,
            UserId = currentUser.UserId,
            IsGameMaster = true,
            CreatedByUserId = GetUserId(),
            CreatedOn = DateTime.UtcNow
        });

        await unitOfWork.CompleteAsync();
    }

    public async Task UpdateCurrentGame(CurrentGameDto currentGame)
    {
        var userId = GetUserId();

        if (currentGame.CurrentGameUsers.All(u => u.User.UserId != GetUserId())) throw new BusinessValidationException("You are not part of this game");

        await unitOfWork.CurrentGames.UpdateAsync(mapper.Map<CurrentGame>(currentGame));
        await unitOfWork.CompleteAsync();
    }

    public async Task DeleteCurrentGame(long currentGameId)
    {
        var userId = GetUserId();

        var currentGameToDelete = await unitOfWork.CurrentGames.FirstOrDefaultAsync(cg => cg.CurrentGameId == currentGameId);
        if (currentGameToDelete == default) throw new BusinessValidationException("Game was not found");

        if (currentGameToDelete.CreatedByUserId != GetUserId()) throw new BusinessValidationException("You can only delete games that you created");

        await unitOfWork.CurrentGames.RemoveAsync(currentGameToDelete);
        await unitOfWork.CompleteAsync();
    }
}