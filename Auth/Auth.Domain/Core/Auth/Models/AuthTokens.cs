namespace Auth.Domain.Core.Auth.Models;

public sealed record AuthTokens(
    string AccessToken,
    DateTime AccessTokenExpiresOn,
    string RefreshToken,
    DateTime RefreshTokenExpiresOn
);
