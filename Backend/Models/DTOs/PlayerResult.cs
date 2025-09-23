namespace Backend.Models.DTOs;

public class PlayerResult
{
    public required long UserId { get; set; }
    public required string Username { get; set; }
    public required int Points { get; set; }
    public required int Rank { get; set; }
    public required bool IsGameMaster { get; set; }
}