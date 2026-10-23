using System.Text;
using HelpDesk.API.Security;
using HelpDesk.Application.Auth;
using HelpDesk.Application.Categories;
using HelpDesk.Application.Common.Interfaces;
using HelpDesk.Application.Tickets;
using HelpDesk.Application.Users;
using HelpDesk.DataAccess;
using HelpDesk.DataAccess.Repositories;
using HelpDesk.Domain.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

using ApplicationPasswordHasher = HelpDesk.Application.Common.Interfaces.IPasswordHasher;
using AspNetPasswordHasher = Microsoft.AspNetCore.Identity.IPasswordHasher<HelpDesk.Domain.Entities.User>;

namespace HelpDesk.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString(nameof(AppDbContext)));
        });

        services.AddScoped<ITicketService, TicketService>();
        services.AddScoped<ITicketRepository, TicketRepository>();
        services.AddScoped<ITicketProcessingService, TicketProcessingService>();

        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();

        services.AddScoped<IUserManagementService, UserManagementService>();
        services.AddScoped<IUserRepository, UserRepository>();

        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IOneTimeTokenRepository, OneTimeTokenRepository>();
        services.AddScoped<AspNetPasswordHasher, PasswordHasher<User>>();
        services.AddScoped<ApplicationPasswordHasher, PasswordHasher>();
        services.AddSingleton(
            configuration.GetSection(EmailOptions.SectionName).Get<EmailOptions>()
            ?? new EmailOptions());
        services.AddScoped<IEmailSender, SmtpEmailSender>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();

        return services;
    }

    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<JwtOptions>(
            configuration.GetSection(JwtOptions.SectionName));

        JwtOptions jwtOptions = configuration
            .GetSection(JwtOptions.SectionName)
            .Get<JwtOptions>()
            ?? throw new InvalidOperationException("JWT configuration is missing.");

        if (string.IsNullOrWhiteSpace(jwtOptions.SigningKey) ||
            jwtOptions.SigningKey.Length < 32)
        {
            throw new InvalidOperationException(
                "Jwt:SigningKey must contain at least 32 characters.");
        }

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtOptions.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });

        return services;
    }

    public static IServiceCollection AddAuthorizationPolicies(
        this IServiceCollection services)
    {
        services.AddAuthorizationBuilder()
            .AddPolicy(AuthorizationPolicies.AdminOnly, policy =>
                policy.RequireRole("Admin"))
            .AddPolicy(AuthorizationPolicies.SupportOrAdmin, policy =>
                policy.RequireRole("Support", "Admin"));

        return services;
    }
}
