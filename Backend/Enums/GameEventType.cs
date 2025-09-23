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
    PlayerJoined,
    PlayerLeft,
    PlayerDisconnected,
    GameCancelled,
    NextPlayerSelected
}