using Backend.Models.DTOs;

namespace Backend.Interfaces.Services;

public interface ICurrentGameService : IServiceBase
{
    public Task<CurrentGameDto> GetCurrentGameForCurrentUser();
    public Task<CurrentGameDto> GetCurrentGame(string sessionId);
    public Task CreateCurrentGame(CurrentGameDto currentGame);
    public Task UpdateCurrentGame(CurrentGameDto currentGame);
    public Task DeleteCurrentGame(long currentGameId);
}