using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using University.Tuition.Api.Domain.Entities;
using University.Tuition.Api.Infrastructure.Data;
using University.Tuition.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.Globalization;

namespace University.Tuition.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin/tuition")]
[Authorize(Roles = "Admin")]
[Produces("application/json")]
public class AdminTuitionController : ControllerBase
{
    private readonly TuitionDbContext _db;

    public AdminTuitionController(TuitionDbContext db)
    {
        _db = db;
    }

    /// <summary>Add single tuition charge (Admin)</summary>
    [HttpPost]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddTuition([FromBody] AddTuitionDto dto)
    {
        if (dto.Amount <= 0)
            return BadRequest("Amount must be > 0.");

        var student = await _db.Students.FirstOrDefaultAsync(s => s.StudentNo == dto.StudentNo);
        if (student == null)
            return NotFound($"Student {dto.StudentNo} not found.");

        var charge = new TuitionCharge
        {
            StudentId = student.Id,
            Term = dto.Term,
            Amount = dto.Amount,
            CreatedAt = DateTime.UtcNow
        };

        _db.TuitionCharges.Add(charge);
        await _db.SaveChangesAsync();

        return Ok(new { status = "Success", message = "Tuition added." });
    }

    /// <summary>Batch CSV upload (Admin)</summary>
    [HttpPost("batch")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AddTuitionBatch([FromForm] TuitionBatchUploadDto input)
    {
        var file = input.File;
        if (file == null || file.Length == 0)
            return BadRequest("CSV file is required.");

        using var stream = new StreamReader(file.OpenReadStream());

        int lineNumber = 0;
        while (!stream.EndOfStream)
        {
            var line = await stream.ReadLineAsync();
            lineNumber++;

            if (lineNumber == 1) continue; // skip header

            var parts = line.Split(',');
            if (parts.Length != 3)
                return BadRequest($"Invalid format on line {lineNumber}.");

            var studentNo = parts[0].Trim();
            var term = parts[1].Trim();
            var amountStr = parts[2].Trim();

            if (!decimal.TryParse(amountStr, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal amount))
                return BadRequest($"Invalid amount on line {lineNumber}.");

            var student = await _db.Students.FirstOrDefaultAsync(s => s.StudentNo == studentNo);
            if (student == null)
                return BadRequest($"Student {studentNo} not found (line {lineNumber}).");

            _db.TuitionCharges.Add(new TuitionCharge
            {
                StudentId = student.Id,
                Term = term,
                Amount = amount,
                CreatedAt = DateTime.UtcNow
            });
        }

        await _db.SaveChangesAsync();
        return Ok(new { status = "Success", message = "Batch tuition add completed." });
    }

    /// <summary>Unpaid list (Admin, paginated)</summary>
    [HttpGet("unpaid")]
    [ProducesResponseType(typeof(PagedResult<UnpaidStudentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetUnpaid(
        [FromQuery] string term,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        if (string.IsNullOrWhiteSpace(term))
            return BadRequest("term is required.");

        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;
        if (pageSize > 100) pageSize = 100;

        var baseQuery = _db.Students
            .Select(s => new
            {
                s.StudentNo,
                s.FullName,
                Term = term,
                TuitionTotal = _db.TuitionCharges
                    .Where(c => c.StudentId == s.Id && c.Term == term)
                    .Sum(c => (decimal?)c.Amount) ?? 0m,
                Paid = _db.Payments
                    .Where(p => p.StudentId == s.Id && p.Term == term)
                    .Sum(p => (decimal?)p.Amount) ?? 0m
            })
            .Select(x => new UnpaidStudentDto
            {
                StudentNo = x.StudentNo,
                FullName = x.FullName,
                Term = x.Term,
                TuitionTotal = x.TuitionTotal,
                Paid = x.Paid,
                Balance = x.TuitionTotal - x.Paid
            })
            .Where(x => x.Balance > 0);

        var totalCount = await baseQuery.CountAsync();

        var items = await baseQuery
            .OrderBy(x => x.StudentNo)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var result = new PagedResult<UnpaidStudentDto>
        {
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            Items = items
        };

        return Ok(result);
    }
}
