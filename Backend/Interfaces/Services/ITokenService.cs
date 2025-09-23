using Backend.Models.Domains;

namespace Backend.Interfaces.Services;

public interface ITokenService : IServiceBase
{
    public string CreateToken(User user);
}