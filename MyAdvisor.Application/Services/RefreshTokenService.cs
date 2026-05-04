using MyAdvisor.Application.Interfaces.Repositories;
using MyAdvisor.Application.Interfaces.Services.Auth;
using MyAdvisor.Domain.Entities;

namespace MyAdvisor.Application.Services
{
    public class RefreshTokenService : IRefreshTokenService
    {
        private readonly IRefreshTokenRepository _repository;

        public RefreshTokenService(IRefreshTokenRepository repository)
        {
            _repository = repository;
        }

        public Task<RefreshToken?> GetByTokenAsync(string token)
            => _repository.GetByTokenAsync(token);

        public Task AddAsync(RefreshToken token)
            => _repository.AddAsync(token);

        public Task UpdateAsync(RefreshToken token)
            => _repository.UpdateAsync(token);

        public Task DeleteExpiredAndRevokedAsync()
            => _repository.DeleteExpiredAndRevokedAsync();
    }
}
