using Backend.Interfaces.Services;
using Backend.Interfaces.Utils;
using Backend.Models.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]/[action]")]
public class UserController(IServiceDispatcher serviceDispatcher) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<UserDto>> GetCurrentUser()
    {
        var user = await serviceDispatcher.For<IUserService>().DispatchAsync(s => s.GetCurrentUser());
        return Ok(user);
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult> CreateUser(AuthenticationDto authenticationDto)
    {
        await serviceDispatcher.For<IUserService>().DispatchAsync(s => s.CreateUser(authenticationDto));
        return Ok();
    }

    [HttpPut]
    public async Task<ActionResult> UpdateUser(UserDto user)
    {
        await serviceDispatcher.For<IUserService>().DispatchAsync(s => s.UpdateUser(user));
        return Ok();
    }

    [HttpDelete("{userId:long}")]
    public async Task<ActionResult> DeleteUser(long userId)
    {
        await serviceDispatcher.For<IUserService>().DispatchAsync(s => s.DeleteUser(userId));
        return Ok();
    }
}