using Backend.Data;
using Backend.Extensions;
using Backend.Models.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace CrewQuiz.Tests;

public class TestBase : IDisposable
{
    public TestBase()
    {
        SetupTestEnvironment();
    }

    protected IServiceProvider ServiceProvider { get; private set; }
    protected IConfiguration Configuration { get; private set; }
    protected CrewQuizContext DbContext { get; private set; }

    public virtual void Dispose()
    {
        DbContext?.Dispose();
        ServiceProvider?.GetService<IServiceScope>()?.Dispose();
        Log.CloseAndFlush();
    }

    private void SetupTestEnvironment()
    {
        // Configure test configuration
        var configurationManager = new ConfigurationManager();
        configurationManager.SetBasePath(Directory.GetCurrentDirectory());
        configurationManager.AddJsonFile("appsettings.Test.json", false, true);
        configurationManager.AddEnvironmentVariables();

        Configuration = configurationManager;

        // Setup Serilog for testing
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(Configuration)
            .WriteTo.Console()
            .CreateLogger();

        // Setup service collection
        var services = new ServiceCollection();

        // Add configuration
        services.AddSingleton(Configuration);
        services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));

        // Add logging
        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddSerilog(Log.Logger);
        });

        // Add AutoMapper
        services.AddAutoMapper(typeof(CrewQuizContext).Assembly);

        // Add in-memory database for testing
        services.AddDbContext<CrewQuizContext>(options =>
        {
            options.UseInMemoryDatabase(Guid.NewGuid().ToString());
            // Enable tracking for proper update/delete operations
            // options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        });

        // Add JWT authentication (test configuration)
        services.AddJwtAuthentication(configurationManager);

        // Add repositories and services
        services.AddRepositories();
        services.AddServices();

        // Add HttpContextAccessor for testing
        services.AddHttpContextAccessor();

        ServiceProvider = services.BuildServiceProvider();
        DbContext = ServiceProvider.GetRequiredService<CrewQuizContext>();

        // Ensure database is created
        DbContext.Database.EnsureCreated();
    }
}