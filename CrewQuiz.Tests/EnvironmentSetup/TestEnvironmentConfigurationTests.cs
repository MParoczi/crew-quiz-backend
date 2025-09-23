using AutoMapper;
using Backend.Data;
using Backend.Interfaces.Data;
using Backend.Models.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using IConfigurationProvider = AutoMapper.IConfigurationProvider;

namespace CrewQuiz.Tests.EnvironmentSetup;

/// <summary>
///     Story ENV-1: Test Environment Configuration
///     As a testing framework, I want to set up the basic test environment
///     So that all subsequent tests can run reliably
///     Acceptance Criteria:
///     - Test database is created and accessible
///     - JWT signing key is configured for testing
///     - AutoMapper profiles are loaded correctly
///     - Logging configuration works for test environment
///     - All dependency injection is properly configured
/// </summary>
public class TestEnvironmentConfigurationTests : TestBase
{
    [Fact]
    public void Should_CreateAndAccessTestDatabase()
    {
        // Arrange & Act
        var dbContext = ServiceProvider.GetRequiredService<CrewQuizContext>();

        // Assert
        Assert.NotNull(dbContext);
        Assert.True(dbContext.Database.IsInMemory());
        Assert.True(dbContext.Database.CanConnect());

        Console.WriteLine("[DEBUG_LOG] Test database created and accessible successfully");
    }

    [Fact]
    public void Should_ConfigureJwtSigningKeyForTesting()
    {
        // Arrange & Act
        var configuration = ServiceProvider.GetRequiredService<IConfiguration>();
        var appSettings = ServiceProvider.GetRequiredService<IOptions<AppSettings>>().Value;

        // Assert
        Assert.NotNull(configuration);
        Assert.NotNull(appSettings);

        var jwtSecret = configuration["AppSettings:Jwt:Secret"];
        var jwtIssuer = configuration["AppSettings:Jwt:Issuer"];
        var jwtAudience = configuration["AppSettings:Jwt:Audience"];

        Assert.NotNull(jwtSecret);
        Assert.NotEmpty(jwtSecret);
        Assert.Equal("TestSigningKey123456789012345678901234567890", jwtSecret);
        Assert.Equal("CrewQuiz.Test", jwtIssuer);
        Assert.Equal("CrewQuiz.Test.Users", jwtAudience);

        // Validate JWT secret key length (should be at least 256 bits / 32 characters for HMAC-SHA256)
        Assert.True(jwtSecret.Length >= 32, "JWT secret key should be at least 32 characters for security");

        Console.WriteLine("[DEBUG_LOG] JWT signing key configured correctly for testing");
    }

    [Fact]
    public void Should_LoadAutoMapperProfilesCorrectly()
    {
        // Arrange & Act
        var mapper = ServiceProvider.GetRequiredService<IMapper>();
        var mapperConfig = ServiceProvider.GetRequiredService<IConfigurationProvider>();

        // Assert
        Assert.NotNull(mapper);
        Assert.NotNull(mapperConfig);

        // Verify configuration is valid (this will throw if there are mapping issues)
        try
        {
            mapperConfig.AssertConfigurationIsValid();
            // If we reach here, no exception was thrown
            Assert.True(true, "AutoMapper configuration is valid");
        }
        catch (Exception ex)
        {
            Assert.True(false, $"AutoMapper configuration is invalid: {ex.Message}");
        }

        // Verify mapper can be used
        Assert.NotNull(mapper);

        Console.WriteLine("[DEBUG_LOG] AutoMapper profiles loaded and validated successfully");
    }

    [Fact]
    public void Should_ConfigureLoggingForTestEnvironment()
    {
        // Arrange & Act
        var loggerFactory = ServiceProvider.GetRequiredService<ILoggerFactory>();
        var logger = ServiceProvider.GetRequiredService<ILogger<TestEnvironmentConfigurationTests>>();
        var configuration = ServiceProvider.GetRequiredService<IConfiguration>();

        // Assert
        Assert.NotNull(loggerFactory);
        Assert.NotNull(logger);
        Assert.NotNull(configuration);

        // Test logging configuration
        var environment = configuration["Environment"];
        Assert.Equal("Test", environment);

        // Verify logger can write messages
        var logException1 = Record.Exception(() => logger.LogInformation("Test logging configuration"));
        var logException2 = Record.Exception(() => logger.LogDebug("Debug level logging test"));
        var logException3 = Record.Exception(() => logger.LogWarning("Warning level logging test"));

        Assert.Null(logException1);
        Assert.Null(logException2);
        Assert.Null(logException3);

        Console.WriteLine("[DEBUG_LOG] Logging configuration working correctly for test environment");
    }

    [Fact]
    public void Should_ConfigureDependencyInjectionProperly()
    {
        // Arrange & Act - Test core services
        var dbContext = ServiceProvider.GetRequiredService<CrewQuizContext>();
        var unitOfWork = ServiceProvider.GetRequiredService<IUnitOfWork>();
        var configuration = ServiceProvider.GetRequiredService<IConfiguration>();
        var mapper = ServiceProvider.GetRequiredService<IMapper>();
        var loggerFactory = ServiceProvider.GetRequiredService<ILoggerFactory>();

        // Assert - Core services
        Assert.NotNull(dbContext);
        Assert.NotNull(unitOfWork);
        Assert.NotNull(configuration);
        Assert.NotNull(mapper);
        Assert.NotNull(loggerFactory);

        // Test service resolution - verify services can be resolved
        var serviceScope = ServiceProvider.CreateScope();
        using (serviceScope)
        {
            var scopedDbContext = serviceScope.ServiceProvider.GetRequiredService<CrewQuizContext>();
            Assert.NotNull(scopedDbContext);
        }

        Console.WriteLine("[DEBUG_LOG] Dependency injection configured properly - all core services resolved");
    }

    [Fact]
    public void Should_ValidateConfigurationIntegrity()
    {
        // Arrange & Act
        var configuration = ServiceProvider.GetRequiredService<IConfiguration>();

        // Assert - Database Configuration
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        Assert.NotNull(connectionString);
        Assert.Equal("Data Source=InMemory", connectionString);

        // Assert - AppSettings Configuration
        var jwtSection = configuration.GetSection("AppSettings:Jwt");
        Assert.True(jwtSection.Exists());

        var corsSection = configuration.GetSection("AppSettings:Cors");
        Assert.True(corsSection.Exists());

        // Assert - Serilog Configuration
        var serilogSection = configuration.GetSection("Serilog");
        Assert.True(serilogSection.Exists());

        // Assert - Environment Configuration
        var environment = configuration["Environment"];
        Assert.Equal("Test", environment);

        Console.WriteLine("[DEBUG_LOG] Configuration integrity validated - all sections present and accessible");
    }
}