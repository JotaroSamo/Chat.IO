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


}
