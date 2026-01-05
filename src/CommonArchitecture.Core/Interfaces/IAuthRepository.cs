using CommonArchitecture.Core.Entities;

namespace CommonArchitecture.Core.Interfaces;

public interface IAuthRepository
{
    Task<User?> GetUserByMobileAsync(string mobile);
    Task<User?> ValidateUserAsync(string mobile, int roleId);
}
