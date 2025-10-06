using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Share.Models.Options;

namespace Auth.Api.Infrastructure.Configuration;

public static class AuthConfiguration
{
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var section = configuration.GetSection("Jwt");
        services.Configure<JwtOptions>(section);

        var jwtOptions = section.Get<JwtOptions>() ?? throw new InvalidOperationException("Jwt settings are not configured.");

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret));

        var googleSection = configuration.GetSection("Authentication:Google");
        services.Configure<GoogleOptions>(googleSection);

        var googleOptions = googleSection.Get<GoogleOptions>();

        if (googleOptions is null ||
            string.IsNullOrWhiteSpace(googleOptions.ClientId) ||
            string.IsNullOrWhiteSpace(googleOptions.ClientSecret))
        {
            throw new InvalidOperationException("Google authentication settings are not configured.");
        }

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = !string.IsNullOrWhiteSpace(jwtOptions.Issuer),
                    ValidIssuer = jwtOptions.Issuer,
                    ValidateAudience = !string.IsNullOrWhiteSpace(jwtOptions.Audience),
                    ValidAudience = jwtOptions.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = signingKey,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            })
            .AddCookie()
            .AddGoogle(options =>
            {
                options.ClientId = googleOptions.ClientId;
                options.ClientSecret = googleOptions.ClientSecret;
                options.SignInScheme = IdentityConstants.ExternalScheme;

                if (!string.IsNullOrWhiteSpace(googleOptions.CallbackPath))
                {
                    options.CallbackPath = googleOptions.CallbackPath;
                }
            });

        services.AddAuthorization();

        return services;
    }
}