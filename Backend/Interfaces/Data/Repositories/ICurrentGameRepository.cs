using System.Linq.Expressions;
using Backend.Models.Domains;

namespace Backend.Interfaces.Data.Repositories;

public interface ICurrentGameRepository : IGenericRepository<CurrentGame>
{
    Task<CurrentGame?> GetCurrentGameWithIncludesAsync(Expression<Func<CurrentGame, bool>> expression);
}