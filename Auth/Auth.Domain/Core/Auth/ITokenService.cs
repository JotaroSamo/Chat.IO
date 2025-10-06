using System.Collections.Generic;
using Auth.Domain.Core.Auth.Models;
using Auth.Domain.Models.Users;
using Microsoft.AspNetCore.Identity;

namespace Auth.Domain.Core.Auth;

public interface ITokenService
{
    Task<AuthTokens> CreateTokensAsync(
        AppUser user,
        IEnumerable<string> roles,
        string? device,
        string? ipAddress,
        CancellationToken cancellationToken = default);
}