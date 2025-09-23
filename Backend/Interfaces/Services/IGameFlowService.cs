using Backend.Models.DTOs;

namespace Backend.Interfaces.Services;

public interface IGameFlowService : IServiceBase
{
    public Task AddUserToCurrentGame(GameFlowDto gameFlowDto);
    public Task StartGame(GameFlowDto gameFlowDto);
    public Task SelectQuestion(GameFlowDto gameFlowDto);
    public Task SubmitAnswer(GameFlowDto gameFlowDto);
    public Task RobQuestion(GameFlowDto gameFlowDto);
    public Task LeaveGame(GameFlowDto gameFlowDto);
    public Task CancelGame(GameFlowDto gameFlowDto);
    public Task SelectNextPlayer(GameFlowDto gameFlowDto);
    public Task AllowRobbing(GameFlowDto gameFlowDto);
}