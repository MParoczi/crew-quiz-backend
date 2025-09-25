namespace Backend.Enums;

public enum GameEventType
{
    GameStarted,
    GameEnded,
    QuestionSelected,
    AnswerSubmitted,
    QuestionRobbingIsAllowed,
    QuestionRobbed,
    QuestionAnswered,
    QuestionAnsweredWrong,
    PlayerJoined,
    PlayerLeft,
    PlayerDisconnected,
    GameCancelled,
    NextPlayerSelected
}