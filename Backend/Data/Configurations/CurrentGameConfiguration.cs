using Backend.Models.Domains;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Data.Configurations;

public class CurrentGameConfiguration : IEntityTypeConfiguration<CurrentGame>
{
    public void Configure(EntityTypeBuilder<CurrentGame> builder)
    {
        builder
            .HasKey(cg => cg.CurrentGameId);

        builder
            .HasIndex(cg => cg.CreatedByUserId);
        builder
            .HasIndex(cg => cg.QuizId);
        builder
            .HasIndex(cg => cg.SessionId)
            .IsUnique();

        builder
            .Property(cg => cg.CurrentGameId)
            .IsRequired()
            .HasColumnOrder(0);
        builder
            .Property(cg => cg.SessionId)
            .IsRequired()
            .HasMaxLength(8)
            .HasColumnOrder(1);
        builder
            .Property(cg => cg.QuizId)
            .IsRequired()
            .HasColumnOrder(2);
        builder
            .Property(cg => cg.IsStarted)
            .IsRequired()
            .HasDefaultValue(false)
            .HasColumnOrder(3);
        builder
            .Property(cg => cg.IsCompleted)
            .HasDefaultValue(false)
            .HasColumnOrder(4);
        builder
            .Property(q => q.CreatedOn)
            .IsRequired()
            .HasColumnOrder(5);
        builder
            .Property(q => q.CreatedByUserId)
            .IsRequired()
            .HasColumnOrder(6);
        builder
            .Property(q => q.UpdatedOn)
            .HasColumnOrder(7);
        builder
            .Property(q => q.UpdatedByUserId)
            .HasColumnOrder(8);

        builder
            .HasOne(cg => cg.Quiz)
            .WithMany(q => q.CurrentGames)
            .HasForeignKey(cg => cg.QuizId)
            .IsRequired();
        builder
            .HasOne(cg => cg.CreatedByUser)
            .WithMany(u => u.CurrentGames)
            .HasForeignKey(cg => cg.CreatedByUserId)
            .IsRequired();
        builder
            .HasOne(cg => cg.UpdatedByUser)
            .WithMany(u => u.UpdatedCurrentGames)
            .HasForeignKey(cg => cg.UpdatedByUserId);
    }
}