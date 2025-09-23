using Backend.Interfaces.Data;
using Backend.Interfaces.Services;
using Backend.Models.DTOs;
using Backend.Models.Exceptions;
using Microsoft.Extensions.DependencyInjection;

namespace CrewQuiz.Tests.Foundation;

public class UserRegistrationTests : TestBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserService _userService;

    public UserRegistrationTests()
    {
        _userService = ServiceProvider.GetRequiredService<IUserService>();
        _unitOfWork = ServiceProvider.GetRequiredService<IUnitOfWork>();
    }

    [Fact]
    public async Task Should_CreateUserSuccessfully_WithValidData()
    {
        // Arrange
        var authDto = new AuthenticationDto
        {
            Username = "testuser",
            PasswordMd5 = "testpassword123",
            FirstName = "Test",
            LastName = "User"
        };

        // Act
        await _userService.CreateUser(authDto);

        // Assert
        var createdUser = await _unitOfWork.Users.FirstOrDefaultAsync(u => u.Username == "testuser");
        Assert.NotNull(createdUser);
        Assert.Equal("testuser", createdUser.Username);
        Assert.Equal("Test", createdUser.FirstName);
        Assert.Equal("User", createdUser.LastName);
        Assert.NotNull(createdUser.PasswordHash);
        Assert.NotEqual("testpassword123", createdUser.PasswordHash); // Password should be hashed

        Console.WriteLine("[DEBUG_LOG] User created successfully with hashed password");
    }

    [Fact]
    public async Task Should_ThrowBusinessValidationException_WithDuplicateUsername()
    {
        // Arrange
        var firstUser = new AuthenticationDto
        {
            Username = "duplicateuser",
            PasswordMd5 = "password123",
            FirstName = "First",
            LastName = "User"
        };

        var duplicateUser = new AuthenticationDto
        {
            Username = "duplicateuser", // Same username
            PasswordMd5 = "differentpassword",
            FirstName = "Second",
            LastName = "User"
        };

        // Act
        await _userService.CreateUser(firstUser);

        // Assert
        var exception = await Assert.ThrowsAsync<BusinessValidationException>(async () => await _userService.CreateUser(duplicateUser)
        );

        Assert.Equal("Username already exists", exception.Message);

        Console.WriteLine("[DEBUG_LOG] Duplicate username properly rejected with BusinessValidationException");
    }

    [Fact]
    public async Task Should_ThrowBusinessValidationException_WithNullPassword()
    {
        // Arrange
        var authDto = new AuthenticationDto
        {
            Username = "testuser",
            PasswordMd5 = null, // Null password
            FirstName = "Test",
            LastName = "User"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessValidationException>(async () => await _userService.CreateUser(authDto)
        );

        Assert.Equal("Password must be provided", exception.Message);

        Console.WriteLine("[DEBUG_LOG] Null password properly rejected with BusinessValidationException");
    }

    [Fact]
    public async Task Should_ThrowBusinessValidationException_WithEmptyPassword()
    {
        // Arrange
        var authDto = new AuthenticationDto
        {
            Username = "testuser",
            PasswordMd5 = string.Empty, // Empty password
            FirstName = "Test",
            LastName = "User"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessValidationException>(async () => await _userService.CreateUser(authDto)
        );

        Assert.Equal("Password must be provided", exception.Message);

        Console.WriteLine("[DEBUG_LOG] Empty password properly rejected with BusinessValidationException");
    }

    [Fact]
    public async Task Should_CreateUser_WithMinimalRequiredData()
    {
        // Arrange - Test with minimal required data (User domain model requires FirstName and LastName)
        var authDto = new AuthenticationDto
        {
            Username = "minimaluser",
            PasswordMd5 = "password123",
            FirstName = "Minimal", // Required by User domain model
            LastName = "User" // Required by User domain model
        };

        // Act
        await _userService.CreateUser(authDto);

        // Assert
        var createdUser = await _unitOfWork.Users.FirstOrDefaultAsync(u => u.Username == "minimaluser");
        Assert.NotNull(createdUser);
        Assert.Equal("minimaluser", createdUser.Username);
        Assert.Equal("Minimal", createdUser.FirstName);
        Assert.Equal("User", createdUser.LastName);
        Assert.NotNull(createdUser.PasswordHash);

        Console.WriteLine("[DEBUG_LOG] User created successfully with all required fields");
    }

    [Fact]
    public async Task Should_HashPasswordSecurely_NotStorePlainText()
    {
        // Arrange
        var plainPassword = "mysecretpassword123";
        var authDto = new AuthenticationDto
        {
            Username = "secureuser",
            PasswordMd5 = plainPassword,
            FirstName = "Secure",
            LastName = "User"
        };

        // Act
        await _userService.CreateUser(authDto);

        // Assert
        var createdUser = await _unitOfWork.Users.FirstOrDefaultAsync(u => u.Username == "secureuser");
        Assert.NotNull(createdUser);
        Assert.NotEqual(plainPassword, createdUser.PasswordHash);
        Assert.True(createdUser.PasswordHash.Length > plainPassword.Length); // Hashed password should be longer

        Console.WriteLine("[DEBUG_LOG] Password properly hashed and not stored in plain text");
    }

    [Fact]
    public async Task Should_AllowDifferentCaseUsernames()
    {
        // Arrange
        var lowerCaseUser = new AuthenticationDto
        {
            Username = "testuser",
            PasswordMd5 = "password123",
            FirstName = "Lower",
            LastName = "Case"
        };

        var upperCaseUser = new AuthenticationDto
        {
            Username = "TESTUSER", // Different case
            PasswordMd5 = "password456",
            FirstName = "Upper",
            LastName = "Case"
        };

        // Act
        await _userService.CreateUser(lowerCaseUser);
        await _userService.CreateUser(upperCaseUser); // Should not throw exception

        // Assert
        var users = await _unitOfWork.Users.GetAllAsync();
        var testUsers = users.Where(u => u.Username.ToLower() == "testuser").ToList();
        Assert.Equal(2, testUsers.Count); // Both users should be created

        Console.WriteLine("[DEBUG_LOG] Different case usernames allowed - case sensitivity maintained");
    }

    [Fact]
    public async Task Should_CreateMultipleUsers_WithDifferentUsernames()
    {
        // Arrange
        var user1 = new AuthenticationDto
        {
            Username = "user1",
            PasswordMd5 = "password1",
            FirstName = "First",
            LastName = "User"
        };

        var user2 = new AuthenticationDto
        {
            Username = "user2",
            PasswordMd5 = "password2",
            FirstName = "Second",
            LastName = "User"
        };

        var user3 = new AuthenticationDto
        {
            Username = "user3",
            PasswordMd5 = "password3",
            FirstName = "Third",
            LastName = "User"
        };

        // Act
        await _userService.CreateUser(user1);
        await _userService.CreateUser(user2);
        await _userService.CreateUser(user3);

        // Assert
        var allUsers = await _unitOfWork.Users.GetAllAsync();
        Assert.Equal(3, allUsers.Count());
        Assert.True(allUsers.Any(u => u.Username == "user1"));
        Assert.True(allUsers.Any(u => u.Username == "user2"));
        Assert.True(allUsers.Any(u => u.Username == "user3"));

        Console.WriteLine("[DEBUG_LOG] Multiple users created successfully with different usernames");
    }
}