namespace Auth.Api.Contracts;

public sealed class TokenValidationResponse
{
    public bool IsValid { get; init; }

    public string? Subject { get; init; }

    public DateTime? ExpiresOn { get; init; }

    public Dictionary<string, string>? Claims { get; init; }

    public string? Error { get; init; }
}
