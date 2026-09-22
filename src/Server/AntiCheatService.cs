using System;
using System.Collections.Concurrent;
using SoccerStar.Shared;

namespace SoccerStar.Server;

public sealed class AntiCheatService
{
    private sealed class Risk { public int InvalidPackets; public int SevereViolations; }
    private readonly ConcurrentDictionary<Guid, Risk> _risk = new();
    private readonly IServerTransport _transport;

    public AntiCheatService(IServerTransport transport) => _transport = transport;

    public bool ValidateSequence(ServerPlayer player, IClientMessage msg)
    {
        if (msg.Sequence <= player.LastAcceptedSequence)
        {
            Report(player.PlayerId, severe: false);
            return false;
        }
        player.LastAcceptedSequence = msg.Sequence;
        return true;
    }

    public void Report(Guid playerId, bool severe)
    {
        var risk = _risk.GetOrAdd(playerId, _ => new Risk());
        if (severe) risk.SevereViolations++; else risk.InvalidPackets++;

        // Correção e descarte vêm antes de punição. Desconexão apenas por abuso persistente.
        if (risk.SevereViolations >= 5 || risk.InvalidPackets >= 40)
            _transport.Disconnect(playerId, "Muitas requisições inválidas.");
    }
}
