using System.Text.Json.Serialization;

namespace Backend.Models.DTOs;

public class UserDto
{
    public long UserId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public required string Username { get; set; }

    [JsonIgnore]
    public string PasswordHash { get; set; }
}