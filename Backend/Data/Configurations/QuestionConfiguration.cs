using Backend.Models.Domains;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Data.Configurations;

public class QuestionConfiguration : IEntityTypeConfiguration<Question>
{
    public void Configure(EntityTypeBuilder<Question> builder)
    {
        builder
            .HasKey(q => q.QuestionId);

        builder
            .HasIndex(q => q.CreatedByUserId);
        builder
            .HasIndex(q => q.QuestionGroupId);
        builder
            .HasIndex(q => new { q.QuestionGroupId, q.Inquiry })
            .IsUnique();

        builder
            .Property(q => q.QuestionId)
            .IsRequired()
            .HasColumnOrder(0);
        builder
            .Property(q => q.Inquiry)
            .IsRequired()
            .HasMaxLength(500)
            .HasColumnOrder(1);
        builder
            .Property(q => q.Answer)
            .IsRequired()
            .HasMaxLength(50)
            .HasColumnOrder(2);
        builder
            .Property(q => q.Point)
            .IsRequired()
            .HasColumnOrder(3);
        builder
            .Property(q => q.QuestionGroupId)
            .IsRequired()
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
            .HasOne(q => q.QuestionGroup)
            .WithMany(qg => qg.Questions)
            .HasForeignKey(q => q.QuestionGroupId)
            .IsRequired();
        builder
            .HasOne(q => q.CreatedByUser)
            .WithMany(u => u.Questions)
            .HasForeignKey(q => q.CreatedByUserId)
            .IsRequired();
        builder
            .HasOne(q => q.UpdatedByUser)
            .WithMany(u => u.UpdatedQuestions)
            .HasForeignKey(q => q.UpdatedByUserId);
    }
}