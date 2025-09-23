using Backend.Extensions;
using Backend.Interfaces.Services;
using Backend.Interfaces.ServiceUtils;
using Backend.Interfaces.Utils;
using Backend.Models.DTOs;
using Backend.Models.Exceptions;
using Backend.Utils;

namespace Backend.Services;

public class AuthenticationService(
    IHttpContextAccessor httpContextAccessor,
    IServiceDispatcher serviceDispatcher,
    IAuthenticationServiceUtil serviceUtil)
    : ServiceBase(httpContextAccessor), IAuthenticationService
{
    public async Task<AuthenticationDto> Login(AuthenticationDto authenticationDto)
    {
        var clonedAuthenticationDto = Utility.DeepClone(authenticationDto);

        if (clonedAuthenticationDto.PasswordMd5 == default) throw new BusinessValidationException("Password must be provided");

        var userToLogin = await serviceDispatcher.For<IUserService>().DispatchAsync(s => s.GetUserByUsername(clonedAuthenticationDto.Username));

        if (userToLogin == default) throw new BusinessValidationException("User with this username was not found");

        using (httpContextAccessor.HttpContext?.EnrichWithUserContext((int)userToLogin.UserId, userToLogin.Username))
        {
            if (!serviceUtil.VerifyPassword(clonedAuthenticationDto.PasswordMd5, userToLogin.PasswordHash))
                throw new BusinessValidationException("Invalid password");

            clonedAuthenticationDto.UserId = userToLogin.UserId;
            clonedAuthenticationDto.Token = serviceUtil.CreateToken(userToLogin);
            clonedAuthenticationDto.PasswordMd5 = null;

            return clonedAuthenticationDto;
        }
    }

    public async Task<AuthenticationDto> Reauthenticate(AuthenticationDto authenticationDto)
    {
        var clonedAuthenticationDto = Utility.DeepClone(authenticationDto);

        var userToLogin = await serviceDispatcher.For<IUserService>().DispatchAsync(s => s.GetUserByUsername(clonedAuthenticationDto.Username));

        if (userToLogin == default) throw new BusinessValidationException("User with this username was not found");

        using (httpContextAccessor.HttpContext?.EnrichWithUserContext((int)userToLogin.UserId, userToLogin.Username))
        {
            clonedAuthenticationDto.UserId = userToLogin.UserId;
            clonedAuthenticationDto.Token = serviceUtil.CreateToken(userToLogin);

            return clonedAuthenticationDto;
        }
    }
}