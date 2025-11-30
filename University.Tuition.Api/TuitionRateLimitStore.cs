using System.Collections.Concurrent;

namespace University.Tuition.Api;

public static class TuitionRateLimitStore
{
    // key: "studentNo-2025-01-15", value = count
    public static ConcurrentDictionary<string, int> DailyCounts { get; set; }
        = new ConcurrentDictionary<string, int>();
}
