using Auth.Domain.Core.Auth.Models;

namespace Auth.Domain.Core.Auth;

public interface IAuthService
{
    Task<AuthTokens> LoginAsync(
        string email,
        string password,
        string? device,
        string? ipAddress,
        CancellationToken cancellationToken = default);

    Task<AuthTokens> RegisterAsync(
        string email,
        string userName,
        string password,
        string? device,
        string? ipAddress,
        CancellationToken cancellationToken = default);

    Task<AuthTokens> LoginWithGoogleAsync(
        string email,
        string? fullName,
        string? device,
        string? ipAddress,
        CancellationToken cancellationToken = default);
}