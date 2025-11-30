using System.Diagnostics;
using System.Text;
using System.Linq; // <-- eklendi: header isimlerini toplamak için
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace University.Tuition.Api.Middleware
{
    public class RequestResponseLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestResponseLoggingMiddleware> _logger;

        public RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            var req = context.Request;
            var utcNow = DateTime.UtcNow;
            var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            // ---- Request body ölçümü (gerekirse) ----
            long requestSize = 0;
            string? authHeader = req.Headers["Authorization"];
            var authPresent = !string.IsNullOrEmpty(authHeader);

            // Body okunabilir olsun
            req.EnableBuffering();
            if (req.ContentLength.HasValue)
            {
                requestSize = req.ContentLength.Value;
            }
            else
            {
                using var reader = new StreamReader(req.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true);
                var body = await reader.ReadToEndAsync();
                requestSize = Encoding.UTF8.GetByteCount(body);
                req.Body.Position = 0;
            }

            // ---- Response yakalama ----
            var originalBody = context.Response.Body;
            await using var memStream = new MemoryStream();
            context.Response.Body = memStream;

            var sw = Stopwatch.StartNew();
            try
            {
                await _next(context);
            }
            finally
            {
                sw.Stop();

                // response size
                context.Response.Body.Seek(0, SeekOrigin.Begin);
                var responseText = await new StreamReader(context.Response.Body).ReadToEndAsync();
                var responseSize = Encoding.UTF8.GetByteCount(responseText);
                context.Response.Body.Seek(0, SeekOrigin.Begin);

                // *** AUTH durumunu pipeline SONRASINDA ölç ***
                var isAuthenticatedNow = context.User?.Identity?.IsAuthenticated == true;

                // *** Header isimleri ve User-Agent ***
                var headerNames = string.Join(",", req.Headers.Select(h => h.Key));
                var userAgent = req.Headers.UserAgent.ToString();

                // ---- Request log (PDF alanlarıyla) ----
                _logger.LogInformation(
                    "[REQ] {Method} {Path} | ts_utc={Utc} | ip={IP} | auth_present={AuthHeader} | auth_ok={AuthOk} | req_size={ReqSize}B | headers=[{Headers}] | ua={UA}",
                    req.Method,
                    req.Path + req.QueryString,
                    utcNow.ToString("O"),
                    ip,
                    authPresent,
                    isAuthenticatedNow,
                    requestSize,
                    headerNames,
                    userAgent
                );

                // ---- Response log ----
                _logger.LogInformation(
                    "[RES] {Method} {Path} | status={Status} | latency_ms={Latency} | res_size={ResSize}B",
                    req.Method,
                    req.Path + req.QueryString,
                    context.Response.StatusCode,
                    sw.ElapsedMilliseconds,
                    responseSize
                );

                // orijinal response'u geri yaz
                await memStream.CopyToAsync(originalBody);
                context.Response.Body = originalBody;
            }
        }
    }
}
