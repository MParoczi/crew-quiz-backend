using System.Data;
using Backend.Enums;
using Backend.Models.Domains;
using Backend.Models.Exceptions;

namespace Backend.Models.DTOs;

public class GameFlowDto
{
    public long? UserId { get; set; }
    public string? Username { get; set; }
    public required string SessionId { get; set; }
    public long? QuestionId { get; set; }
    public string? Answer { get; set; }

    // Game completion results
    public List<PlayerResult>? FinalResults { get; set; }
    public bool? IsGameCompleted { get; set; }

    public void Validate(GameEventType gameEventType, CurrentGame? currentGame, long currentUserId)
    {
        ValidateCurrentGame(currentGame);

        switch (gameEventType)
        {
            case GameEventType.PlayerJoined:
                ValidateUser(currentUserId);
                ValidateGameNotStarted(currentGame!);
                break;
            case GameEventType.GameStarted:
                ValidateGameNotStarted(currentGame!);
                ValidateGameMaster(currentGame!, currentUserId);
                break;
            case GameEventType.QuestionSelected:
                ValidateJoinedUser(currentGame!, currentUserId);
                ValidatePreviousQuestionFinished(currentGame!);
                ValidateQuestion(currentGame!);
                break;
            case GameEventType.AnswerSubmitted:
                ValidateJoinedUser(currentGame!, currentUserId);
                ValidateQuestion(currentGame!);
                ValidateAnswer(currentGame!);
                break;
            case GameEventType.QuestionRobbed:
                ValidateJoinedUser(currentGame!, currentUserId);
                ValidateQuestion(currentGame!);
                ValidateRobbingAnswer(currentGame!);
                break;
            case GameEventType.PlayerLeft:
                ValidateUser(currentUserId);
                break;
            case GameEventType.GameCancelled:
                ValidateGameMaster(currentGame!, currentUserId);
                break;
            case GameEventType.NextPlayerSelected:
                ValidateGameMaster(currentGame!, currentUserId);
                break;
            case GameEventType.QuestionRobbingIsAllowed:
                ValidateGameMaster(currentGame!, currentUserId);
                ValidateQuestion(currentGame!);
                break;
        }
    }

    private static void ValidateCurrentGame(CurrentGame? currentGame)
    {
        if (currentGame == default) throw new BusinessValidationException("Game was not found");
    }

    private void ValidateUser(long currentUserId)
    {
        if (UserId == default) throw new NoNullAllowedException("UserId must be provided");

        if (UserId != currentUserId) throw new BusinessValidationException("Identity theft is not a joke");
    }

    private void ValidateJoinedUser(CurrentGame currentGame, long currentUserId)
    {
        if (UserId == default) throw new NoNullAllowedException("UserId must be provided");

        if (currentGame.CurrentGameUsers.All(u => u.UserId != UserId) || currentUserId != UserId)
            throw new BusinessValidationException("User does not belong to this game");

        var currentQuestion = currentGame.CurrentGameQuestions.FirstOrDefault(q => q.IsCurrent);

        if (currentQuestion == default) return;
        if (currentQuestion.IsAnswered)
        {
            if (UserId != currentUserId) throw new BusinessValidationException("It is not your turn");
        }
        else
        {
            if (!currentQuestion.IsRobbingAllowed && UserId != currentUserId) throw new BusinessValidationException("It is not your turn");
        }
    }

    private void ValidateQuestion(CurrentGame currentGame)
    {
        if (QuestionId == default) throw new NoNullAllowedException("QuestionId must be provided");

        if (currentGame.CurrentGameQuestions.All(q => q.QuestionId != QuestionId))
            throw new BusinessValidationException("Question does not belong to this game");
    }

    private static void ValidateGameNotStarted(CurrentGame currentGame)
    {
        if (currentGame.IsStarted) throw new BusinessValidationException("Game has already been started");
    }

    private static void ValidatePreviousQuestionFinished(CurrentGame currentGame)
    {
        var previousQuestion = currentGame.CurrentGameQuestions.FirstOrDefault(q => q.IsCurrent);
        if (previousQuestion != default && !previousQuestion.IsAnswered) throw new BusinessValidationException("Previous question has not been answered");
    }

    private void ValidateAnswer(CurrentGame currentGame)
    {
        if (Answer == default) throw new NoNullAllowedException("Answer must be provided");
        if (currentGame.CurrentGameQuestions.First(q => q.QuestionId == QuestionId).IsAnswered)
            throw new BusinessValidationException("Question has already been answered");
    }

    private void ValidateRobbingAnswer(CurrentGame currentGame)
    {
        if (Answer == default) throw new NoNullAllowedException("Answer must be provided");
        var currentQuestion = currentGame.CurrentGameQuestions.First(q => q.QuestionId == QuestionId);
        if (currentQuestion.IsAnswered)
            throw new BusinessValidationException("Question has already been answered");
        if (!currentQuestion.IsRobbingAllowed)
            throw new BusinessValidationException("Question is not available for robbing");
    }

    private static void ValidateGameMaster(CurrentGame currentGame, long currentUserId)
    {
        var currentUser = currentGame.CurrentGameUsers.FirstOrDefault(u => u.UserId == currentUserId);
        if (currentUser == default)
            throw new BusinessValidationException("User does not belong to this game");

        if (!currentUser.IsGameMaster)
            throw new BusinessValidationException("Only game master can start the game");
    }
}