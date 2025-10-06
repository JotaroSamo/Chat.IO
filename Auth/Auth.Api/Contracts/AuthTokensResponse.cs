using Auth.Domain.Core.Auth.Models;

namespace Auth.Api.Contracts;

public sealed class AuthTokensResponse
{
    public string AccessToken { get; init; } = string.Empty;

    public DateTime AccessTokenExpiresOn { get; init; }

    public string RefreshToken { get; init; } = string.Empty;

    public DateTime RefreshTokenExpiresOn { get; init; }

    public static AuthTokensResponse FromDomain(AuthTokens tokens) => new()
    {
        AccessToken = tokens.AccessToken,
        AccessTokenExpiresOn = tokens.AccessTokenExpiresOn,
        RefreshToken = tokens.RefreshToken,
        RefreshTokenExpiresOn = tokens.RefreshTokenExpiresOn
    };
}
