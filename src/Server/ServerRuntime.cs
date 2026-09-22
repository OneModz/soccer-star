using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using SoccerStar.Shared;

namespace SoccerStar.Server;

public sealed class ServerRuntime
{
    private readonly ConcurrentDictionary<Guid, ServerPlayer> _players = new();
    private readonly ServerMessageRouter _router;
    private readonly MatchCoordinator _coordinator;
    private readonly MatchService _matches;

    public ServerRuntime(ServerMessageRouter router, MatchCoordinator coordinator, MatchService matches)
    {
        _router = router; _coordinator = coordinator; _matches = matches;
    }

    public void Register(ServerPlayer player) => _players[player.PlayerId] = player;
    public void Unregister(Guid playerId) => _players.TryRemove(playerId, out _);
    public ServerPlayer? Find(Guid id) => _players.TryGetValue(id, out var p) ? p : null;

    public async Task ReceiveAsync(Guid playerId, IClientMessage message, CancellationToken ct)
    {
        var player = Find(playerId);
        if (player is null) return;
        switch (message)
        {
            case QueueRequest queue:
                await _coordinator.HandleQueueAsync(player, queue, ct);
                break;
            case RematchRequest rematch:
                await _coordinator.HandleRematchAsync(player, rematch, ct);
                break;
            default:
                _router.Handle(player, message);
                break;
        }
    }

    public async Task TickAsync(float dt, CancellationToken ct)
    {
        foreach (var finished in _matches.Tick(dt))
            await _coordinator.SettleAsync(finished, ct);
    }
}
