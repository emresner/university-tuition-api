namespace University.Tuition.Api.Infrastructure.RateLimiting;

public class RateLimitOptions
{
    // Varsayılan 3; appsettings.json’dan override ederiz.
    public int TuitionQueriesPerDay { get; set; } = 3;
}
