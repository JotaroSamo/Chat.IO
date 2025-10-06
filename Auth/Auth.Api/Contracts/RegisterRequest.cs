namespace Auth.Api.Contracts;

public sealed class RegisterRequest
{
    public string Email { get; set; } = string.Empty;

    public string? UserName { get; set; }

    public string Password { get; set; } = string.Empty;

    public string? Device { get; set; }
}
