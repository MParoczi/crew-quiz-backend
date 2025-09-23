using AutoMapper;
using Backend.Interfaces.Data;
using Backend.Interfaces.Services;
using Backend.Interfaces.ServiceUtils;
using Backend.Models.Domains;
using Backend.Models.DTOs;
using Backend.Models.Exceptions;

namespace Backend.Services;

public class UserService(
    IHttpContextAccessor httpContextAccessor,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    IUserServiceUtil serviceUtil)
    : ServiceBase(httpContextAccessor), IUserService
{
    public async Task<UserDto> GetCurrentUser()
    {
        var userId = GetUserId();

        var user = await unitOfWork.Users.GetByIdAsync(userId);

        if (user == default) throw new BusinessValidationException("User was not found");

        var userDto = mapper.Map<UserDto>(user);
        userDto.PasswordHash = null; // Clear password hash for security in profile context

        return userDto;
    }

    public async Task<UserDto> GetUserByUsername(string username)
    {
        var user = await unitOfWork.Users.FirstOrDefaultAsync(u => u.Username == username);

        if (user == default) throw new BusinessValidationException("User was not found");

        return mapper.Map<UserDto>(user);
    }

    public async Task CreateUser(AuthenticationDto authenticationDto)
    {
        if (string.IsNullOrEmpty(authenticationDto.PasswordMd5)) throw new BusinessValidationException("Password must be provided");

        // Check for username uniqueness
        var existingUser = await unitOfWork.Users.FirstOrDefaultAsync(u => u.Username == authenticationDto.Username);
        if (existingUser != null) throw new BusinessValidationException("Username already exists");

        var user = mapper.Map<User>(authenticationDto);
        user.PasswordHash = serviceUtil.HashPassword(authenticationDto.PasswordMd5);
        await unitOfWork.Users.AddAsync(user);
        await unitOfWork.CompleteAsync();
    }

    public async Task UpdateUser(UserDto userDto)
    {
        var currentUserId = GetUserId();

        if (userDto.UserId != currentUserId) throw new BusinessValidationException("User cannot be modified by someone else");

        await unitOfWork.Users.UpdateAsync(mapper.Map<User>(userDto));
        await unitOfWork.CompleteAsync();
    }

    public async Task DeleteUser(long id)
    {
        var currentUserId = GetUserId();

        if (id != currentUserId) throw new BusinessValidationException("User cannot be deleted by someone else");

        var user = await unitOfWork.Users.GetByIdAsync(id);

        if (user == default) throw new BusinessValidationException("User not found");

        await unitOfWork.Users.RemoveAsync(user);
        await unitOfWork.CompleteAsync();
    }
}