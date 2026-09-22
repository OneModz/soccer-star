using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SoccerStar.Shared;

namespace SoccerStar.Server;

// Apenas para testes locais. Em produção, substitua por banco transacional.
public sealed class InMemoryPlayerRepository : IPlayerRepository
{
    private sealed class Row
    {
        public required PlayerProfile Profile;
        public readonly Dictionary<string, long> Reservations = new();
        public readonly HashSet<string> AppliedMatches = new();
        public readonly object Sync = new();
    }
    private readonly ConcurrentDictionary<Guid, Row> _rows = new();

    public void Seed(PlayerProfile profile) => _rows[profile.PlayerId] = new Row { Profile = profile };

    public Task<PlayerProfile?> LoadAsync(Guid playerId, CancellationToken ct) =>
        Task.FromResult(_rows.TryGetValue(playerId, out var row) ? row.Profile : null);

    public Task<bool> TryReserveCoinsAsync(Guid playerId, long coins, string reservationId, CancellationToken ct)
    {
        if (coins < 0 || !_rows.TryGetValue(playerId, out var row)) return Task.FromResult(false);
        lock (row.Sync)
        {
            if (row.Reservations.ContainsKey(reservationId)) return Task.FromResult(true);
            long reserved = 0; foreach (var v in row.Reservations.Values) reserved += v;
            if (row.Profile.Coins - reserved < coins) return Task.FromResult(false);
            row.Reservations[reservationId] = coins;
            return Task.FromResult(true);
        }
    }

    public Task CommitReservationAsync(Guid playerId, string reservationId, CancellationToken ct)
    {
        if (!_rows.TryGetValue(playerId, out var row)) return Task.CompletedTask;
        lock (row.Sync)
        {
            if (!row.Reservations.Remove(reservationId, out var amount)) return Task.CompletedTask;
            row.Profile = row.Profile with { Coins = row.Profile.Coins - amount, Version = row.Profile.Version + 1 };
        }
        return Task.CompletedTask;
    }

    public Task ReleaseReservationAsync(Guid playerId, string reservationId, CancellationToken ct)
    {
        if (_rows.TryGetValue(playerId, out var row)) lock (row.Sync) row.Reservations.Remove(reservationId);
        return Task.CompletedTask;
    }

    public Task ApplyMatchResultAsync(Guid playerId, string matchId, long xpDelta, long coinDelta, CancellationToken ct)
    {
        if (!_rows.TryGetValue(playerId, out var row)) return Task.CompletedTask;
        lock (row.Sync)
        {
            if (!row.AppliedMatches.Add(matchId)) return Task.CompletedTask; // idempotência
            long nextXp = row.Profile.Xp + Math.Max(0, xpDelta);
            int nextLevel = 1 + (int)(nextXp / 1000);
            row.Profile = row.Profile with
            {
                Xp = nextXp,
                Level = nextLevel,
                Coins = row.Profile.Coins + Math.Max(0, coinDelta),
                Version = row.Profile.Version + 1
            };
        }
        return Task.CompletedTask;
    }
}
