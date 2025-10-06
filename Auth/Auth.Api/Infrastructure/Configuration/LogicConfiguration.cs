using Auth.Domain.Core.Auth;
using Auth.Logic.Service.Auth;
using Microsoft.Extensions.DependencyInjection;

namespace Auth.Api.Infrastructure.Configuration;

public static class LogicConfiguration
{
    public static IServiceCollection AddAuthLogic(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ITokenService, TokenService>();

        return services;
    }
}