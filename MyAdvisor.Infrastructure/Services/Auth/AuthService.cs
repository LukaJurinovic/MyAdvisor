using MyAdvisor.Application.DTOs.Auth;
using MyAdvisor.Application.Interfaces.Contracts;
using MyAdvisor.Application.Interfaces.Services.Domain;
using MyAdvisor.Application.Interfaces.Services.Auth;

namespace MyAdvisor.Infrastructure.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly ITokenService _tokenService;
        private readonly IUserService _userService;
        private readonly IIdentityService _identityService;
        private readonly IUnitOfWork _unitOfWork;

        public AuthService(
            ITokenService tokenService,
            IUserService userService,
            IIdentityService identityService,
            IUnitOfWork unitOfWork)
        {
            _tokenService = tokenService;
            _userService = userService;
            _identityService = identityService;
            _unitOfWork = unitOfWork;
        }

        public async Task<RegisterResponseDto> RegisterAsync(RegisterRequestDto request)
        {
            RegisterResponseDto? response = null;

            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var userDto = await _userService.CreateAsync($"{request.FirstName} {request.LastName}", request.Email);
                await _identityService.CreateAsync(userDto.Id, request.Email, request.Password);
                response = new RegisterResponseDto("User created");
            });

            return response!;
        }

        public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
        {
            var tokenUser = await _identityService.ValidateCredentialsAsync(request.Email, request.Password);
            var accessToken = _tokenService.GenerateAccessToken(tokenUser);
            var refreshToken = await _tokenService.GenerateRefreshTokenAsync(tokenUser);
            return new AuthResponseDto(accessToken, refreshToken);
        }

        public async Task<AuthResponseDto> RefreshAsync(RefreshRequestDto request)
        {
            var result = await _tokenService.RefreshAsync(request.RefreshToken);
            return new AuthResponseDto(result.accessToken, result.refreshToken);
        }

        public Task RevokeAsync(RevokeRequestDto request, int userId)
            => _tokenService.RevokeAsync(request.RefreshToken, userId);
    }
}
