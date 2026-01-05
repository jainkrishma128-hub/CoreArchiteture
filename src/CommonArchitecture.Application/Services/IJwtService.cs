using CommonArchitecture.Core.Entities;
using System.Security.Claims;

namespace CommonArchitecture.Application.Services;

public interface IJwtService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}

