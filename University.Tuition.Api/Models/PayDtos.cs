namespace University.Tuition.Api.Models;

public class PayRequestDto
{
    public string StudentNo { get; set; } = null!;
    public string Term { get; set; } = null!;
    public decimal Amount { get; set; }
}

public class PayResponseDto
{
    public string Status { get; set; } = "Successful"; // or "Error"
    public string? Message { get; set; }

    public decimal TuitionTotal { get; set; }
    public decimal Paid { get; set; }
    public decimal Balance { get; set; }
}
