using Backend.Data.Configurations;
using Backend.Models.Domains;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data;

public class CrewQuizContext(DbContextOptions<CrewQuizContext> options)
    : DbContext(options)
{
    public virtual DbSet<CurrentGame> CurrentGame { get; set; }
    public virtual DbSet<Quiz> Quiz { get; set; }
    public virtual DbSet<QuestionGroup> QuestionGroup { get; set; }
    public virtual DbSet<Question> Question { get; set; }
    public virtual DbSet<User> User { get; set; }
    public virtual DbSet<QuestionGroupQuiz> QuestionGroupQuiz { get; set; }
    public virtual DbSet<CurrentGameQuestion> CurrentGameQuestion { get; set; }
    public virtual DbSet<CurrentGameUser> CurrentGameUser { get; set; }
    public virtual DbSet<PreviousGame> PreviousGame { get; set; }
    public virtual DbSet<PreviousGameUser> PreviousGameUser { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new QuizConfiguration());
        modelBuilder.ApplyConfiguration(new QuestionGroupConfiguration());
        modelBuilder.ApplyConfiguration(new QuestionConfiguration());
        modelBuilder.ApplyConfiguration(new QuestionGroupQuizConfiguration());
        modelBuilder.ApplyConfiguration(new CurrentGameConfiguration());
        modelBuilder.ApplyConfiguration(new CurrentGameQuestionConfiguration());
        modelBuilder.ApplyConfiguration(new CurrentGameUserConfiguration());
        modelBuilder.ApplyConfiguration(new PreviousGameConfiguration());
        modelBuilder.ApplyConfiguration(new PreviousGameUserConfiguration());
    }
}