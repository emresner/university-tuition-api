namespace University.Tuition.Api.Domain.Entities;

public class Payment
{
    public int Id { get; set; }

    public int StudentId { get; set; }

    public string Term { get; set; } = null!;

    public decimal Amount { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Student Student { get; set; } = null!;
}
