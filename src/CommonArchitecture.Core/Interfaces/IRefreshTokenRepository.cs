using CommonArchitecture.Core.Entities;

namespace CommonArchitecture.Core.Interfaces;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenAsync(string token);
    Task<RefreshToken> CreateAsync(RefreshToken refreshToken);
    Task<bool> RevokeTokenAsync(string token);
    Task<bool> RevokeAllUserTokensAsync(int userId);
    Task<bool> DeleteExpiredTokensAsync();
    Task<bool> IsTokenReusedAsync(string token, int userId);
    Task<int> GetActiveTokenCountAsync(int userId);
}

