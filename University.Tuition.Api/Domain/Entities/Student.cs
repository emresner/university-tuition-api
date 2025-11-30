namespace University.Tuition.Api.Domain.Entities;

public class Student
{
    public int Id { get; set; }

    // Öğrenci numarası (örn. "20201234") — unique olacak (DbContext’te ayarlayacağız)
    public string StudentNo { get; set; } = null!;

    public string? FullName { get; set; }

    // Navigation collections
    public ICollection<TuitionCharge> Charges { get; set; } = new List<TuitionCharge>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
