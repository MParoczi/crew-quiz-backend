namespace Backend.Models.Configurations;

public class Jwt
{
    public required string Secret { get; init; }
    public required int ExpirationInMinutes { get; init; }
    public required string Issuer { get; init; }
    public required string Audience { get; init; }
}