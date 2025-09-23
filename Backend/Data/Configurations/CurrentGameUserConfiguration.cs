using Backend.Models.Domains;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Data.Configurations;

public class CurrentGameUserConfiguration : IEntityTypeConfiguration<CurrentGameUser>
{
    public void Configure(EntityTypeBuilder<CurrentGameUser> builder)
    {
        builder
            .HasKey(cgu => new { cgu.CurrentGameId, cgu.UserId });

        builder
            .HasIndex(cgu => cgu.CurrentGameId);
        builder
            .HasIndex(cgu => cgu.UserId);
        builder
            .HasIndex(cgu => new { cgu.CurrentGameId, cgu.UserId })
            .IsUnique();

        builder
            .Property(cgu => cgu.CurrentGameId)
            .IsRequired()
            .HasColumnOrder(0);
        builder
            .Property(cgu => cgu.UserId)
            .IsRequired()
            .HasColumnOrder(1);
        builder
            .Property(cgu => cgu.IsCurrent)
            .IsRequired()
            .HasDefaultValue(false)
            .HasColumnOrder(2);
        builder
            .Property(cgu => cgu.IsGameMaster)
            .IsRequired()
            .HasDefaultValue(false)
            .HasColumnOrder(3);
        builder
            .Property(cgu => cgu.Points)
            .IsRequired()
            .HasDefaultValue(0)
            .HasColumnOrder(4);
        builder
            .Property(cgu => cgu.CreatedOn)
            .IsRequired()
            .HasColumnOrder(5);
        builder
            .Property(cgu => cgu.CreatedByUserId)
            .IsRequired()
            .HasColumnOrder(6);
        builder
            .Property(cgu => cgu.UpdatedOn)
            .HasColumnOrder(7);
        builder
            .Property(cgu => cgu.UpdatedByUserId)
            .HasColumnOrder(8);

        builder
            .HasOne(cgu => cgu.CurrentGame)
            .WithMany(cg => cg.CurrentGameUsers)
            .HasForeignKey(cgu => cgu.CurrentGameId);
        builder
            .HasOne(cgu => cgu.User)
            .WithOne(u => u.CurrentGameUser)
            .HasForeignKey<CurrentGameUser>(cgu => cgu.UserId)
            .IsRequired();
        builder
            .HasOne(cgu => cgu.CreatedByUser)
            .WithMany(u => u.CurrentGameUsers)
            .HasForeignKey(cgu => cgu.CreatedByUserId)
            .IsRequired();
        builder
            .HasOne(cgu => cgu.UpdatedByUser)
            .WithMany(u => u.UpdatedCurrentGameUsers)
            .HasForeignKey(cgu => cgu.UpdatedByUserId);
    }
}