using Backend.Models.Domains;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Data.Configurations;

public class QuestionGroupQuizConfiguration : IEntityTypeConfiguration<QuestionGroupQuiz>
{
    public void Configure(EntityTypeBuilder<QuestionGroupQuiz> builder)
    {
        builder
            .HasKey(qgq => new { qgq.QuestionGroupId, qgq.QuizId });

        builder
            .HasIndex(qgq => qgq.QuestionGroupId);
        builder
            .HasIndex(qgq => qgq.QuizId);
        builder
            .HasIndex(qgq => qgq.CreatedByUserId);
        builder
            .HasIndex(qgq => new { qgq.QuestionGroupId, qgq.QuizId })
            .IsUnique();

        builder
            .Property(qgq => qgq.QuestionGroupId)
            .IsRequired()
            .HasColumnOrder(0);
        builder
            .Property(qgq => qgq.QuizId)
            .IsRequired()
            .HasColumnOrder(1);
        builder
            .Property(qgq => qgq.CreatedOn)
            .IsRequired()
            .HasColumnOrder(2);
        builder
            .Property(qgq => qgq.CreatedByUserId)
            .IsRequired()
            .HasColumnOrder(3);
        builder
            .Property(qgq => qgq.UpdatedOn)
            .HasColumnOrder(4);
        builder
            .Property(qgq => qgq.UpdatedByUserId)
            .HasColumnOrder(5);

        builder
            .HasOne(qgq => qgq.CreatedByUser)
            .WithMany(u => u.QuestionGroupQuizzes)
            .HasForeignKey(qgq => qgq.CreatedByUserId)
            .IsRequired();
        builder
            .HasOne(qgq => qgq.UpdatedByUser)
            .WithMany(u => u.UpdatedQuestionGroupQuizzes)
            .HasForeignKey(qgq => qgq.UpdatedByUserId);
    }
}