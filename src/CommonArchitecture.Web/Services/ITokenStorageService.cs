namespace CommonArchitecture.Web.Services;

public interface ITokenStorageService
{
    Task<string?> GetAccessTokenAsync();
    Task<string?> GetRefreshTokenAsync();
    Task SaveTokensAsync(string accessToken, string refreshToken, DateTime expiresAt);
    Task ClearTokensAsync();
    Task<bool> IsTokenExpiredAsync();
}

