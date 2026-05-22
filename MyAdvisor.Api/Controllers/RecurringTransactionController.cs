using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyAdvisor.Application.DTOs.Common;
using MyAdvisor.Application.DTOs.RecurringTransaction;
using MyAdvisor.Application.Interfaces.Services.Domain;

namespace MyAdvisor.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class RecurringTransactionController : BaseController
    {
        private readonly IRecurringTransactionService _service;

        public RecurringTransactionController(IRecurringTransactionService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var userId = ResolveUserId();
            if (userId is null) return Unauthorized();

            var items = await _service.GetByUserIdAsync(userId.Value);
            return Ok(items);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var userId = ResolveUserId();
            if (userId is null) return Unauthorized();

            try
            {
                var item = await _service.GetByIdAsync(id, userId.Value);
                return Ok(item);
            }
            catch (KeyNotFoundException ex) { return NotFound(new ErrorResponse(ex.Message)); }
            catch (UnauthorizedAccessException) { return Forbid(); }
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateRecurringTransactionRequestDto request)
        {
            var userId = ResolveUserId();
            if (userId is null) return Unauthorized();

            try
            {
                var item = await _service.CreateAsync(request, userId.Value);
                return StatusCode(StatusCodes.Status201Created, item);
            }
            catch (ArgumentException ex) { return BadRequest(new ErrorResponse(ex.Message)); }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, UpdateRecurringTransactionRequestDto request)
        {
            var userId = ResolveUserId();
            if (userId is null) return Unauthorized();

            try
            {
                var item = await _service.UpdateAsync(id, request, userId.Value);
                return Ok(item);
            }
            catch (KeyNotFoundException ex) { return NotFound(new ErrorResponse(ex.Message)); }
            catch (UnauthorizedAccessException) { return Forbid(); }
            catch (ArgumentException ex) { return BadRequest(new ErrorResponse(ex.Message)); }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = ResolveUserId();
            if (userId is null) return Unauthorized();

            try
            {
                await _service.DeleteAsync(id, userId.Value);
                return NoContent();
            }
            catch (KeyNotFoundException ex) { return NotFound(new ErrorResponse(ex.Message)); }
            catch (UnauthorizedAccessException) { return Forbid(); }
        }
    }
}
