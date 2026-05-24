using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyAdvisor.Application.DTOs.Common;
using MyAdvisor.Application.Interfaces.Services.App;

namespace MyAdvisor.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class StatisticsController : BaseController
    {
        private readonly ISpendingStatisticService _spendingStatisticService;

        public StatisticsController(ISpendingStatisticService spendingStatisticService)
        {
            _spendingStatisticService = spendingStatisticService;
        }

        [HttpGet("monthly")]
        public async Task<IActionResult> GetMonthly([FromQuery] int year, [FromQuery] int month)
        {
            var userId = ResolveUserId();
            if (userId is null) return Unauthorized();

            if (month < 1 || month > 12)
                return BadRequest(new ErrorResponse("Month must be between 1 and 12."));

            var statistic = await _spendingStatisticService.GetForMonthAsync(userId.Value, year, month);
            return Ok(statistic);
        }

        [HttpGet("yearly")]
        public async Task<IActionResult> GetYearly([FromQuery] int year)
        {
            var userId = ResolveUserId();
            if (userId is null) return Unauthorized();

            var statistics = await _spendingStatisticService.GetForYearAsync(userId.Value, year);
            return Ok(statistics);
        }
    }
}
