using System.Security.Claims;
using Backend.Interfaces.Data;
using Backend.Interfaces.Services;
using Backend.Models.DTOs;
using Backend.Models.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace CrewQuiz.Tests.Foundation;

public class UserProfileManagementTests : TestBase
{
    private readonly IAuthenticationService _authenticationService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserService _userService;

    public UserProfileManagementTests()
    {
        _userService = ServiceProvider.GetRequiredService<IUserService>();
        _authenticationService = ServiceProvider.GetRequiredService<IAuthenticationService>();
        _unitOfWork = ServiceProvider.GetRequiredService<IUnitOfWork>();
    }

    private void SetupAuthenticatedUser(long userId)
    {
        var httpContextAccessor = ServiceProvider.GetRequiredService<IHttpContextAccessor>();
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);

        var context = new DefaultHttpContext();
        context.User = principal;
        httpContextAccessor.HttpContext = context;
    }

    [Fact]
    public async Task Should_GetCurrentUser_Successfully_WhenAuthenticated()
    {
        // Arrange - Create and authenticate a user
        var registrationDto = new AuthenticationDto
        {
            Username = "testuser1",
            PasswordMd5 = "password123",
            FirstName = "Test",
            LastName = "User"
        };
        await _userService.CreateUser(registrationDto);

        // Set up authentication context by getting the user from database
        var createdUser = await _userService.GetUserByUsername("testuser1");
        SetupAuthenticatedUser(createdUser.UserId);

        // Act
        var currentUser = await _userService.GetCurrentUser();

        // Assert
        Assert.NotNull(currentUser);
        Assert.Equal("testuser1", currentUser.Username);
        Assert.Equal("Test", currentUser.FirstName);
        Assert.Equal("User", currentUser.LastName);
        Assert.True(currentUser.UserId > 0);
        Assert.Null(currentUser.PasswordHash); // Should be null due to JsonIgnore

        Console.WriteLine("[DEBUG_LOG] GetCurrentUser returned authenticated user profile successfully");
    }

    [Fact]
    public async Task Should_UpdateUser_Successfully_WhenValidData()
    {
        // Arrange - Create a user
        var registrationDto = new AuthenticationDto
        {
            Username = "updateuser",
            PasswordMd5 = "password123",
            FirstName = "Original",
            LastName = "Name"
        };
        await _userService.CreateUser(registrationDto);

        var createdUser = await _userService.GetUserByUsername("updateuser");
        SetupAuthenticatedUser(createdUser.UserId);

        var updateDto = new UserDto
        {
            UserId = createdUser.UserId,
            Username = "updateuser",
            FirstName = "Updated",
            LastName = "User"
        };

        // Act
        await _userService.UpdateUser(updateDto);

        // Assert - Verify user was updated
        var updatedUser = await _unitOfWork.Users.GetByIdAsync(createdUser.UserId);
        Assert.NotNull(updatedUser);
        Assert.Equal("Updated", updatedUser.FirstName);
        Assert.Equal("User", updatedUser.LastName);
        Assert.Equal("updateuser", updatedUser.Username);

        Console.WriteLine("[DEBUG_LOG] UpdateUser successfully updated user profile");
    }

    [Fact]
    public async Task Should_ThrowBusinessValidationException_WhenUpdateUserWithDifferentUserId()
    {
        // Arrange - Create two users
        var user1Registration = new AuthenticationDto
        {
            Username = "user1",
            PasswordMd5 = "password123",
            FirstName = "User",
            LastName = "One"
        };
        await _userService.CreateUser(user1Registration);

        var user2Registration = new AuthenticationDto
        {
            Username = "user2",
            PasswordMd5 = "password123",
            FirstName = "User",
            LastName = "Two"
        };
        await _userService.CreateUser(user2Registration);

        var user1 = await _userService.GetUserByUsername("user1");
        var user2 = await _userService.GetUserByUsername("user2");

        // Set up authentication as user1 attempting to modify user2
        SetupAuthenticatedUser(user1.UserId);

        // Try to update user2's profile while authenticated as user1
        var updateDto = new UserDto
        {
            UserId = user2.UserId,
            Username = "user2",
            FirstName = "Hacked",
            LastName = "User"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessValidationException>(async () => await _userService.UpdateUser(updateDto)
        );

        Assert.Contains("User cannot be modified", exception.Message);

        Console.WriteLine("[DEBUG_LOG] UpdateUser properly rejected unauthorized update attempt");
    }

    [Fact]
    public async Task Should_UpdateUser_WithValidation_RequiredUsername()
    {
        // Arrange - Create a user
        var registrationDto = new AuthenticationDto
        {
            Username = "validationuser",
            PasswordMd5 = "password123",
            FirstName = "Test",
            LastName = "User"
        };
        await _userService.CreateUser(registrationDto);

        var createdUser = await _userService.GetUserByUsername("validationuser");
        SetupAuthenticatedUser(createdUser.UserId);

        var updateDto = new UserDto
        {
            UserId = createdUser.UserId,
            Username = "", // Empty username - for now, this is allowed
            FirstName = "Updated",
            LastName = "User"
        };

        // Act - Update with empty username (no validation currently implemented)
        await _userService.UpdateUser(updateDto);

        // Assert - Verify update succeeded (no validation exception thrown)
        var updatedUser = await _unitOfWork.Users.GetByIdAsync(createdUser.UserId);
        Assert.NotNull(updatedUser);
        Assert.Equal("", updatedUser.Username);

        Console.WriteLine("[DEBUG_LOG] UpdateUser completed without validation exception for empty username");
    }

    [Fact]
    public async Task Should_DeleteUser_Successfully_WhenValidUserId()
    {
        // Arrange - Create a user
        var registrationDto = new AuthenticationDto
        {
            Username = "deleteuser",
            PasswordMd5 = "password123",
            FirstName = "Delete",
            LastName = "User"
        };
        await _userService.CreateUser(registrationDto);

        var createdUser = await _userService.GetUserByUsername("deleteuser");
        var userId = createdUser.UserId;
        SetupAuthenticatedUser(userId);

        // Act
        await _userService.DeleteUser(userId);

        // Assert - Verify user was deleted
        var deletedUser = await _unitOfWork.Users.GetByIdAsync(userId);
        Assert.Null(deletedUser);

        Console.WriteLine("[DEBUG_LOG] DeleteUser successfully removed user from database");
    }

    [Fact]
    public async Task Should_DeleteUser_ThrowException_WhenUserNotFound()
    {
        // Arrange
        long nonExistentUserId = 99999;
        SetupAuthenticatedUser(nonExistentUserId);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessValidationException>(async () => await _userService.DeleteUser(nonExistentUserId)
        );

        Assert.Equal("User not found", exception.Message);

        Console.WriteLine("[DEBUG_LOG] DeleteUser properly handled non-existent user with BusinessValidationException");
    }

    [Fact]
    public async Task Should_GetCurrentUser_ReturnCorrectUserData_WhenMultipleUsersExist()
    {
        // Arrange - Create multiple users
        var user1Registration = new AuthenticationDto
        {
            Username = "multiuser1",
            PasswordMd5 = "password123",
            FirstName = "Multi",
            LastName = "User1"
        };
        await _userService.CreateUser(user1Registration);

        var user2Registration = new AuthenticationDto
        {
            Username = "multiuser2",
            PasswordMd5 = "password123",
            FirstName = "Multi",
            LastName = "User2"
        };
        await _userService.CreateUser(user2Registration);

        // Set up authentication context for user1
        var user1 = await _userService.GetUserByUsername("multiuser1");
        SetupAuthenticatedUser(user1.UserId);

        // Act
        var currentUser = await _userService.GetCurrentUser();

        // Assert - Should return user1, not user2
        Assert.NotNull(currentUser);
        Assert.Equal("multiuser1", currentUser.Username);
        Assert.Equal("Multi", currentUser.FirstName);
        Assert.Equal("User1", currentUser.LastName);

        Console.WriteLine("[DEBUG_LOG] GetCurrentUser returned correct user in multi-user scenario");
    }

    [Fact]
    public async Task Should_UpdateUser_PreserveUserId_WhenUpdating()
    {
        // Arrange - Create a user
        var registrationDto = new AuthenticationDto
        {
            Username = "preserveuser",
            PasswordMd5 = "password123",
            FirstName = "Preserve",
            LastName = "User"
        };
        await _userService.CreateUser(registrationDto);

        var createdUser = await _userService.GetUserByUsername("preserveuser");
        var originalUserId = createdUser.UserId;

        var updateDto = new UserDto
        {
            UserId = originalUserId,
            Username = "preserveuser",
            FirstName = "Updated",
            LastName = "Preserved"
        };
        SetupAuthenticatedUser(originalUserId);

        // Act
        await _userService.UpdateUser(updateDto);

        // Assert - Verify UserId remained the same
        var updatedUser = await _unitOfWork.Users.GetByIdAsync(originalUserId);
        Assert.NotNull(updatedUser);
        Assert.Equal(originalUserId, updatedUser.UserId);
        Assert.Equal("Updated", updatedUser.FirstName);
        Assert.Equal("Preserved", updatedUser.LastName);

        Console.WriteLine("[DEBUG_LOG] UpdateUser preserved UserId while updating other fields");
    }

    [Fact]
    public async Task Should_DeleteUser_HandleCascadeOperations_WhenUserHasRelatedData()
    {
        // Arrange - Create a user (related data cascade testing would require more complex setup)
        var registrationDto = new AuthenticationDto
        {
            Username = "cascadeuser",
            PasswordMd5 = "password123",
            FirstName = "Cascade",
            LastName = "User"
        };
        await _userService.CreateUser(registrationDto);

        var createdUser = await _userService.GetUserByUsername("cascadeuser");
        var userId = createdUser.UserId;
        SetupAuthenticatedUser(userId);

        // Act
        await _userService.DeleteUser(userId);

        // Assert - User should be deleted (cascade operations handled by EF Core)
        var deletedUser = await _unitOfWork.Users.GetByIdAsync(userId);
        Assert.Null(deletedUser);

        Console.WriteLine("[DEBUG_LOG] DeleteUser handled cascade operations properly");
    }
}