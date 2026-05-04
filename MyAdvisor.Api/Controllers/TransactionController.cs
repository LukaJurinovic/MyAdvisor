using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyAdvisor.Application.DTOs.Common;
using MyAdvisor.Application.DTOs.Transaction;
using MyAdvisor.Application.Interfaces.Services.App;

namespace MyAdvisor.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class TransactionController : BaseController
    {
        private readonly IDiaryTransactionService _diaryTransactionService;

        public TransactionController(IDiaryTransactionService diaryTransactionService)
        {
            _diaryTransactionService = diaryTransactionService;
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var userId = ResolveUserId();
            if (userId is null) return Unauthorized();

            try
            {
                var transaction = await _diaryTransactionService.GetByIdAsync(id, userId.Value);
                return Ok(transaction);
            }
            catch (KeyNotFoundException ex) { return NotFound(new ErrorResponse(ex.Message)); }
            catch (UnauthorizedAccessException) { return Forbid(); }
        }

        [HttpGet("diary/{diaryId:int}")]
        public async Task<IActionResult> GetByDiaryId(int diaryId)
        {
            var userId = ResolveUserId();
            if (userId is null) return Unauthorized();

            try
            {
                var transactions = await _diaryTransactionService.GetByDiaryIdAsync(diaryId, userId.Value);
                return Ok(transactions);
            }
            catch (KeyNotFoundException ex) { return NotFound(new ErrorResponse(ex.Message)); }
            catch (UnauthorizedAccessException) { return Forbid(); }
        }

        [HttpPost]
        public async Task<IActionResult> Add(AddTransactionRequestDto request)
        {
            var userId = ResolveUserId();
            if (userId is null) return Unauthorized();

            try
            {
                var transaction = await _diaryTransactionService.AddAsync(request, userId.Value);
                return StatusCode(StatusCodes.Status201Created, transaction);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ErrorResponse(ex.Message));
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, UpdateTransactionRequestDto request)
        {
            var userId = ResolveUserId();
            if (userId is null) return Unauthorized();

            try
            {
                var transaction = await _diaryTransactionService.UpdateAsync(id, request, userId.Value);
                return Ok(transaction);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ErrorResponse(ex.Message));
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = ResolveUserId();
            if (userId is null) return Unauthorized();

            try
            {
                await _diaryTransactionService.DeleteAsync(id, userId.Value);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ErrorResponse(ex.Message));
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }
    }
}
