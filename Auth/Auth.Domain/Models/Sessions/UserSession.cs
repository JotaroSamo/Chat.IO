using Auth.Domain.Models.Tokens;
using Auth.Domain.Models.Users;

namespace Auth.Domain.Models.Sessions;

public class UserSession
{
    public Guid Id { get; set; }

    public string UserId { get; set; } = null!;

    public AppUser User { get; set; } = null!;

    public string? Device { get; set; }

    public string? IpAddress { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime ModifiedOn { get; set; }

    public DateTime? ExpiresOn { get; set; }

    public bool IsRevoked { get; set; }

    public DateTime? RevokedOn { get; set; }

    public string? RevocationReason { get; set; }

    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
