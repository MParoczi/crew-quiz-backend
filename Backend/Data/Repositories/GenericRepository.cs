using System.Linq.Expressions;
using System.Security.Claims;
using Backend.Interfaces.Data.Repositories;
using Backend.Models.Domains;
using Backend.Models.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data.Repositories;

public abstract class GenericRepository<TEntity>(CrewQuizContext context, IHttpContextAccessor httpContextAccessor)
    : IGenericRepository<TEntity>
    where TEntity : AuditableEntity
{
    public virtual async Task<IEnumerable<TEntity>> GetAllAsync()
    {
        return await context.Set<TEntity>().OrderBy(e => e.CreatedOn).ToListAsync();
    }

    public virtual async Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> expression)
    {
        return await context.Set<TEntity>().Where(expression).FirstOrDefaultAsync();
    }

    public virtual async Task<TEntity?> GetByIdAsync(object id)
    {
        return await context.Set<TEntity>().FindAsync(id);
    }

    public virtual async Task<TEntity> AddAsync(TEntity entity)
    {
        var createdEntity = await context.Set<TEntity>().AddAsync(entity);
        return createdEntity.Entity;
    }

    public virtual async Task IncludeCollectionAsync<TProperty>(TEntity entity, Expression<Func<TEntity, IEnumerable<TProperty>>> propertyExpression)
        where TProperty : class
    {
        await context.Entry(entity).Collection(propertyExpression).LoadAsync();
    }

    public virtual async Task IncludeReferenceAsync<TProperty>(TEntity entity, Expression<Func<TEntity, TProperty?>> propertyExpression) where TProperty : class
    {
        await context.Entry(entity).Reference(propertyExpression).LoadAsync();
    }

    public abstract Task<bool> UpdateAsync(TEntity entity);
    public abstract Task<bool> RemoveAsync(TEntity entity);

    protected long GetCurrentUserId()
    {
        var userIdClaim = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out var userId)) throw new BusinessValidationException("User is not authenticated");

        return userId;
    }
}