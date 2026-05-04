using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyAdvisor.Application.DTOs.AI;
using MyAdvisor.Application.DTOs.Common;
using MyAdvisor.Application.Interfaces.Services.App;

namespace MyAdvisor.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class ChatController : BaseController
    {
        private readonly IFinancialChatService _chatService;

        public ChatController(IFinancialChatService chatService)
        {
            _chatService = chatService;
        }

        [HttpPost]
        public async Task<IActionResult> Chat([FromBody] ChatRequestDto request)
        {
            var userId = ResolveUserId();
            if (userId is null) return Unauthorized();

            try
            {
                var response = await _chatService.ChatAsync(userId.Value, request);
                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(502, new ErrorResponse($"AI service error: {ex.Message}"));
            }
        }

        [HttpPost("summarize-image")]
        [RequestSizeLimit(20 * 1024 * 1024)]
        public async Task<IActionResult> SummarizeImage(IFormFile image)
        {
            var userId = ResolveUserId();
            if (userId is null) return Unauthorized();

            if (image is null || image.Length == 0)
                return BadRequest(new ErrorResponse("No image provided."));

            try
            {
                using var ms = new MemoryStream();
                await image.CopyToAsync(ms);

                var response = await _chatService.SummarizeImageAsync(ms.ToArray(), image.ContentType);
                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(502, new ErrorResponse($"AI service error: {ex.Message}"));
            }
        }
    }
}
