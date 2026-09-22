using System;
using System.Numerics;

namespace SoccerStar.Shared;

public interface IClientMessage
{
    long Sequence { get; }
    double ClientTime { get; }
}

public sealed record ShotRequest(long Sequence, double ClientTime, Vector3 Direction, float Strength, bool FromAssist) : IClientMessage;
public sealed record ActionRequest(long Sequence, double ClientTime, PlayerAction Action, Vector3 Direction) : IClientMessage;
public sealed record ConfigRequest(long Sequence, double ClientTime, ClientSettings Settings) : IClientMessage;
public sealed record QueueRequest(long Sequence, double ClientTime, TableFilter Filter, string? TableId, int CoinsBudget) : IClientMessage;
public sealed record RematchRequest(long Sequence, double ClientTime, string MatchId) : IClientMessage;
public sealed record ModerationHighlightRequest(long Sequence, double ClientTime, Guid TargetPlayerId, bool Enabled) : IClientMessage;

public sealed record BallSnapshot(Vector3 Position, Vector3 Velocity);
public sealed record PlayerSnapshot(Guid PlayerId, Vector3 Position, Vector3 Velocity, Vector3 Forward, Team Team);
public sealed record MatchSnapshot(string MatchId, double TimeRemaining, int RedScore, int BlueScore, BallSnapshot Ball, PlayerSnapshot[] Players);
public sealed record ServerNotice(string Code, string Message);
public sealed record QueueStatus(string State, string? TableId, string? MatchId);
public sealed record MatchFinishedNotice(string MatchId, int RedScore, int BlueScore, bool CanRematch);
public sealed record HighlightInstruction(Guid TargetPlayerId, bool Enabled);
