using Backend.Enums;
using Backend.Models.Domains;
using Backend.Models.DTOs;

namespace Backend.Interfaces.ServiceUtils;

public interface IGameFlowServiceUtil : IServiceUtilBase
{
    public Task SendEventToGroup(GameFlowDto gameFlowDto, GameEventType eventType);
    public Task UpdateGameFlow(GameFlowDto gameFlowDto, GameEventType eventType);
    public Task<CurrentGame> GetAndValidateCurrentGame(GameEventType gameEventType, GameFlowDto gameFlowDto, long currentUserId);
    public Task<CurrentGame> SelectNextPlayer(CurrentGame currentGame);
    public Task<CurrentGame> SelectNextQuestion(CurrentGame currentGame, long questionId);
    public Task<CurrentGame> SetQuestionAnswered(GameFlowDto gameFlowDto, CurrentGame currentGame, CurrentGameQuestion currentQuestion);
    public Task<CurrentGame> SetQuestionRobbingAllowed(CurrentGame currentGame, CurrentGameQuestion currentQuestion);
    public Task<CurrentGame> SetQuestionRobbedByUser(GameFlowDto gameFlowDto, CurrentGame currentGame, CurrentGameQuestion currentQuestion);
    public bool CheckIfLastQuestion(CurrentGame currentGame);
    public Task<GameFlowDto> CompleteGame(GameFlowDto gameFlowDto, CurrentGame currentGame);
}