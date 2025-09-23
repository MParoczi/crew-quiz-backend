using System.Linq.Expressions;
using Backend.Models.Domains;

namespace Backend.Interfaces.Data.Repositories;

public interface IPreviousGameRepository : IGenericRepository<PreviousGame>
{
    Task<PreviousGame?> GetPreviousGameWithIncludesAsync(Expression<Func<PreviousGame, bool>> expression);
    Task<IEnumerable<PreviousGame>> GetPreviousGamesByUserIdAsync(long userId);
}