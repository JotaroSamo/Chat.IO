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
    
}