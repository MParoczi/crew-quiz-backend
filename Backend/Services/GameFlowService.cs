using Backend.Enums;
using Backend.Extensions;
using Backend.Interfaces.Data;
using Backend.Interfaces.Services;
using Backend.Interfaces.ServiceUtils;
using Backend.Models.Domains;
using Backend.Models.DTOs;
using Backend.Models.Exceptions;

namespace Backend.Services;

public class GameFlowService(
    IHttpContextAccessor httpContextAccessor,
    IGameFlowServiceUtil serviceUtil,
    IUnitOfWork unitOfWork) : ServiceBase(httpContextAccessor), IGameFlowService
{
    public async Task AddUserToCurrentGame(GameFlowDto gameFlowDto)
    {
        var currentGame = await serviceUtil.GetAndValidateCurrentGame(GameEventType.PlayerJoined, gameFlowDto, GetUserId());
        var userToAdd = await unitOfWork.Users.GetByIdAsync(gameFlowDto.UserId!.Value);

        if (userToAdd == default) throw new BusinessValidationException("User not found.");

        var newCurrentGameUser = new CurrentGameUser
        {
            CurrentGameId = currentGame.CurrentGameId,
            UserId = gameFlowDto.UserId!.Value,
            CreatedByUserId = GetUserId(),
            CreatedOn = DateTime.UtcNow
        };

        // Add to the repository to ensure proper Entity Framework tracking
        await unitOfWork.CurrentGameUsers.AddAsync(newCurrentGameUser);

        await unitOfWork.CompleteAsync();

        gameFlowDto.Username = userToAdd.Username;
        await serviceUtil.UpdateGameFlow(gameFlowDto, GameEventType.PlayerJoined);
    }

    public async Task StartGame(GameFlowDto gameFlowDto)
    {
        var currentGame = await serviceUtil.GetAndValidateCurrentGame(GameEventType.GameStarted, gameFlowDto, GetUserId());

        currentGame.IsStarted = true;
        await unitOfWork.CurrentGames.UpdateAsync(currentGame);

        await serviceUtil.SelectNextPlayer(currentGame);

        await serviceUtil.UpdateGameFlow(gameFlowDto, GameEventType.GameStarted);
    }

    public async Task SelectQuestion(GameFlowDto gameFlowDto)
    {
        var currentGame = await serviceUtil.GetAndValidateCurrentGame(GameEventType.QuestionSelected, gameFlowDto, GetUserId());

        await serviceUtil.SelectNextQuestion(currentGame, gameFlowDto.QuestionId!.Value);
        var currentUser = currentGame.CurrentGameUsers.FirstOrDefault(cgu => cgu.IsCurrent)?.User;
        
        gameFlowDto.Username = currentUser?.Username;
        await serviceUtil.UpdateGameFlow(gameFlowDto, GameEventType.QuestionSelected);
    }

    public async Task<bool> SubmitAnswer(GameFlowDto gameFlowDto)
    {
        var currentGame = await serviceUtil.GetAndValidateCurrentGame(GameEventType.AnswerSubmitted, gameFlowDto, GetUserId());
        var currentQuestion = currentGame.CurrentGameQuestions.First(q => q.IsCurrent);
        var currentUser = currentGame.CurrentGameUsers.FirstOrDefault(cgu => cgu.IsCurrent)?.User;
        await unitOfWork.CurrentGameQuestions.IncludeReferenceAsync(currentQuestion, cgq => cgq.Question);

        var question = await unitOfWork.Questions.GetByIdAsync(currentQuestion.QuestionId);

        gameFlowDto.Username = currentUser?.Username;
        
        if (question != default &&
            string.Equals(question.Answer.RemoveAccents(), gameFlowDto.Answer?.RemoveAccents(), StringComparison.CurrentCultureIgnoreCase))
        {
            currentGame = await serviceUtil.SetQuestionAnswered(gameFlowDto, currentGame, currentQuestion);
            currentGame = await serviceUtil.SelectNextPlayer(currentGame);

            var isLastQuestion = serviceUtil.CheckIfLastQuestion(currentGame);
            
            gameFlowDto.Answer = currentQuestion.Question.Answer;

            if (isLastQuestion)
            {
                var completionDto = await serviceUtil.CompleteGame(gameFlowDto, currentGame);
                await serviceUtil.UpdateGameFlow(completionDto, GameEventType.GameEnded);
            }
            else
            {
                await serviceUtil.UpdateGameFlow(gameFlowDto, GameEventType.QuestionAnswered);
            }

            return true;
        }

        if (question != default)
        {
            if (!currentQuestion.IsRobbingAllowed)
            {
                await serviceUtil.SetQuestionRobbingAllowed(currentGame, currentQuestion);
                await serviceUtil.UpdateGameFlow(gameFlowDto, GameEventType.QuestionRobbingIsAllowed);
            }
            else
            {
                await serviceUtil.UpdateGameFlow(gameFlowDto, GameEventType.QuestionAnsweredWrong);   
            }
        }

        return false;
    }

    public async Task<bool> RobQuestion(GameFlowDto gameFlowDto)
    {
        var currentGame = await serviceUtil.GetAndValidateCurrentGame(GameEventType.AnswerSubmitted, gameFlowDto, GetUserId());
        var currentQuestion = currentGame.CurrentGameQuestions.First(q => q.IsCurrent && q.IsRobbingAllowed);
        var currentUser = currentGame.CurrentGameUsers.FirstOrDefault(cgu => cgu.UserId == GetUserId())?.User;
        
        var question = await unitOfWork.Questions.GetByIdAsync(currentQuestion.QuestionId);

        gameFlowDto.Username = currentUser?.Username;
        
        if (question != default &&
            string.Equals(question.Answer.RemoveAccents(), gameFlowDto.Answer?.RemoveAccents(), StringComparison.CurrentCultureIgnoreCase))
        {
            currentGame = await serviceUtil.SetQuestionRobbedByUser(gameFlowDto, currentGame, currentQuestion);
            currentGame = await serviceUtil.SelectNextPlayer(currentGame);

            var isLastQuestion = serviceUtil.CheckIfLastQuestion(currentGame);
            
            gameFlowDto.Answer = currentQuestion.Question.Answer;

            if (isLastQuestion)
            {
                var completionDto = await serviceUtil.CompleteGame(gameFlowDto, currentGame);
                await serviceUtil.UpdateGameFlow(completionDto, GameEventType.GameEnded);
            }
            else
            {
                await serviceUtil.UpdateGameFlow(gameFlowDto, GameEventType.QuestionRobbed);
            }

            return true;
        }

        if (question != default)
        {
            await serviceUtil.UpdateGameFlow(gameFlowDto, GameEventType.QuestionAnsweredWrong);
        }

        return false;
    }

    public async Task LeaveGame(GameFlowDto gameFlowDto)
    {
        var currentGame = await serviceUtil.GetAndValidateCurrentGame(GameEventType.PlayerLeft, gameFlowDto, GetUserId());
        var playerToRemove = currentGame.CurrentGameUsers.First(u => u.UserId == gameFlowDto.UserId);
        
        var wasCurrentPlayer = playerToRemove.IsCurrent;
        
        await unitOfWork.CurrentGameUsers.RemoveAsync(playerToRemove);
        await unitOfWork.CompleteAsync();
        
        if (wasCurrentPlayer && currentGame.CurrentGameUsers.Count > 1)
        {
            currentGame = await serviceUtil.GetAndValidateCurrentGame(GameEventType.PlayerLeft, gameFlowDto, GetUserId());
            await serviceUtil.SelectNextPlayer(currentGame);
        }

        gameFlowDto.Username = playerToRemove.User.Username;
        await serviceUtil.UpdateGameFlow(gameFlowDto, GameEventType.PlayerLeft);
    }

    public async Task CancelGame(GameFlowDto gameFlowDto)
    {
        var currentGame = await serviceUtil.GetAndValidateCurrentGame(GameEventType.GameCancelled, gameFlowDto, GetUserId());
        await unitOfWork.CurrentGames.RemoveAsync(currentGame);
        await unitOfWork.CompleteAsync();
        await serviceUtil.UpdateGameFlow(gameFlowDto, GameEventType.GameCancelled);
    }

    public async Task SelectNextPlayer(GameFlowDto gameFlowDto)
    {
        var currentGame = await serviceUtil.GetAndValidateCurrentGame(GameEventType.NextPlayerSelected, gameFlowDto, GetUserId());
        if (currentGame.CurrentGameUsers.Count > 1)
        {
            currentGame = await serviceUtil.SelectNextPlayer(currentGame);
            var currentUser = currentGame.CurrentGameUsers.FirstOrDefault(u => u.IsCurrent)?.User;
            gameFlowDto.Username = currentUser?.Username;
            await serviceUtil.UpdateGameFlow(gameFlowDto, GameEventType.NextPlayerSelected);
        }
    }

    public async Task AllowRobbing(GameFlowDto gameFlowDto)
    {
        var currentGame = await serviceUtil.GetAndValidateCurrentGame(GameEventType.QuestionRobbingIsAllowed, gameFlowDto, GetUserId());
        var currentQuestion = currentGame.CurrentGameQuestions.First(q => q.IsCurrent);
        await serviceUtil.SetQuestionRobbingAllowed(currentGame, currentQuestion);
        await serviceUtil.UpdateGameFlow(gameFlowDto, GameEventType.QuestionRobbingIsAllowed);
    }
}