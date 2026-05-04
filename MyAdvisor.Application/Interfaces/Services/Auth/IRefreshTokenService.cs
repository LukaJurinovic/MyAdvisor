using MyAdvisor.Domain.Entities;

namespace MyAdvisor.Application.Interfaces.Services.Auth
{
    public interface IRefreshTokenService
    {
        Task<RefreshToken?> GetByTokenAsync(string token);
        Task AddAsync(RefreshToken token);
        Task UpdateAsync(RefreshToken token);
        Task DeleteExpiredAndRevokedAsync();
    }
}
