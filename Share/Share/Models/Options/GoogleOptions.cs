namespace Share.Models.Options;

public sealed class GoogleOptions
{
    public string ClientId { get; set; } = string.Empty;

    public string ClientSecret { get; set; } = string.Empty;

    public string? CallbackPath { get; set; }
}
