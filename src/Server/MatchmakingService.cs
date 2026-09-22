using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SoccerStar.Shared;

namespace SoccerStar.Server;

public sealed record MatchTable(string Id, int EntryFee, int MinLevel, int MaxLevel, int Capacity = 2);
public sealed record QueueEntry(ServerPlayer Player, MatchTable Table, string ReservationId);
public sealed record PairingResult(MatchTable Table, QueueEntry A, QueueEntry B);

public sealed class MatchmakingService
{
    private readonly IPlayerRepository _repo;
    private readonly ConcurrentDictionary<string, ConcurrentQueue<QueueEntry>> _queues = new();
    private readonly Dictionary<string, MatchTable> _tables;

    public MatchmakingService(IPlayerRepository repo, IEnumerable<MatchTable> tables)
    {
        _repo = repo;
        _tables = tables.ToDictionary(x => x.Id, StringComparer.OrdinalIgnoreCase);
        foreach (var table in _tables.Values) _queues[table.Id] = new ConcurrentQueue<QueueEntry>();
    }

    public MatchTable? ResolveTable(ServerPlayer player, QueueRequest req)
    {
        if (req.CoinsBudget < 0) return null;
        if (req.Filter == TableFilter.FixOnSingleTable)
        {
            if (req.TableId is null || !_tables.TryGetValue(req.TableId, out var fixedTable)) return null;
            return Eligible(player, fixedTable, req.CoinsBudget) ? fixedTable : null;
        }

        return _tables.Values
            .Where(t => Eligible(player, t, req.CoinsBudget))
            .OrderByDescending(t => t.EntryFee)
            .FirstOrDefault();
    }

    private static bool Eligible(ServerPlayer p, MatchTable t, int budget) =>
        p.MatchId is null && p.Level >= t.MinLevel && p.Level <= t.MaxLevel &&
        t.EntryFee <= budget && t.EntryFee <= p.Coins;

    public async Task<PairingResult?> EnqueueAsync(ServerPlayer player, QueueRequest req, CancellationToken ct)
    {
        var table = ResolveTable(player, req);
        if (table is null) return null;

        string reservationId = $"queue:{player.PlayerId:N}:{Guid.NewGuid():N}";
        if (!await _repo.TryReserveCoinsAsync(player.PlayerId, table.EntryFee, reservationId, ct)) return null;

        var entry = new QueueEntry(player, table, reservationId);
        var q = _queues[table.Id];
        q.Enqueue(entry);

        if (!q.TryDequeue(out var a)) return null;
        if (!q.TryDequeue(out var b))
        {
            q.Enqueue(a);
            return null;
        }

        if (a.Player.PlayerId == b.Player.PlayerId)
        {
            q.Enqueue(a);
            await _repo.ReleaseReservationAsync(b.Player.PlayerId, b.ReservationId, ct);
            return null;
        }

        return new PairingResult(table, a, b);
    }
}
