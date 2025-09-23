using System.Security.Authentication;
using System.Security.Claims;
using Backend.Extensions;
using Backend.Models.Domains;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Backend.Data;

public class AuditInterceptor(IHttpContextAccessor httpContextAccessor) : SaveChangesInterceptor
{
    private readonly List<User> _newUsers = new();

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        UpdateEntities(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        UpdateEntities(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        UpdateUserAuditInfo(eventData.Context);
        return base.SavedChanges(eventData, result);
    }

    public override ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
    {
        UpdateUserAuditInfo(eventData.Context);
        return base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    private void UpdateEntities(DbContext? context)
    {
        if (context == null) return;

        var userClaim = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userIdParsed = long.TryParse(userClaim, out var userId);
        var now = DateTime.UtcNow;

        _newUsers.Clear();

        var entries = context.ChangeTracker.Entries<AuditableEntity>().ToList();

        foreach (var entry in entries)
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedOn = now;

                if (entry.Entity is User user)
                {
                    _newUsers.Add(user);
                }
                else
                {
                    if (userIdParsed)
                        entry.Entity.CreatedByUserId = userId;
                    else
                        throw new AuthenticationException("User must be authenticated to perform this operation");
                }
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedOn = now;

                if (userIdParsed)
                    entry.Entity.UpdatedByUserId = userId;
                else if (entry.Entity is not User)
                    throw new AuthenticationException("User must be authenticated to perform this operation");
            }
    }

    private void UpdateUserAuditInfo(DbContext? context)
    {
        if (context == null || _newUsers.Count == 0) return;

        foreach (var user in _newUsers) user.CreatedByUserId = user.UserId;

        context.SaveChangesWithoutInterception();
    }
}