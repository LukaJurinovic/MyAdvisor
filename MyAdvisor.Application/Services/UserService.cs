using MyAdvisor.Application.DTOs.User;
using MyAdvisor.Application.Interfaces.Repositories;
using MyAdvisor.Application.Interfaces.Services.Domain;
using MyAdvisor.Application.Mappers;
using MyAdvisor.Domain.Entities;

namespace MyAdvisor.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly UserMapper _mapper;

        public UserService(IUserRepository userRepository, UserMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<UserDto> CreateAsync(string username, string email)
        {
            var user = new User(username, email);
            await _userRepository.AddAsync(user);
            return _mapper.ToDto(user);
        }
    }
}
