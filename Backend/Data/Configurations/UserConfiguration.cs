using Backend.Models.Domains;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder
            .HasKey(u => u.UserId);

        builder
            .HasIndex(u => u.Username)
            .IsUnique();

        builder
            .Property(u => u.UserId)
            .IsRequired()
            .HasColumnOrder(0);
        builder
            .Property(u => u.FirstName)
            .IsRequired()
            .HasMaxLength(20)
            .HasColumnOrder(1);
        builder
            .Property(u => u.LastName)
            .IsRequired()
            .HasMaxLength(20)
            .HasColumnOrder(2);
        builder
            .Property(u => u.Username)
            .IsRequired()
            .HasMaxLength(20)
            .HasColumnOrder(3);
        builder
            .Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnOrder(4);
        builder
            .Property(u => u.CreatedOn)
            .IsRequired()
            .HasColumnOrder(5);
        builder
            .Property(u => u.CreatedByUserId)
            .HasColumnOrder(6);
        builder
            .Property(u => u.UpdatedOn)
            .HasColumnOrder(7);
        builder
            .Property(u => u.UpdatedByUserId)
            .HasColumnOrder(8);

        builder
            .HasOne(u => u.CreatedByUser)
            .WithMany(u => u.Users)
            .HasForeignKey(q => q.CreatedByUserId);
        builder
            .HasOne(q => q.UpdatedByUser)
            .WithMany(u => u.UpdatedUsers)
            .HasForeignKey(q => q.UpdatedByUserId);
    }
}