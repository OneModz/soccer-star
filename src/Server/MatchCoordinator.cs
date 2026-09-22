using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using SoccerStar.Shared;

namespace SoccerStar.Server;

public sealed class MatchCoordinator
{
    private readonly MatchmakingService _matchmaking;
    private readonly MatchService _matches;
    private readonly IPlayerRepository _repo;
    private readonly IServerTransport _transport;
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<Guid, bool>> _rematchVotes = new();
    private readonly Func<Guid, ServerPlayer?> _findPlayer;

    public MatchCoordinator(MatchmakingService matchmaking, MatchService matches, IPlayerRepository repo,
        IServerTransport transport, Func<Guid, ServerPlayer?> findPlayer)
    {
        _matchmaking = matchmaking; _matches = matches; _repo = repo; _transport = transport; _findPlayer = findPlayer;
    }

    public async Task HandleQueueAsync(ServerPlayer player, QueueRequest req, CancellationToken ct)
    {
        var pair = await _matchmaking.EnqueueAsync(player, req, ct);
        if (pair is null)
        {
            _transport.Send(player.PlayerId, new QueueStatus("waiting_or_rejected", req.TableId, null));
            return;
        }

        await _repo.CommitReservationAsync(pair.A.Player.PlayerId, pair.A.ReservationId, ct);
        await _repo.CommitReservationAsync(pair.B.Player.PlayerId, pair.B.ReservationId, ct);
        var match = _matches.Create(pair.A.Player, pair.B.Player, pair.Table.EntryFee);
        _transport.Send(pair.A.Player.PlayerId, new QueueStatus("matched", pair.Table.Id, match.MatchId));
        _transport.Send(pair.B.Player.PlayerId, new QueueStatus("matched", pair.Table.Id, match.MatchId));
    }

    public async Task SettleAsync(MatchState match, CancellationToken ct)
    {
        if (match.Settled) return;
        match.Settled = true;

        long redCoins = 0, blueCoins = 0;
        long redXp = 50, blueXp = 50;
        if (match.RedScore > match.BlueScore) { redCoins = match.EntryFee * 2L; redXp = 100; }
        else if (match.BlueScore > match.RedScore) { blueCoins = match.EntryFee * 2L; blueXp = 100; }
        else { redCoins = match.EntryFee; blueCoins = match.EntryFee; redXp = blueXp = 75; }

        await _repo.ApplyMatchResultAsync(match.RedPlayerId, match.MatchId, redXp, redCoins, ct);
        await _repo.ApplyMatchResultAsync(match.BluePlayerId, match.MatchId, blueXp, blueCoins, ct);

        var red = _findPlayer(match.RedPlayerId); var blue = _findPlayer(match.BluePlayerId);
        if (red is not null) red.MatchId = null;
        if (blue is not null) blue.MatchId = null;

        var notice = new MatchFinishedNotice(match.MatchId, match.RedScore, match.BlueScore, true);
        _transport.Send(match.RedPlayerId, notice);
        _transport.Send(match.BluePlayerId, notice);
    }

    public async Task HandleRematchAsync(ServerPlayer player, RematchRequest req, CancellationToken ct)
    {
        var old = _matches.Get(req.MatchId);
        if (old is null || !old.Finished) return;
        if (player.PlayerId != old.RedPlayerId && player.PlayerId != old.BluePlayerId) return;

        var votes = _rematchVotes.GetOrAdd(req.MatchId, _ => new ConcurrentDictionary<Guid, bool>());
        votes[player.PlayerId] = true;
        if (!votes.ContainsKey(old.RedPlayerId) || !votes.ContainsKey(old.BluePlayerId)) return;

        var red = _findPlayer(old.RedPlayerId); var blue = _findPlayer(old.BluePlayerId);
        if (red is null || blue is null || red.MatchId is not null || blue.MatchId is not null) return;

        string rr = $"rematch:{req.MatchId}:red";
        string rb = $"rematch:{req.MatchId}:blue";
        if (!await _repo.TryReserveCoinsAsync(red.PlayerId, old.EntryFee, rr, ct)) return;
        if (!await _repo.TryReserveCoinsAsync(blue.PlayerId, old.EntryFee, rb, ct))
        {
            await _repo.ReleaseReservationAsync(red.PlayerId, rr, ct);
            return;
        }
        await _repo.CommitReservationAsync(red.PlayerId, rr, ct);
        await _repo.CommitReservationAsync(blue.PlayerId, rb, ct);
        var next = _matches.Create(red, blue, old.EntryFee);
        _transport.Send(red.PlayerId, new QueueStatus("rematched", null, next.MatchId));
        _transport.Send(blue.PlayerId, new QueueStatus("rematched", null, next.MatchId));
        _rematchVotes.TryRemove(req.MatchId, out _);
    }
}
