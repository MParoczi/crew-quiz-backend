namespace Backend.Models.DTOs;

public class CurrentGameUserDto
{
    public bool IsCurrent { get; set; }
    public bool IsGameMaster { get; set; }
    public int Points { get; set; }
    public required UserDto User { get; set; }
}