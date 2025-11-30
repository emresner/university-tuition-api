namespace University.Tuition.Api.Models;

public class TuitionBalanceDto
{
    public decimal TuitionTotal { get; set; }
    public decimal Paid { get; set; }
    public decimal Balance { get; set; }
}
