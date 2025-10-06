using System.Security.Claims;

namespace Auth.Domain.Core.Auth.Models;

public sealed record TokenValidationResult(
    bool IsValid,
    ClaimsPrincipal? Principal,
    DateTime? ExpiresOn,
    string? Error
);
