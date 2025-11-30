namespace University.Tuition.Api.Domain.Entities;

public class TuitionCharge
{
    public int Id { get; set; }

    public int StudentId { get; set; }

    // Örn: "2024-Fall", "2025-Spring"
    public string Term { get; set; } = null!;

    // TL türünde parasal değer — DbContext’te decimal(18,2) yapacağız
    public decimal Amount { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Student Student { get; set; } = null!;
}
