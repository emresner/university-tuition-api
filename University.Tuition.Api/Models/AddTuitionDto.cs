namespace University.Tuition.Api.Models;

public class AddTuitionDto
{
    public string StudentNo { get; set; } = null!;
    public string Term { get; set; } = null!;
    public decimal Amount { get; set; }
}
