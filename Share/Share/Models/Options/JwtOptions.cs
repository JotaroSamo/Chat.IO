namespace Share.Models.Options;

public sealed class JwtOptions
{
    public string Secret { get; set; } = string.Empty;

    public string? Issuer { get; set; }

    public string? Audience { get; set; }

    public int AccessTokenLifetimeSeconds { get; set; } = 60;

    public int RefreshTokenLifetimeDays { get; set; } = 7;
}
