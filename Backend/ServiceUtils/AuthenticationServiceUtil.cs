using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Backend.Constants;
using Backend.Interfaces.ServiceUtils;
using Backend.Models.Configurations;
using Backend.Models.DTOs;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Backend.ServiceUtils;

public class AuthenticationServiceUtil(
    IOptions<AppSettings> apiSettings) : ServiceUtilBase, IAuthenticationServiceUtil
{
    private readonly AppSettings _appSettings = apiSettings.Value;

    public string CreateToken(UserDto user)
    {
        var secretKey = _appSettings.Jwt.Secret;
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var expirationTime = DateTime.UtcNow.AddMinutes(_appSettings.Jwt.ExpirationInMinutes);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(
            [
                new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
                new Claim(JwtRegisteredClaimNames.Name, user.Username)
            ]),
            IssuedAt = DateTime.UtcNow,
            Expires = expirationTime,
            SigningCredentials = credentials,
            Issuer = _appSettings.Jwt.Issuer,
            Audience = _appSettings.Jwt.Audience
        };

        var handler = new JsonWebTokenHandler();
        var token = handler.CreateToken(tokenDescriptor);

        return token;
    }

    public bool VerifyPassword(string passwordMd5, string passwordHash)
    {
        var parts = passwordHash.Split('-');
        if (parts.Length != 2) return false;

        var hash = Convert.FromHexString(parts[0]);
        var salt = Convert.FromHexString(parts[1]);

        var inputHash = Rfc2898DeriveBytes.Pbkdf2(passwordMd5, salt, Cryptography.Iterations, Cryptography.Algorithm, Cryptography.HashSize);

        var isValidPassword = CryptographicOperations.FixedTimeEquals(hash, inputHash);

        return isValidPassword;
    }
}