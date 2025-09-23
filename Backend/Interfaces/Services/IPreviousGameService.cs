using Backend.Models.DTOs;

namespace Backend.Interfaces.Services;

public interface IPreviousGameService : IServiceBase
{
    Task ArchiveFinishedGame(long currentGameId);
    Task<IEnumerable<PreviousGameDto>> GetPreviousGamesForCurrentUser();
    Task<PreviousGameDto?> GetPreviousGame(string sessionId);
}