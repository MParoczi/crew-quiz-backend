using System.Linq.Expressions;
using Backend.Models.Domains;

namespace Backend.Interfaces.Data.Repositories;

public interface IGenericRepository<TEntity> where TEntity : AuditableEntity
{
    Task<IEnumerable<TEntity>> GetAllAsync();
    Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> expression);
    Task<TEntity?> GetByIdAsync(object id);
    Task<TEntity> AddAsync(TEntity entity);
    Task<bool> UpdateAsync(TEntity entity);
    Task<bool> RemoveAsync(TEntity entity);

    Task IncludeCollectionAsync<TProperty>(TEntity entity, Expression<Func<TEntity, IEnumerable<TProperty>>> propertyExpression)
        where TProperty : class;

    Task IncludeReferenceAsync<TProperty>(TEntity entity, Expression<Func<TEntity, TProperty?>> propertyExpression) where TProperty : class;
}