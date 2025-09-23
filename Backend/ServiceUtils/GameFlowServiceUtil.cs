using Backend.Enums;
using Backend.Hubs;
using Backend.Interfaces.Data;
using Backend.Interfaces.Services;
using Backend.Interfaces.ServiceUtils;
using Backend.Interfaces.Utils;
using Backend.Models.Domains;
using Backend.Models.DTOs;
using Backend.Models.Exceptions;
using Microsoft.AspNetCore.SignalR;

namespace Backend.ServiceUtils;

public class GameFlowServiceUtil(
    IHubContext<GameHub> hubContext,
    IUnitOfWork unitOfWork,
    IServiceDispatcher serviceDispatcher)
    : ServiceUtilBase, IGameFlowServiceUtil
{
    public async Task SendEventToGroup(GameFlowDto gameFlowDto, GameEventType eventType)
    {
        try
        {
            await hubContext.Clients.Group(gameFlowDto.SessionId).SendAsync(eventType.ToString(), gameFlowDto);
        }
        catch (HubException ex)
        {
            throw new BusinessValidationException("Couldn't send event to users");
        }
    }

    public async Task UpdateGameFlow(GameFlowDto gameFlowDto, GameEventType eventType)
    {
        await SendEventToGroup(gameFlowDto, eventType);
    }

    public async Task<CurrentGame> GetAndValidateCurrentGame(GameEventType gameEventType, GameFlowDto gameFlowDto, long currentUserId)
    {
        var currentGame = await unitOfWork.CurrentGames.GetCurrentGameWithIncludesAsync(cg => cg.SessionId == gameFlowDto.SessionId);

        if (currentGame == default) throw new BusinessValidationException("Game was not found");

        gameFlowDto.Validate(gameEventType, currentGame, currentUserId);

        return currentGame;
    }

    public async Task<CurrentGame> SelectNextPlayer(CurrentGame currentGame)
    {
        if (currentGame.CurrentGameUsers.Count == 0) throw new BusinessValidationException("No users are available");

        var currentPlayer = currentGame.CurrentGameUsers.FirstOrDefault(u => u.IsCurrent);
        CurrentGameUser? nextPlayer = null;

        if (currentPlayer == default)
        {
            nextPlayer = currentGame.CurrentGameUsers.FirstOrDefault(u => !u.IsGameMaster);
            if (nextPlayer != default) nextPlayer.IsCurrent = true;
        }
        else
        {
            var currentPlayerIndex = currentGame.CurrentGameUsers.IndexOf(currentPlayer);
            if (currentPlayerIndex == currentGame.CurrentGameUsers.Count - 1)
            {
                nextPlayer = currentGame.CurrentGameUsers.FirstOrDefault(u => !u.IsGameMaster);
                if (nextPlayer != default) nextPlayer.IsCurrent = true;
            }
            else
            {
                for (var i = currentPlayerIndex + 1; i < currentGame.CurrentGameUsers.Count; i++)
                {
                    if (currentGame.CurrentGameUsers[i].IsGameMaster) continue;
                    currentGame.CurrentGameUsers[i].IsCurrent = true;
                    nextPlayer = currentGame.CurrentGameUsers[i];
                    break;
                }
            }

            currentPlayer.IsCurrent = false;
        }

        if (nextPlayer == default) throw new BusinessValidationException("No next player found");

        if (currentPlayer != null)
            await unitOfWork.CurrentGameUsers.UpdateAsync(currentPlayer);

        await unitOfWork.CurrentGameUsers.UpdateAsync(nextPlayer);
        await unitOfWork.CompleteAsync();

        return currentGame;
    }


    public async Task<CurrentGame> SelectNextQuestion(CurrentGame currentGame, long questionId)
    {
        var selectedQuestion = currentGame.CurrentGameQuestions.First(q => q.QuestionId == questionId);
        selectedQuestion.IsCurrent = true;

        await unitOfWork.CurrentGameQuestions.UpdateAsync(selectedQuestion);
        await unitOfWork.CompleteAsync();

        return currentGame;
    }

    public async Task<CurrentGame> SetQuestionAnswered(GameFlowDto gameFlowDto, CurrentGame currentGame, CurrentGameQuestion currentQuestion)
    {
        var answeringUser = currentGame.CurrentGameUsers.First(u => u.UserId == gameFlowDto.UserId);
        var pointsAwarded = currentQuestion.Question.Point;

        currentQuestion.IsAnswered = true;
        currentQuestion.IsCurrent = false;
        currentQuestion.AnsweredByUserId = gameFlowDto.UserId;

        answeringUser.Points += pointsAwarded;

        await unitOfWork.CurrentGameQuestions.UpdateAsync(currentQuestion);
        await unitOfWork.CurrentGameUsers.UpdateAsync(answeringUser);

        return currentGame;
    }

    public async Task<CurrentGame> SetQuestionRobbingAllowed(CurrentGame currentGame, CurrentGameQuestion currentQuestion)
    {
        currentQuestion.IsRobbingAllowed = true;
        await unitOfWork.CurrentGameQuestions.UpdateAsync(currentQuestion);
        await unitOfWork.CompleteAsync();

        return currentGame;
    }

    public async Task<CurrentGame> SetQuestionRobbedByUser(GameFlowDto gameFlowDto, CurrentGame currentGame, CurrentGameQuestion currentQuestion)
    {
        var robbingUser = currentGame.CurrentGameUsers.First(u => u.UserId == gameFlowDto.UserId);
        var pointsAwarded = currentQuestion.Question.Point;

        currentQuestion.IsAnswered = true;
        currentQuestion.IsCurrent = false;
        currentQuestion.IsRobbingAllowed = false;
        currentQuestion.AnsweredByUserId = gameFlowDto.UserId;

        robbingUser.Points += pointsAwarded;

        await unitOfWork.CurrentGameQuestions.UpdateAsync(currentQuestion);
        await unitOfWork.CurrentGameUsers.UpdateAsync(robbingUser);

        return currentGame;
    }

    public bool CheckIfLastQuestion(CurrentGame currentGame)
    {
        var unansweredQuestions = currentGame.CurrentGameQuestions.Count(q => !q.IsAnswered);
        var isLastQuestion = unansweredQuestions == 0;

        return isLastQuestion;
    }

    public async Task<GameFlowDto> CompleteGame(GameFlowDto gameFlowDto, CurrentGame currentGame)
    {
        // Mark the game as completed
        currentGame.IsCompleted = true;
        await unitOfWork.CurrentGames.UpdateAsync(currentGame);
        await unitOfWork.CompleteAsync();

        // Calculate final scores and rankings
        var playerResults = currentGame.CurrentGameUsers
            .OrderByDescending(user => user.Points)
            .ThenBy(user => user.User.Username)
            .Select((user, index) => new PlayerResult
            {
                UserId = user.UserId,
                Username = user.User.Username,
                Points = user.Points,
                Rank = index + 1,
                IsGameMaster = user.IsGameMaster
            })
            .ToList();

        // Create enriched GameFlowDto with final results
        var completionDto = new GameFlowDto
        {
            SessionId = gameFlowDto.SessionId,
            UserId = gameFlowDto.UserId,
            FinalResults = playerResults,
            IsGameCompleted = true
        };

        await serviceDispatcher.For<IPreviousGameService>().DispatchAsync(s => s.ArchiveFinishedGame(currentGame.CurrentGameId));
        return completionDto;
    }
}