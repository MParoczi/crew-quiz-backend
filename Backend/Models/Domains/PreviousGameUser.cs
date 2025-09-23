namespace Backend.Models.Domains;

public class PreviousGameUser : AuditableEntity
{
    public required long PreviousGameId { get; set; }
    public required long UserId { get; set; }
    public required string Username { get; set; }
    public bool IsGameMaster { get; set; }
    public int Points { get; set; }
    public int Rank { get; set; }

    public PreviousGame PreviousGame { get; set; }
    public User User { get; set; }
}