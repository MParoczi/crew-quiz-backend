using System.Security.Claims;
using Backend.Interfaces.Services;
using Backend.Models.Exceptions;

namespace Backend.Services;

public class ServiceBase(IHttpContextAccessor httpContextAccessor) : IServiceBase
{
    public long GetUserId()
    {
        var value = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (value != null)
            return long.Parse(value);

        throw new BusinessValidationException("User is not authenticated");
    }
}