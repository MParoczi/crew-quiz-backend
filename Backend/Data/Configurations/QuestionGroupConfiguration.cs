using Backend.Models.Domains;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Data.Configurations;

public class QuestionGroupConfiguration : IEntityTypeConfiguration<QuestionGroup>
{
    public void Configure(EntityTypeBuilder<QuestionGroup> builder)
    {
        builder
            .HasKey(qg => qg.QuestionGroupId);

        builder
            .HasIndex(qg => qg.CreatedByUserId);
        builder
            .HasIndex(qg => new { qg.CreatedByUserId, qg.Name })
            .IsUnique();

        builder
            .Property(qg => qg.QuestionGroupId)
            .IsRequired()
            .HasColumnOrder(0);
        builder
            .Property(qg => qg.Name)
            .IsRequired()
            .HasMaxLength(50)
            .HasColumnOrder(1);
        builder
            .Property(qg => qg.Description)
            .HasMaxLength(500)
            .HasColumnOrder(2);
        builder
            .Property(qg => qg.CreatedOn)
            .IsRequired()
            .HasColumnOrder(3);
        builder
            .Property(qg => qg.CreatedByUserId)
            .IsRequired()
            .HasColumnOrder(4);
        builder
            .Property(qg => qg.UpdatedOn)
            .HasColumnOrder(5);
        builder
            .Property(qg => qg.UpdatedByUserId)
            .HasColumnOrder(6);

        builder
            .HasMany(qg => qg.Quizzes)
            .WithMany(q => q.QuestionGroups)
            .UsingEntity<QuestionGroupQuiz>();
        builder
            .HasOne(qg => qg.CreatedByUser)
            .WithMany(u => u.QuestionGroups)
            .HasForeignKey(qg => qg.CreatedByUserId)
            .IsRequired();
        builder
            .HasOne(qg => qg.UpdatedByUser)
            .WithMany(u => u.UpdatedQuestionGroups)
            .HasForeignKey(qg => qg.UpdatedByUserId);
    }
}