using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using University.Tuition.Api.Infrastructure.RateLimiting;

namespace University.Tuition.Api.Middleware;

public class TuitionRateLimitMiddleware
{
    private readonly RequestDelegate _next;
    private readonly RateLimitOptions _options;

    public TuitionRateLimitMiddleware(RequestDelegate next, IOptions<RateLimitOptions> options)
    {
        _next = next;
        _options = options.Value;
    }

    public async Task Invoke(HttpContext context)
    {
        var req = context.Request;

        // Sadece: GET /api/v*/mobile/tuition
        var path = req.Path.Value ?? string.Empty;
        if (HttpMethods.IsGet(req.Method) &&
            path.Contains("/mobile/tuition", StringComparison.OrdinalIgnoreCase))
        {
            var studentNo = req.Query["studentNo"].ToString();

            if (!string.IsNullOrWhiteSpace(studentNo))
            {
                var today = DateTime.UtcNow.Date;
                var key = $"{studentNo}-{today:yyyy-MM-dd}";

                TuitionRateLimitStore.DailyCounts.TryGetValue(key, out var currentCount);

                var limit = _options.TuitionQueriesPerDay;
                var remainingIfAllowed = Math.Max(0, limit - (currentCount + 1));

                // Başarılı senaryo için de önceden header’ları set edelim
                context.Response.Headers["X-RateLimit-Limit"] = limit.ToString();
                context.Response.Headers["X-RateLimit-Remaining"] = remainingIfAllowed.ToString();

                if (currentCount >= limit)
                {
                    // yarın UTC 00:00'a saniye
                    var now = DateTime.UtcNow;
                    var resetUtc = today.AddDays(1);
                    var retryAfter = (int)(resetUtc - now).TotalSeconds;

                    context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    context.Response.Headers["Retry-After"] = retryAfter.ToString();
                    context.Response.Headers["X-RateLimit-Reset"] = resetUtc.ToString("O");

                    await context.Response.WriteAsJsonAsync(new
                    {
                        status = "TooManyRequests",
                        message = $"Daily query limit ({limit}) exceeded for student {studentNo}."
                    });
                    return;
                }

                // izin ver ve sayaç artır
                TuitionRateLimitStore.DailyCounts.AddOrUpdate(key, 1, (_, v) => v + 1);
            }
        }

        await _next(context);
    }
}
