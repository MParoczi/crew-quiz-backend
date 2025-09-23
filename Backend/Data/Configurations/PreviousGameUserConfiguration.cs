using Backend.Models.Domains;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Data.Configurations;

public class PreviousGameUserConfiguration : IEntityTypeConfiguration<PreviousGameUser>
{
    public void Configure(EntityTypeBuilder<PreviousGameUser> builder)
    {
        builder
            .HasKey(pgu => new { pgu.PreviousGameId, pgu.UserId });

        builder
            .HasIndex(pgu => pgu.PreviousGameId);
        builder
            .HasIndex(pgu => pgu.UserId);
        builder
            .HasIndex(pgu => new { pgu.PreviousGameId, pgu.UserId })
            .IsUnique();
        builder
            .HasIndex(pgu => pgu.Rank);

        builder
            .Property(pgu => pgu.PreviousGameId)
            .IsRequired()
            .HasColumnOrder(0);
        builder
            .Property(pgu => pgu.UserId)
            .IsRequired()
            .HasColumnOrder(1);
        builder
            .Property(pgu => pgu.Username)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnOrder(2);
        builder
            .Property(pgu => pgu.IsGameMaster)
            .IsRequired()
            .HasDefaultValue(false)
            .HasColumnOrder(3);
        builder
            .Property(pgu => pgu.Points)
            .IsRequired()
            .HasDefaultValue(0)
            .HasColumnOrder(4);
        builder
            .Property(pgu => pgu.Rank)
            .IsRequired()
            .HasColumnOrder(5);
        builder
            .Property(pgu => pgu.CreatedOn)
            .IsRequired()
            .HasColumnOrder(6);
        builder
            .Property(pgu => pgu.CreatedByUserId)
            .IsRequired()
            .HasColumnOrder(7);
        builder
            .Property(pgu => pgu.UpdatedOn)
            .HasColumnOrder(8);
        builder
            .Property(pgu => pgu.UpdatedByUserId)
            .HasColumnOrder(9);

        builder
            .HasOne(pgu => pgu.PreviousGame)
            .WithMany(pg => pg.PreviousGameUsers)
            .HasForeignKey(pgu => pgu.PreviousGameId);
        builder
            .HasOne(pgu => pgu.User)
            .WithMany(u => u.PreviousGameUsers)
            .HasForeignKey(pgu => pgu.UserId)
            .IsRequired();
        builder
            .HasOne(pgu => pgu.CreatedByUser)
            .WithMany()
            .HasForeignKey(pgu => pgu.CreatedByUserId)
            .IsRequired();
        builder
            .HasOne(pgu => pgu.UpdatedByUser)
            .WithMany(u => u.UpdatedPreviousGameUsers)
            .HasForeignKey(pgu => pgu.UpdatedByUserId);
    }
}