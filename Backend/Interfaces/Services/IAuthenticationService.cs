using Backend.Models.DTOs;

namespace Backend.Interfaces.Services;

public interface IAuthenticationService : IServiceBase
{
    public Task<AuthenticationDto> Login(AuthenticationDto authenticationDto);
    public Task<AuthenticationDto> Reauthenticate(AuthenticationDto authenticationDto);
}