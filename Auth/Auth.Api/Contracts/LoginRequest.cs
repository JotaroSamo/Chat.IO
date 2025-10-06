namespace Auth.Api.Contracts;

public sealed class LoginRequest
{
    public string Login { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string? Device { get; set; }
}
