using Microsoft.AspNetCore.Identity;
using MyAdvisor.Application.Interfaces.Contracts;
using MyAdvisor.Application.Interfaces.Services.Auth;
using MyAdvisor.Infrastructure.Identity;

namespace MyAdvisor.Infrastructure.Services.Auth
{
    public class IdentityService : IIdentityService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public IdentityService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task CreateAsync(int domainUserId, string email, string password)
        {
            var identityUser = new ApplicationUser
            {
                UserName = email,
                Email = email,
                DomainUserId = domainUserId
            };

            var result = await _userManager.CreateAsync(identityUser, password);

            if (!result.Succeeded)
                throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        public async Task<ITokenUser> ValidateCredentialsAsync(string email, string password)
        {
            var identityUser = await _userManager.FindByEmailAsync(email)
                ?? throw new UnauthorizedAccessException();

            if (!await _userManager.CheckPasswordAsync(identityUser, password))
                throw new UnauthorizedAccessException();

            return identityUser;
        }
    }
}
