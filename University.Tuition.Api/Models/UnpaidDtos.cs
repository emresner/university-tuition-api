namespace University.Tuition.Api.Models;

public class UnpaidStudentDto
{
    public string StudentNo { get; set; } = null!;
    public string? FullName { get; set; }
    public string Term { get; set; } = null!;
    public decimal TuitionTotal { get; set; }
    public decimal Paid { get; set; }
    public decimal Balance { get; set; }
}

public class PagedResult<T>
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public IEnumerable<T> Items { get; set; } = Array.Empty<T>();
}
