using Backend.Models.DTOs;

namespace Backend.Interfaces.ServiceUtils;

public interface IAuthenticationServiceUtil : IServiceUtilBase
{
    public string CreateToken(UserDto user);
    public bool VerifyPassword(string passwordMd5, string passwordHash);
}