using System.Linq;
using System.Text;
using Auth.Domain.Core.Auth;
using Auth.Domain.Core.Auth.Models;
using Auth.Domain.Models.Users;
using Microsoft.AspNetCore.Identity;

namespace Auth.Logic.Service.Auth;

public class AuthService : IAuthService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly ITokenService _tokenService;
    public AuthService(
        UserManager<AppUser> userManager,
        ITokenService tokenService)
    {
        _userManager = userManager;
        _tokenService = tokenService;
    }

    public async Task<AuthTokens> LoginAsync(
        string login,
        string password,
        string? device,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
        {
            throw new UnauthorizedAccessException("Invalid credentials provided.");
        }

        var user = await _userManager.FindByNameAsync(login);

        if (user is null)
        {
            throw new UnauthorizedAccessException("Invalid credentials provided.");
        }

        var signInResult = await _userManager.CheckPasswordAsync(user, password);

        if (!signInResult)
        {
            throw new UnauthorizedAccessException("Invalid credentials provided.");
        }

        var roles = await _userManager.GetRolesAsync(user);

        return await _tokenService.CreateTokensAsync(user, roles, device, ipAddress, cancellationToken);
    }

    public async Task<AuthTokens> RegisterAsync(
        string email,
        string userName,
        string password,
        string? device,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email is required.", nameof(email));
        }

        if (string.IsNullOrWhiteSpace(userName))
        {
            throw new ArgumentException("User name is required.", nameof(userName));
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException("Password is required.", nameof(password));
        }

        var existingByEmail = await _userManager.FindByEmailAsync(email);

        if (existingByEmail is not null)
        {
            throw new InvalidOperationException("Email is already registered.");
        }

        var existingByName = await _userManager.FindByNameAsync(userName);

        if (existingByName is not null)
        {
            throw new InvalidOperationException("User name is already taken.");
        }

        var now = DateTime.UtcNow;

        var user = new AppUser
        {
            Email = email,
            UserName = userName,
            CreatedOn = now,
            ModifiedOn = now
        };

        var result = await _userManager.CreateAsync(user, password);

        if (!result.Succeeded)
        {
            var errors = string.Join(" ", result.Errors.Select(error => error.Description));
            throw new InvalidOperationException(string.IsNullOrWhiteSpace(errors) ? "Failed to register user." : errors);
        }

        var roles = await _userManager.GetRolesAsync(user);

        return await _tokenService.CreateTokensAsync(user, roles, device, ipAddress, cancellationToken);
    }

    public async Task<AuthTokens> LoginWithGoogleAsync(
        string email,
        string? fullName,
        string? device,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new InvalidOperationException("Google login failed to provide an email address.");
        }

        var existingUser = await _userManager.FindByEmailAsync(email);

        if (existingUser is null)
        {
            var userName = BuildUserName(email, fullName);
            var now = DateTime.UtcNow;

            existingUser = new AppUser
            {
                Email = email,
                UserName = userName,
                EmailConfirmed = true,
                CreatedOn = now,
                ModifiedOn = now
            };

            var createResult = await _userManager.CreateAsync(existingUser);

            if (!createResult.Succeeded)
            {
                var errors = string.Join(" ", createResult.Errors.Select(error => error.Description));
                throw new InvalidOperationException(string.IsNullOrWhiteSpace(errors) ? "Failed to create user from Google login." : errors);
            }
        }
        else
        {
            var updated = false;

            if (!existingUser.EmailConfirmed)
            {
                existingUser.EmailConfirmed = true;
                updated = true;
            }

            if (!string.IsNullOrWhiteSpace(fullName) && string.IsNullOrWhiteSpace(existingUser.UserName))
            {
                existingUser.UserName = BuildUserName(email, fullName);
                updated = true;
            }

            if (updated)
            {
                existingUser.ModifiedOn = DateTime.UtcNow;
                await _userManager.UpdateAsync(existingUser);
            }
        }

        var roles = await _userManager.GetRolesAsync(existingUser);

        return await _tokenService.CreateTokensAsync(existingUser, roles, device, ipAddress, cancellationToken);
    }

    private static string BuildUserName(string email, string? fullName)
    {
        if (!string.IsNullOrWhiteSpace(fullName))
        {
            var sanitized = new string(fullName
                .Where(ch => char.IsLetterOrDigit(ch) || ch == '-' || ch == '_')
                .ToArray());

            if (!string.IsNullOrWhiteSpace(sanitized))
            {
                return sanitized.Length > 32 ? sanitized[..32] : sanitized;
            }
        }

        var atIndex = email.IndexOf('@');
        var baseName = atIndex > 0 ? email[..atIndex] : email;
        baseName = new string(baseName
            .Where(ch => char.IsLetterOrDigit(ch) || ch == '-' || ch == '_')
            .ToArray());

        if (string.IsNullOrWhiteSpace(baseName))
        {
            baseName = Convert.ToBase64String(Encoding.UTF8.GetBytes(email)).TrimEnd('=');
        }

        return baseName.Length > 32 ? baseName[..32] : baseName;
    }
}
