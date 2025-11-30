using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using University.Tuition.Api.Infrastructure.Data;
using University.Tuition.Api.Models;

namespace University.Tuition.Api.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}")]
    [Produces("application/json")]
    public class TuitionController : ControllerBase
    {
        private readonly TuitionDbContext _db;
        public TuitionController(TuitionDbContext db) => _db = db;

        private async Task<TuitionBalanceDto?> ComputeAsync(string studentNo, string term)
        {
            if (string.IsNullOrWhiteSpace(studentNo) || string.IsNullOrWhiteSpace(term))
                return null;

            var student = await _db.Students.FirstOrDefaultAsync(s => s.StudentNo == studentNo);
            if (student is null) return null;

            var tuitionTotal = await _db.TuitionCharges
                .Where(c => c.StudentId == student.Id && c.Term == term)
                .SumAsync(c => (decimal?)c.Amount) ?? 0;

            var paid = await _db.Payments
                .Where(p => p.StudentId == student.Id && p.Term == term)
                .SumAsync(p => (decimal?)p.Amount) ?? 0;

            return new TuitionBalanceDto
            {
                TuitionTotal = tuitionTotal,
                Paid = paid,
                Balance = tuitionTotal - paid
            };
        }

        /// <summary>University Mobile App — NO Auth</summary>
        [HttpGet("mobile/tuition")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(TuitionBalanceDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)] // rate limit
        public async Task<IActionResult> MobileQuery([FromQuery] string studentNo, [FromQuery] string term)
        {
            var dto = await ComputeAsync(studentNo, term);
            if (dto is null) return NotFound("Student or term not found / invalid.");
            return Ok(dto);
        }

        /// <summary>Banking App — YES Auth</summary>
        [HttpGet("banking/tuition")]
        [Authorize]
        [ProducesResponseType(typeof(TuitionBalanceDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> BankingQuery([FromQuery] string studentNo, [FromQuery] string term)
        {
            var dto = await ComputeAsync(studentNo, term);
            if (dto is null) return NotFound("Student or term not found / invalid.");
            return Ok(dto);
        }
    }
}
