using Backend.Models.Domains;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Data.Configurations;

public class CurrentGameQuestionConfiguration : IEntityTypeConfiguration<CurrentGameQuestion>
{
    public void Configure(EntityTypeBuilder<CurrentGameQuestion> builder)
    {
        builder
            .HasKey(cgq => new { cgq.CurrentGameId, cgq.QuestionId });

        builder
            .HasIndex(cgq => cgq.CurrentGameId);
        builder
            .HasIndex(cgq => cgq.QuestionId);
        builder
            .HasIndex(cgq => cgq.AnsweredByUserId);

        builder
            .Property(cgq => cgq.CurrentGameId)
            .IsRequired()
            .HasColumnOrder(0);
        builder
            .Property(cgq => cgq.QuestionId)
            .IsRequired()
            .HasColumnOrder(1);
        builder
            .Property(cgq => cgq.IsAnswered)
            .HasDefaultValue(false)
            .IsRequired()
            .HasColumnOrder(2);
        builder
            .Property(cgq => cgq.IsCurrent)
            .IsRequired()
            .HasDefaultValue(false)
            .HasColumnOrder(3);
        builder
            .Property(cgq => cgq.IsRobbingAllowed)
            .IsRequired()
            .HasDefaultValue(false)
            .HasColumnOrder(4);
        builder
            .Property(cgq => cgq.AnsweredByUserId)
            .HasColumnOrder(5);
        builder
            .Property(cgq => cgq.CreatedOn)
            .IsRequired()
            .HasColumnOrder(6);
        builder
            .Property(cgq => cgq.CreatedByUserId)
            .IsRequired()
            .HasColumnOrder(7);
        builder
            .Property(cgq => cgq.UpdatedOn)
            .HasColumnOrder(8);
        builder
            .Property(cgq => cgq.UpdatedByUserId)
            .HasColumnOrder(9);

        builder
            .HasOne(cgq => cgq.CurrentGame)
            .WithMany(cg => cg.CurrentGameQuestions)
            .HasForeignKey(cgq => cgq.CurrentGameId);
        builder
            .HasOne(cgq => cgq.Question)
            .WithMany(q => q.CurrentGameQuestions)
            .HasForeignKey(cgq => cgq.QuestionId);
        builder
            .HasOne(cgq => cgq.AnsweredByUser)
            .WithMany(q => q.AnsweredCurrentGameQuestions)
            .HasForeignKey(cgq => cgq.AnsweredByUserId);
        builder
            .HasOne(cgq => cgq.CreatedByUser)
            .WithMany(u => u.CurrentGameQuestions)
            .HasForeignKey(cgq => cgq.CreatedByUserId)
            .IsRequired();
        builder
            .HasOne(cgq => cgq.UpdatedByUser)
            .WithMany(u => u.UpdatedCurrentGameQuestions)
            .HasForeignKey(cgq => cgq.UpdatedByUserId);
    }
}