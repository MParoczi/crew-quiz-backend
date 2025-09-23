using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Backend.Extensions;

public static class DbContextExtensions
{
    public static int SaveChangesWithoutInterception(this DbContext context)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));

        var originalOptions = context.GetService<IDbContextOptions>();
        var optionsBuilder = new DbContextOptionsBuilder();

        foreach (var extension in originalOptions.Extensions.Where(e => !(e is CoreOptionsExtension)))
            ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);

        var newCoreExtension = new CoreOptionsExtension();

        ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(newCoreExtension);


        using var tempContext = context.GetType().GetConstructor(new[] { typeof(DbContextOptions) })?.Invoke(new[] { optionsBuilder.Options }) as DbContext;

        if (tempContext == null) return context.SaveChanges(true);

        foreach (var entry in context.ChangeTracker.Entries())
        {
            var tempEntry = tempContext.Entry(entry.Entity);
            tempEntry.State = entry.State;
        }

        return tempContext.SaveChanges(true);
    }
}