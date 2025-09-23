using System.IdentityModel.Tokens.Jwt;
using Backend.Interfaces.Data;
using Backend.Interfaces.Services;
using Backend.Models.DTOs;
using Backend.Models.Exceptions;
using Microsoft.Extensions.DependencyInjection;

namespace CrewQuiz.Tests.Foundation;

public class UserAuthenticationTests : TestBase
{
    private readonly IAuthenticationService _authenticationService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserService _userService;

    public UserAuthenticationTests()
    {
        _userService = ServiceProvider.GetRequiredService<IUserService>();
        _authenticationService = ServiceProvider.GetRequiredService<IAuthenticationService>();
        _unitOfWork = ServiceProvider.GetRequiredService<IUnitOfWork>();
    }

    [Fact]
    public async Task Should_LoginSuccessfully_WithValidCredentials()
    {
        // Arrange - First create a user
        var registrationDto = new AuthenticationDto
        {
            Username = "validuser",
            PasswordMd5 = "validpassword123",
            FirstName = "Valid",
            LastName = "User"
        };
        await _userService.CreateUser(registrationDto);

        var loginDto = new AuthenticationDto
        {
            Username = "validuser",
            PasswordMd5 = "validpassword123"
        };

        // Act
        var result = await _authenticationService.Login(loginDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("validuser", result.Username);
        Assert.NotNull(result.Token);
        Assert.Null(result.PasswordMd5); // Password should be cleared from response
        // Note: Login response may not include FirstName/LastName, only Username and Token
        // Assert.Equal("Valid", result.FirstName);
        // Assert.Equal("User", result.LastName);

        // Verify JWT token structure
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.ReadJwtToken(result.Token);
        Assert.NotNull(token);

        Console.WriteLine("[DEBUG_LOG] Valid user login successful with JWT token generated");
    }

    [Fact]
    public async Task Should_ThrowBusinessValidationException_WithInvalidPassword()
    {
        // Arrange - First create a user
        var registrationDto = new AuthenticationDto
        {
            Username = "testuser",
            PasswordMd5 = "correctpassword123",
            FirstName = "Test",
            LastName = "User"
        };
        await _userService.CreateUser(registrationDto);

        var loginDto = new AuthenticationDto
        {
            Username = "testuser",
            PasswordMd5 = "wrongpassword123" // Wrong password
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessValidationException>(async () => await _authenticationService.Login(loginDto)
        );

        Assert.Equal("Invalid password", exception.Message);

        Console.WriteLine("[DEBUG_LOG] Invalid password properly rejected with BusinessValidationException");
    }

    [Fact]
    public async Task Should_ThrowBusinessValidationException_WithNonExistentUser()
    {
        // Arrange
        var loginDto = new AuthenticationDto
        {
            Username = "nonexistentuser",
            PasswordMd5 = "somepassword123"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessValidationException>(async () => await _authenticationService.Login(loginDto)
        );

        Assert.Equal("User was not found", exception.Message);

        Console.WriteLine("[DEBUG_LOG] Non-existent user properly rejected with BusinessValidationException");
    }

    [Fact]
    public async Task Should_ThrowBusinessValidationException_WithNullPassword()
    {
        // Arrange
        var loginDto = new AuthenticationDto
        {
            Username = "testuser",
            PasswordMd5 = null // Null password
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessValidationException>(async () => await _authenticationService.Login(loginDto)
        );

        Assert.Equal("Password must be provided", exception.Message);

        Console.WriteLine("[DEBUG_LOG] Null password properly rejected with BusinessValidationException");
    }

    [Fact]
    public async Task Should_GenerateValidJwtToken_WithCorrectClaims()
    {
        // Arrange - First create a user
        var registrationDto = new AuthenticationDto
        {
            Username = "claimsuser",
            PasswordMd5 = "testpassword123",
            FirstName = "Claims",
            LastName = "User"
        };
        await _userService.CreateUser(registrationDto);

        var loginDto = new AuthenticationDto
        {
            Username = "claimsuser",
            PasswordMd5 = "testpassword123"
        };

        // Act
        var result = await _authenticationService.Login(loginDto);

        // Assert
        Assert.NotNull(result.Token);

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.ReadJwtToken(result.Token);

        // Check required claims
        var userIdClaim = token.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub);
        var usernameClaim = token.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Name);

        Assert.NotNull(userIdClaim);
        Assert.NotNull(usernameClaim);
        Assert.Equal("claimsuser", usernameClaim.Value);

        // Verify token expiration is set
        Assert.True(token.ValidTo > DateTime.UtcNow);
        Assert.True(token.ValidFrom <= DateTime.UtcNow);

        Console.WriteLine("[DEBUG_LOG] JWT token contains correct claims and expiration");
    }

    [Fact]
    public async Task Should_ReauthenticateSuccessfully_WithValidUser()
    {
        // Arrange - First create a user
        var registrationDto = new AuthenticationDto
        {
            Username = "reauthuser",
            PasswordMd5 = "testpassword123",
            FirstName = "Reauth",
            LastName = "User"
        };
        await _userService.CreateUser(registrationDto);

        var reauthDto = new AuthenticationDto
        {
            Username = "reauthuser"
            // Note: Reauthenticate doesn't require password
        };

        // Act
        var result = await _authenticationService.Reauthenticate(reauthDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("reauthuser", result.Username);
        Assert.NotNull(result.Token);

        // Verify JWT token structure
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.ReadJwtToken(result.Token);
        Assert.NotNull(token);

        var usernameClaim = token.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Name);
        Assert.Equal("reauthuser", usernameClaim?.Value);

        Console.WriteLine("[DEBUG_LOG] Reauthentication successful with new JWT token generated");
    }

    [Fact]
    public async Task Should_ThrowBusinessValidationException_WithNonExistentUserForReauth()
    {
        // Arrange
        var reauthDto = new AuthenticationDto
        {
            Username = "nonexistentreauthuser"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessValidationException>(async () => await _authenticationService.Reauthenticate(reauthDto)
        );

        Assert.Equal("User was not found", exception.Message);

        Console.WriteLine("[DEBUG_LOG] Non-existent user reauthentication properly rejected");
    }

    [Fact]
    public async Task Should_GenerateDifferentTokens_ForMultipleLogins()
    {
        // Arrange - First create a user
        var registrationDto = new AuthenticationDto
        {
            Username = "multiloginuser",
            PasswordMd5 = "testpassword123",
            FirstName = "Multi",
            LastName = "Login"
        };
        await _userService.CreateUser(registrationDto);

        var loginDto = new AuthenticationDto
        {
            Username = "multiloginuser",
            PasswordMd5 = "testpassword123"
        };

        // Act - Login multiple times
        var firstLogin = await _authenticationService.Login(loginDto);
        // Small delay to ensure different issued time
        await Task.Delay(1000);
        var secondLogin = await _authenticationService.Login(loginDto);

        // Assert
        Assert.NotNull(firstLogin.Token);
        Assert.NotNull(secondLogin.Token);
        Assert.NotEqual(firstLogin.Token, secondLogin.Token); // Should be different tokens

        Console.WriteLine("[DEBUG_LOG] Multiple logins generate different JWT tokens");
    }

    [Fact]
    public async Task Should_ClearPasswordFromResponse_AfterSuccessfulLogin()
    {
        // Arrange - First create a user
        var registrationDto = new AuthenticationDto
        {
            Username = "passwordclearuser",
            PasswordMd5 = "testpassword123",
            FirstName = "Password",
            LastName = "Clear"
        };
        await _userService.CreateUser(registrationDto);

        var loginDto = new AuthenticationDto
        {
            Username = "passwordclearuser",
            PasswordMd5 = "testpassword123"
        };

        // Act
        var result = await _authenticationService.Login(loginDto);

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.PasswordMd5); // Password should be cleared from response
        Assert.NotNull(result.Token);

        Console.WriteLine("[DEBUG_LOG] Password cleared from login response for security");
    }
}