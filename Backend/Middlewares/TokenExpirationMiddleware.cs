using System.IdentityModel.Tokens.Jwt;

namespace Backend.Middlewares;

public class TokenExpirationMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext httpContext)
    {
        var endpoint = httpContext.GetEndpoint();
        if (endpoint != null)
        {
            var hasAuthorizeAttribute = endpoint.Metadata.Any(m => m is AuthorizeAttribute);
            var hasAllowAnonymousAttribute = endpoint.Metadata.Any(m => m is AllowAnonymousAttribute);

            if (hasAuthorizeAttribute && !hasAllowAnonymousAttribute)
            {
                var authorizationHeader = httpContext.Request.Headers.Authorization.FirstOrDefault();

                if (authorizationHeader != null && authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    var token = authorizationHeader.Substring("Bearer ".Length).Trim();

                    if (IsTokenExpired(token))
                    {
                        httpContext.Response.StatusCode = 401;
                        await httpContext.Response.WriteAsJsonAsync("Token has expired.");
                        return;
                    }
                }
            }
        }

        await next(httpContext);
    }

    private static bool IsTokenExpired(string token)
    {
        var handler = new JwtSecurityTokenHandler();

        if (handler.ReadToken(token) is not JwtSecurityToken jwtToken)
            throw new ArgumentException("Invalid token");

        var expClaim = jwtToken?.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Exp);
        if (expClaim == null)
            throw new ArgumentException("Token doesn't contain an expiration claim.");

        var expirationDate = DateTimeOffset.FromUnixTimeSeconds(long.Parse(expClaim.Value)).UtcDateTime;
        return expirationDate < DateTime.UtcNow;
    }
}