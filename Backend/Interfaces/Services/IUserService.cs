using Backend.Models.DTOs;

namespace Backend.Interfaces.Services;

public interface IUserService : IServiceBase
{
    public Task<UserDto> GetCurrentUser();
    public Task<UserDto> GetUserByUsername(string username);
    public Task CreateUser(AuthenticationDto authenticationDto);
    public Task UpdateUser(UserDto userDto);
    public Task DeleteUser(long id);
}