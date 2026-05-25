using MyAdvisor.Application.DTOs.User;

namespace MyAdvisor.Application.Interfaces.Services.Domain
{
    public interface IUserService
    {
        Task<UserDto> CreateAsync(string username, string email);
    }
}
