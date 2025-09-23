namespace Backend.Models.Domains;

public class User : AuditableEntity
{
    public long UserId { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Username { get; set; }
    public required string PasswordHash { get; set; }

    public List<User> Users { get; } = [];
    public List<User> UpdatedUsers { get; set; } = [];
    public List<Quiz> Quizzes { get; } = [];
    public List<Quiz> UpdatedQuizzes { get; set; } = [];
    public List<QuestionGroup> QuestionGroups { get; } = [];
    public List<QuestionGroup> UpdatedQuestionGroups { get; } = [];
    public List<Question> Questions { get; } = [];
    public List<Question> UpdatedQuestions { get; } = [];
    public List<CurrentGame> CurrentGames { get; } = [];
    public List<CurrentGame> UpdatedCurrentGames { get; } = [];
    public CurrentGameUser? CurrentGameUser { get; set; }
    public List<CurrentGameUser> CurrentGameUsers { get; } = [];
    public List<CurrentGameUser> UpdatedCurrentGameUsers { get; } = [];
    public List<CurrentGameQuestion> CurrentGameQuestions { get; } = [];
    public List<CurrentGameQuestion> UpdatedCurrentGameQuestions { get; } = [];
    public List<CurrentGameQuestion> AnsweredCurrentGameQuestions { get; } = [];
    public List<CurrentGameQuestion> UpdatedAnsweredCurrentGameQuestions { get; } = [];
    public List<QuestionGroupQuiz> QuestionGroupQuizzes { get; } = [];
    public List<QuestionGroupQuiz> UpdatedQuestionGroupQuizzes { get; } = [];
    public List<PreviousGame> PreviousGames { get; } = [];
    public List<PreviousGame> UpdatedPreviousGames { get; } = [];
    public List<PreviousGameUser> PreviousGameUsers { get; } = [];
    public List<PreviousGameUser> UpdatedPreviousGameUsers { get; } = [];
}