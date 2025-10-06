using System.Security.Claims;
using Auth.Api.Contracts;
using Auth.Domain.Core.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Auth.Api.Infrastructure.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .MapGroup("/auth")
            .WithTags("Auth");

        group.MapPost("/login", async (
            LoginRequest request,
            IAuthService authService,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var device = ResolveDevice(request.Device, httpContext);
            var ipAddress = ResolveIpAddress(httpContext);

            try
            {
                var tokens = await authService.LoginAsync(request.Login, request.Password, device, ipAddress, cancellationToken);
                return Results.Ok(AuthTokensResponse.FromDomain(tokens));
            }
            catch (UnauthorizedAccessException)
            {
                return Results.Unauthorized();
            }
        });

        group.MapPost("/register", async (
            RegisterRequest request,
            IAuthService authService,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var device = ResolveDevice(request.Device, httpContext);
            var ipAddress = ResolveIpAddress(httpContext);
            var userName = string.IsNullOrWhiteSpace(request.UserName) ? request.Email : request.UserName;

            try
            {
                var tokens = await authService.RegisterAsync(request.Email, userName, request.Password, device, ipAddress, cancellationToken);
                return Results.Ok(AuthTokensResponse.FromDomain(tokens));
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        });

        group.MapGet("/google/login", (string? redirectUri) =>
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = string.IsNullOrWhiteSpace(redirectUri)
                    ? "/auth/google/callback"
                    : redirectUri
            };

            return Results.Challenge(properties, new[] { GoogleDefaults.AuthenticationScheme });
        });

        group.MapGet("/google/callback", async (
            HttpContext httpContext,
            IAuthService authService,
            CancellationToken cancellationToken) =>
        {
            var authResult = await httpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);

            if (!authResult.Succeeded || authResult.Principal is null)
            {
                return Results.Unauthorized();
            }

            var principal = authResult.Principal;

            var email = principal.FindFirstValue(ClaimTypes.Email) ?? principal.FindFirstValue("email");
            var fullName = principal.FindFirstValue(ClaimTypes.Name) ?? principal.FindFirstValue("name");

            var device = ResolveDevice(null, httpContext);
            var ipAddress = ResolveIpAddress(httpContext);

            try
            {
                var tokens = await authService.LoginWithGoogleAsync(email ?? string.Empty, fullName, device, ipAddress, cancellationToken);

                await httpContext.SignOutAsync(GoogleDefaults.AuthenticationScheme);

                return Results.Ok(AuthTokensResponse.FromDomain(tokens));
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        });

        return endpoints;
    }

    private static string? ResolveDevice(string? device, HttpContext httpContext)
    {
        if (!string.IsNullOrWhiteSpace(device))
        {
            return device;
        }

        if (httpContext.Request.Headers.TryGetValue("User-Agent", out var userAgent) && !string.IsNullOrWhiteSpace(userAgent))
        {
            return userAgent.ToString();
        }

        return null;
    }

    private static string? ResolveIpAddress(HttpContext httpContext)
    {
        return httpContext.Connection.RemoteIpAddress?.ToString();
    }
}
