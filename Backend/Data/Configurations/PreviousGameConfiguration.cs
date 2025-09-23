using Backend.Models.Domains;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Data.Configurations;

public class PreviousGameConfiguration : IEntityTypeConfiguration<PreviousGame>
{
    public void Configure(EntityTypeBuilder<PreviousGame> builder)
    {
        builder
            .HasKey(pg => pg.PreviousGameId);

        builder
            .HasIndex(pg => pg.CreatedByUserId);
        builder
            .HasIndex(pg => pg.SessionId);
        builder
            .HasIndex(pg => pg.CompletedOn);

        builder
            .Property(pg => pg.PreviousGameId)
            .IsRequired()
            .HasColumnOrder(0);
        builder
            .Property(pg => pg.SessionId)
            .IsRequired()
            .HasMaxLength(8)
            .HasColumnOrder(1);
        builder
            .Property(pg => pg.QuizName)
            .IsRequired()
            .HasMaxLength(500)
            .HasColumnOrder(2);
        builder
            .Property(pg => pg.CompletedOn)
            .IsRequired()
            .HasColumnOrder(3);
        builder
            .Property(pg => pg.CreatedOn)
            .IsRequired()
            .HasColumnOrder(4);
        builder
            .Property(pg => pg.CreatedByUserId)
            .IsRequired()
            .HasColumnOrder(5);
        builder
            .Property(pg => pg.UpdatedOn)
            .HasColumnOrder(6);
        builder
            .Property(pg => pg.UpdatedByUserId)
            .HasColumnOrder(7);

        builder
            .HasOne(pg => pg.CreatedByUser)
            .WithMany(u => u.PreviousGames)
            .HasForeignKey(pg => pg.CreatedByUserId)
            .IsRequired();
        builder
            .HasOne(pg => pg.UpdatedByUser)
            .WithMany(u => u.UpdatedPreviousGames)
            .HasForeignKey(pg => pg.UpdatedByUserId);
    }
}