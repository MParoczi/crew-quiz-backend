using AutoMapper;
using Backend.Interfaces.Data;
using Backend.Interfaces.Services;
using Backend.Models.Domains;
using Backend.Models.DTOs;
using Backend.Models.Exceptions;

namespace Backend.Services;

public class PreviousGameService(IHttpContextAccessor httpContextAccessor, IUnitOfWork unitOfWork, IMapper mapper)
    : ServiceBase(httpContextAccessor), IPreviousGameService
{
    public async Task ArchiveFinishedGame(long currentGameId)
    {
        var currentGame = await unitOfWork.CurrentGames.GetCurrentGameWithIncludesAsync(cg => cg.CurrentGameId == currentGameId);
        if (currentGame == default) throw new BusinessValidationException("Current game was not found");

        if (!currentGame.IsCompleted) throw new BusinessValidationException("Game is not completed yet");

        var previousGame = new PreviousGame
        {
            SessionId = currentGame.SessionId,
            QuizName = currentGame.Quiz.Name,
            CompletedOn = DateTime.UtcNow
        };

        var createdPreviousGame = await unitOfWork.PreviousGames.AddAsync(previousGame);

        await unitOfWork.CompleteAsync();

        var rankedUsers = currentGame.CurrentGameUsers
            .OrderByDescending(u => u.Points)
            .ThenBy(u => u.UserId)
            .ToList();

        for (var i = 0; i < rankedUsers.Count; i++)
        {
            var currentGameUser = rankedUsers[i];

            var previousGameUser = new PreviousGameUser
            {
                PreviousGameId = createdPreviousGame.PreviousGameId,
                UserId = currentGameUser.UserId,
                Username = currentGameUser.User.Username,
                IsGameMaster = currentGameUser.IsGameMaster,
                Points = currentGameUser.Points,
                Rank = i + 1 // Rankings start from 1
            };

            await unitOfWork.PreviousGameUsers.AddAsync(previousGameUser);
        }

        await unitOfWork.CurrentGames.RemoveAsync(currentGame);
        await unitOfWork.CompleteAsync();
    }

    public async Task<IEnumerable<PreviousGameDto>> GetPreviousGamesForCurrentUser()
    {
        var userId = GetUserId();

        var previousGames = await unitOfWork.PreviousGames.GetPreviousGamesByUserIdAsync(GetUserId());
        var result = mapper.Map<IEnumerable<PreviousGameDto>>(previousGames);

        return result;
    }

    public async Task<PreviousGameDto?> GetPreviousGame(string sessionId)
    {
        var userId = GetUserId();

        var previousGame = await unitOfWork.PreviousGames.GetPreviousGameWithIncludesAsync(cg => cg.SessionId == sessionId);
        if (previousGame == default) throw new BusinessValidationException("Game was not found");

        if (previousGame.PreviousGameUsers.Any(u => u.UserId == GetUserId()))
        {
            var result = mapper.Map<PreviousGameDto>(previousGame);
            return result;
        }

        throw new BusinessValidationException("You were not part of this game");
    }
}