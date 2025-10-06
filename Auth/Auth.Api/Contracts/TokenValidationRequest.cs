namespace Auth.Api.Contracts;

public sealed class TokenValidationRequest
{
    public string Token { get; set; } = string.Empty;
}
