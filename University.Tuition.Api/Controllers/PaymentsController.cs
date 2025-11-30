using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using University.Tuition.Api.Domain.Entities;
using University.Tuition.Api.Infrastructure.Data;
using University.Tuition.Api.Models;

namespace University.Tuition.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
public class PaymentsController : ControllerBase
{
    private readonly TuitionDbContext _db;
    public PaymentsController(TuitionDbContext db) => _db = db;

    /// <summary>Records a payment for given student and term. Auth: NO</summary>
    [HttpPost("/api/v{version:apiVersion}/pay")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PayResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PayResponseDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(PayResponseDto), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Pay([FromBody] PayRequestDto request)
    {
        if (request.Amount <= 0)
            return BadRequest(new PayResponseDto { Status = "Error", Message = "Amount must be positive." });

        var student = await _db.Students.FirstOrDefaultAsync(s => s.StudentNo == request.StudentNo);
        if (student is null)
            return NotFound(new PayResponseDto { Status = "Error", Message = $"Student {request.StudentNo} not found." });

        var tuitionTotal = await _db.TuitionCharges
            .Where(c => c.StudentId == student.Id && c.Term == request.Term)
            .SumAsync(c => (decimal?)c.Amount) ?? 0;

        if (tuitionTotal == 0)
            return BadRequest(new PayResponseDto { Status = "Error", Message = $"No tuition for term {request.Term}." });

        var alreadyPaid = await _db.Payments
            .Where(p => p.StudentId == student.Id && p.Term == request.Term)
            .SumAsync(p => (decimal?)p.Amount) ?? 0;

        var balance = tuitionTotal - alreadyPaid;

        if (balance <= 0)
            return BadRequest(new PayResponseDto { Status = "Error", Message = "No remaining balance for this term." });

        if (request.Amount > balance)
        {
            return BadRequest(new PayResponseDto
            {
                Status = "Error",
                Message = $"Amount exceeds remaining balance ({balance:N2}).",
                TuitionTotal = tuitionTotal,
                Paid = alreadyPaid,
                Balance = balance
            });
        }

        var payment = new Payment
        {
            StudentId = student.Id,
            Term = request.Term,
            Amount = request.Amount,
            CreatedAt = DateTime.UtcNow
        };
        _db.Payments.Add(payment);
        await _db.SaveChangesAsync();

        var newPaid = alreadyPaid + request.Amount;
        var newBalance = tuitionTotal - newPaid;

        return Ok(new PayResponseDto
        {
            Status = "Successful",
            Message = newBalance > 0 ? "Partial payment recorded." : "Payment completed; balance is zero.",
            TuitionTotal = tuitionTotal,
            Paid = newPaid,
            Balance = newBalance
        });
    }
}
