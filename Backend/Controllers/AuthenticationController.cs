using Backend.Interfaces.Services;
using Backend.Interfaces.Utils;
using Backend.Models.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]/[action]")]
public class AuthenticationController(IServiceDispatcher serviceDispatcher) : ControllerBase
{
    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult<AuthenticationDto>> Login(AuthenticationDto authenticationDto)
    {
        var token = await serviceDispatcher.For<IAuthenticationService>().DispatchAsync(s => s.Login(authenticationDto));
        return Ok(token);
    }

    [HttpPost]
    public async Task<ActionResult<AuthenticationDto>> Reauthenticate(AuthenticationDto authenticationDto)
    {
        var token = await serviceDispatcher.For<IAuthenticationService>().DispatchAsync(s => s.Reauthenticate(authenticationDto));
        return Ok(token);
    }
}