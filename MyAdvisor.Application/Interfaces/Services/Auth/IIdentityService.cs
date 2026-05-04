using MyAdvisor.Application.Interfaces.Contracts;

namespace MyAdvisor.Application.Interfaces.Services.Auth
{
    public interface IIdentityService
    {
        Task CreateAsync(int domainUserId, string email, string password);
        Task<ITokenUser> ValidateCredentialsAsync(string email, string password);
    }
}
