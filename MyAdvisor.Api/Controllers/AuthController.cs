using Microsoft.AspNetCore.Mvc;
using MyAdvisor.Application.DTOs.Auth;
using MyAdvisor.Application.DTOs.Common;
using MyAdvisor.Application.Interfaces.Services.Auth;

namespace MyAdvisor.Api.Controllers
{
    [Route("api/[controller]")]
    public class AuthController : BaseController
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequestDto request)
        {
            try
            {
                var result = await _authService.RegisterAsync(request);
                return StatusCode(StatusCodes.Status201Created, result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ErrorResponse(ex.Message));
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequestDto request)
        {
            try
            {
                var result = await _authService.LoginAsync(request);
                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new ErrorResponse("Invalid email or password."));
            }
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh(RefreshRequestDto request)
        {
            try
            {
                var result = await _authService.RefreshAsync(request);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return Unauthorized(new ErrorResponse(ex.Message));
            }
        }

        [HttpPost("revoke")]
        public async Task<IActionResult> Revoke(RevokeRequestDto request)
        {
            var userId = ResolveUserId();
            if (userId is null) return Unauthorized();
            try
            {
                await _authService.RevokeAsync(request, userId.Value);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ErrorResponse(ex.Message));
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }
    }
}
