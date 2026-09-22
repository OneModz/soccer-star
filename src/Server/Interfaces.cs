using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using SoccerStar.Shared;

namespace SoccerStar.Server;

public interface IServerClock { double NowSeconds { get; } }

public interface IServerTransport
{
    void Send(Guid playerId, object message);
    void Broadcast(string matchId, object message);
    void Disconnect(Guid playerId, string reason);
}

public interface IWorldPhysics
{
    BallState GetBall(string matchId);
    IReadOnlyList<ServerPlayer> GetPlayers(string matchId);
    void ApplyBallImpulse(string matchId, Vector3 impulse);
    void ApplyPlayerAction(string matchId, Guid playerId, PlayerAction action, Vector3 direction);
    void Step(string matchId, float deltaSeconds);
    bool TryConsumeGoal(string matchId, out Team scoringTeam);
    void ResetAfterGoal(string matchId);
}

public interface IPlayerRepository
{
    Task<PlayerProfile?> LoadAsync(Guid playerId, CancellationToken ct);
    Task<bool> TryReserveCoinsAsync(Guid playerId, long coins, string reservationId, CancellationToken ct);
    Task CommitReservationAsync(Guid playerId, string reservationId, CancellationToken ct);
    Task ReleaseReservationAsync(Guid playerId, string reservationId, CancellationToken ct);
    Task ApplyMatchResultAsync(Guid playerId, string matchId, long xpDelta, long coinDelta, CancellationToken ct);
}

public interface IPermissionService
{
    bool IsModerator(Guid playerId);
}

public sealed class BallState
{
    public Vector3 Position { get; set; }
    public Vector3 Velocity { get; set; }
}
