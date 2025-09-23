using System.Reflection;
using System.Text;
using Backend.Data;
using Backend.Interfaces.Data;
using Backend.Interfaces.Services;
using Backend.Interfaces.ServiceUtils;
using Backend.Interfaces.Utils;
using Backend.Services;
using Backend.ServiceUtils;
using Backend.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace Backend.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddNpgsqlDbContext(this IServiceCollection services, string? connectionString)
    {
        services.AddScoped<AuditInterceptor>();
        services.AddDbContext<CrewQuizContext>((sp, o) =>
        {
            var auditInterceptor = sp.GetService<AuditInterceptor>()!;
            o.UseNpgsql(connectionString)
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                .AddInterceptors(auditInterceptor);
        });

        return services;
    }

    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, ConfigurationManager configuration)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(o =>
            {
                o.RequireHttpsMetadata = false;
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["AppSettings:Jwt:Secret"]!)),
                    ValidIssuer = configuration["AppSettings:Jwt:Issuer"],
                    ValidAudience = configuration["AppSettings:Jwt:Audience"],
                    ClockSkew = TimeSpan.Zero,
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true
                };
            });

        return services;
    }

    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        var repositoryTypes = Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract)
            .Where(t => t.Name.EndsWith("Repository"))
            .ToList();

        var registeredCount = 0;
        foreach (var repositoryType in repositoryTypes)
        {
            var interfaceType = repositoryType.GetInterfaces().FirstOrDefault(i => i.Name == "I" + repositoryType.Name);
            if (interfaceType != null)
            {
                services.AddTransient(interfaceType, repositoryType);
                registeredCount++;
            }
        }

        services.AddTransient<IUnitOfWork, UnitOfWork>();

        return services;
    }

    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        var serviceTypes = Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract)
            .Where(t => typeof(ServiceBase).IsAssignableFrom(t) || typeof(ServiceUtilBase).IsAssignableFrom(t))
            .Where(t => t.GetInterfaces().Any(i => i.IsAssignableTo(typeof(IServiceBase))) ||
                        t.GetInterfaces().Any(i => i.IsAssignableTo(typeof(IServiceUtilBase))))
            .ToList();

        var registeredCount = 0;
        foreach (var serviceType in serviceTypes)
        {
            var interfaces = serviceType.GetInterfaces().Where(i => i.IsAssignableTo(typeof(IServiceBase)) || i.IsAssignableTo(typeof(IServiceUtilBase)));
            foreach (var serviceInterface in interfaces)
            {
                services.AddTransient(serviceInterface, serviceType);
                registeredCount++;
            }
        }

        services.AddScoped<IServiceDispatcher, ServiceDispatcher>();
        services.AddScoped<ISessionCleanupService, SessionCleanupService>();
        services.AddHostedService<SessionCleanupBackgroundService>();

        return services;
    }

    public static IServiceCollection AddSwaggerGenWithAuth(this IServiceCollection services)
    {
        services.AddSwaggerGen(o =>
        {
            o.CustomSchemaIds(id => id.FullName!.Replace('+', '-'));

            var securityScheme = new OpenApiSecurityScheme
            {
                Name = "JWT Authentication",
                Description = "Enter JWT Bearer token",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = JwtBearerDefaults.AuthenticationScheme,
                BearerFormat = "JWT"
            };
            o.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, securityScheme);

            var securityRequirements = new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = JwtBearerDefaults.AuthenticationScheme
                        }
                    },
                    []
                }
            };
            o.AddSecurityRequirement(securityRequirements);
        });

        return services;
    }

    public static IServiceCollection ConfigureCors(this IServiceCollection services, ConfigurationManager configuration, string corsPolicyName)
    {
        var allowedOrigins = configuration["AppSettings:Cors:AllowedOrigins"]?.Split(";") ?? [];

        services.AddCors(options =>
        {
            options.AddPolicy(corsPolicyName,
                builder => builder
                    .WithOrigins(allowedOrigins)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials());
        });

        return services;
    }
}