using Backend.Models.Domains;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Data.Configurations;

public class QuizConfiguration : IEntityTypeConfiguration<Quiz>
{
    public void Configure(EntityTypeBuilder<Quiz> builder)
    {
        builder
            .HasKey(q => q.QuizId);

        builder
            .HasIndex(q => q.CreatedByUserId);
        builder
            .HasIndex(q => new { q.Name, q.CreatedByUserId })
            .IsUnique();

        builder
            .Property(q => q.QuizId)
            .IsRequired()
            .HasColumnOrder(0);
        builder
            .Property(q => q.Name)
            .HasMaxLength(50)
            .IsRequired()
            .HasColumnOrder(1);
        builder
            .Property(q => q.CreatedOn)
            .IsRequired()
            .HasColumnOrder(2);
        builder
            .Property(q => q.CreatedByUserId)
            .IsRequired()
            .HasColumnOrder(3);
        builder
            .Property(q => q.UpdatedOn)
            .HasColumnOrder(4);
        builder
            .Property(q => q.UpdatedByUserId)
            .HasColumnOrder(5);

        builder
            .HasMany(q => q.QuestionGroups)
            .WithMany(qg => qg.Quizzes)
            .UsingEntity<QuestionGroupQuiz>();
        builder
            .HasOne(q => q.CreatedByUser)
            .WithMany(u => u.Quizzes)
            .HasForeignKey(q => q.CreatedByUserId)
            .IsRequired();
        builder
            .HasOne(q => q.UpdatedByUser)
            .WithMany(u => u.UpdatedQuizzes)
            .HasForeignKey(q => q.UpdatedByUserId);
    }
}