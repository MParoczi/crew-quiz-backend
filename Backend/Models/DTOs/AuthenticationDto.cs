namespace Backend.Models.DTOs;

public class AuthenticationDto
{
    public long? UserId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public required string Username { get; set; }
    public string? PasswordMd5 { get; set; }
    public string? Token { get; set; }
}