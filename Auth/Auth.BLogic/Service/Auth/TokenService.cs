using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Linq;
using Auth.DataAccess;
using Auth.Domain.Core.Auth;
using Auth.Domain.Core.Auth.Models;
using Auth.Domain.Models.Sessions;
using Auth.Domain.Models.Tokens;
using Auth.Domain.Models.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Share.Models.Options;

namespace Auth.Logic.Service.Auth;

public class TokenService : ITokenService
{
    private readonly AuthDbContext _dbContext;
    private readonly JwtSecurityTokenHandler _tokenHandler = new();
    private readonly JwtOptions _options;

    public TokenService(AuthDbContext dbContext, IOptions<JwtOptions> options)
    {
        _dbContext = dbContext;
        _options = options.Value;
    }

    public async Task<AuthTokens> CreateTokensAsync(
        AppUser user,
        IEnumerable<string> roles,
        string? device,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(user);

        await RemoveExpiredSessionsAsync(user.Id, cancellationToken);

        var now = DateTime.UtcNow;
        var accessTokenExpiresOn = now.AddSeconds(_options.AccessTokenLifetimeSeconds);
        var refreshTokenExpiresOn = now.AddDays(_options.RefreshTokenLifetimeDays);

        var roleList = roles?.Where(r => !string.IsNullOrWhiteSpace(r)).Distinct(StringComparer.OrdinalIgnoreCase).ToList() ?? new List<string>();

        var session = new UserSession
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            User = user,
            Device = device,
            IpAddress = ipAddress,
            CreatedOn = now,
            ModifiedOn = now,
            ExpiresOn = refreshTokenExpiresOn
        };

        var accessToken = GenerateAccessToken(user, roleList, now, accessTokenExpiresOn);
        var refreshTokenValue = GenerateRefreshToken(user, session.Id, roleList, now, refreshTokenExpiresOn);

        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            SessionId = session.Id,
            Session = session,
            Token = refreshTokenValue,
            CreatedOn = now,
            ModifiedOn = now,
            ExpiresOn = refreshTokenExpiresOn
        };

        session.RefreshTokens.Add(refreshToken);

        _dbContext.UserSessions.Add(session);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new AuthTokens(accessToken, accessTokenExpiresOn, refreshTokenValue, refreshTokenExpiresOn);
    }

    private string GenerateAccessToken(AppUser user, IReadOnlyCollection<string> roles, DateTime issuedAt, DateTime expiresAt)
    {
        var claims = CreateCommonClaims(user, roles);
        claims.Add(new Claim("token_type", "access"));

        return CreateJwt(claims, issuedAt, expiresAt);
    }

    private string GenerateRefreshToken(AppUser user, Guid sessionId, IReadOnlyCollection<string> roles, DateTime issuedAt, DateTime expiresAt)
    {
        var claims = CreateCommonClaims(user, roles);
        claims.Add(new Claim("token_type", "refresh"));
        claims.Add(new Claim("sid", sessionId.ToString()));

        return CreateJwt(claims, issuedAt, expiresAt);
    }

    private List<Claim> CreateCommonClaims(AppUser user, IReadOnlyCollection<string> roles)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        if (!string.IsNullOrWhiteSpace(user.UserName))
        {
            claims.Add(new Claim(ClaimTypes.Name, user.UserName));
        }

        if (!string.IsNullOrWhiteSpace(user.Email))
        {
            claims.Add(new Claim(ClaimTypes.Email, user.Email));
        }

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        return claims;
    }

    private string CreateJwt(IEnumerable<Claim> claims, DateTime issuedAt, DateTime expiresAt)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Secret));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: issuedAt,
            expires: expiresAt,
            signingCredentials: credentials);

        return _tokenHandler.WriteToken(token);
    }

    private async Task RemoveExpiredSessionsAsync(string userId, CancellationToken cancellationToken)
    {
        var utcNow = DateTime.UtcNow;

        var expiredSessions = await _dbContext.UserSessions
            .Where(session => session.UserId == userId && session.ExpiresOn != null && session.ExpiresOn < utcNow)
            .ToListAsync(cancellationToken);

        if (expiredSessions.Count == 0)
        {
            return;
        }

        _dbContext.UserSessions.RemoveRange(expiredSessions);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
