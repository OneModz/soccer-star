using System;
using System.Collections.Concurrent;

namespace SoccerStar.Server;

public sealed class RateLimiter
{
    private sealed class Bucket
    {
        public int Count;
        public double ResetAt;
        public readonly object Sync = new();
    }

    private readonly ConcurrentDictionary<string, Bucket> _buckets = new();
    private readonly IServerClock _clock;

    public RateLimiter(IServerClock clock) => _clock = clock;

    public bool Allow(Guid playerId, string action, int maxRequests, double windowSeconds)
    {
        string key = $"{playerId:N}:{action}";
        var bucket = _buckets.GetOrAdd(key, _ => new Bucket { Count = 0, ResetAt = _clock.NowSeconds + windowSeconds });

        lock (bucket.Sync)
        {
            double now = _clock.NowSeconds;
            if (now >= bucket.ResetAt)
            {
                bucket.Count = 0;
                bucket.ResetAt = now + windowSeconds;
            }

            if (bucket.Count >= maxRequests) return false;
            bucket.Count++;
            return true;
        }
    }
}
