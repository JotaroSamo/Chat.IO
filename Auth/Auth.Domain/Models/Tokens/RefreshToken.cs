using Auth.Domain.Models.Sessions;

namespace Auth.Domain.Models.Tokens;

public class RefreshToken
{
    public Guid Id { get; set; }

    public Guid SessionId { get; set; }

    public UserSession Session { get; set; } = null!;

    public string Token { get; set; } = string.Empty;

    public DateTime ExpiresOn { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime ModifiedOn { get; set; }

    public DateTime? ConsumedOn { get; set; }

    public bool IsRevoked { get; set; }

    public DateTime? RevokedOn { get; set; }

    public string? RevocationReason { get; set; }
}
