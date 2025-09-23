namespace Backend.Models.Domains;

public class CurrentGameUser : AuditableEntity
{
    public required long CurrentGameId { get; set; }
    public required long UserId { get; set; }
    public bool IsCurrent { get; set; }
    public bool IsGameMaster { get; set; }
    public int Points { get; set; }

    public CurrentGame CurrentGame { get; set; }
    public User User { get; set; }
}