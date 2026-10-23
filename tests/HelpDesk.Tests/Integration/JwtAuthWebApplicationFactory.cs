using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using HelpDesk.API.Security;

namespace HelpDesk.Tests.Integration;

public sealed class JwtAuthWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        const string signingKey = "test-signing-key-with-at-least-32-characters";

        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Issuer"] = "HelpDesk.Api.Tests",
                ["Jwt:Audience"] = "HelpDesk.Client.Tests",
                ["Jwt:SigningKey"] = signingKey,
                ["Jwt:AccessTokenMinutes"] = "15",
                ["Jwt:RefreshTokenDays"] = "30"
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IOptions<JwtOptions>>();
            services.AddSingleton<IOptions<JwtOptions>>(
                Options.Create(new JwtOptions
                {
                    Issuer = "HelpDesk.Api.Tests",
                    Audience = "HelpDesk.Client.Tests",
                    SigningKey = signingKey,
                    AccessTokenMinutes = 15,
                    RefreshTokenDays = 30
                }));

            services.PostConfigure<JwtBearerOptions>(
                JwtBearerDefaults.AuthenticationScheme,
                options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = "HelpDesk.Api.Tests",
                    ValidateAudience = true,
                    ValidAudience = "HelpDesk.Client.Tests",
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(signingKey)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });
        });
    }
}
