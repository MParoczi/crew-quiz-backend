namespace Backend.Models.DTOs;

public class PreviousGameUserDto
{
    public long UserId { get; set; }
    public required string Username { get; set; }
    public bool IsGameMaster { get; set; }
    public int Points { get; set; }
    public int Rank { get; set; }
}