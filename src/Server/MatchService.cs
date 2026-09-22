using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using SoccerStar.Shared;

namespace SoccerStar.Server;

public sealed class MatchState
{
    public required string MatchId { get; init; }
    public required Guid RedPlayerId { get; init; }
    public required Guid BluePlayerId { get; init; }
    public required int EntryFee { get; init; }
    public double StartedAt { get; init; }
    public double EndsAt { get; init; }
    public int RedScore { get; set; }
    public int BlueScore { get; set; }
    public bool Finished { get; set; }
    public bool Settled { get; set; }
}

public sealed class MatchService
{
    private readonly ConcurrentDictionary<string, MatchState> _matches = new();
    private readonly IServerClock _clock;
    private readonly IWorldPhysics _physics;
    private readonly IServerTransport _transport;

    public MatchService(IServerClock clock, IWorldPhysics physics, IServerTransport transport)
    {
        _clock = clock; _physics = physics; _transport = transport;
    }

    public MatchState Create(ServerPlayer red, ServerPlayer blue, int entryFee)
    {
        string id = Guid.NewGuid().ToString("N");
        var state = new MatchState
        {
            MatchId = id,
            RedPlayerId = red.PlayerId,
            BluePlayerId = blue.PlayerId,
            EntryFee = entryFee,
            StartedAt = _clock.NowSeconds,
            EndsAt = _clock.NowSeconds + GameConfig.MatchDurationSeconds
        };
        _matches[id] = state;
        red.Team = Team.Red; blue.Team = Team.Blue;
        red.MatchId = id; blue.MatchId = id;
        return state;
    }

    public MatchState? Get(string id) => _matches.TryGetValue(id, out var m) ? m : null;

    public IReadOnlyList<MatchState> Tick(float dt)
    {
        var finishedNow = new List<MatchState>();
        foreach (var match in _matches.Values.Where(m => !m.Finished))
        {
            _physics.Step(match.MatchId, dt);
            if (_physics.TryConsumeGoal(match.MatchId, out var scoring))
            {
                if (scoring == Team.Red) match.RedScore++; else match.BlueScore++;
                _physics.ResetAfterGoal(match.MatchId);
            }

            if (_clock.NowSeconds >= match.EndsAt)
            {
                match.Finished = true;
                finishedNow.Add(match);
            }

            var ball = _physics.GetBall(match.MatchId);
            var players = _physics.GetPlayers(match.MatchId)
                .Select(p => new PlayerSnapshot(p.PlayerId, p.Position, System.Numerics.Vector3.Zero, p.Forward, p.Team)).ToArray();

            _transport.Broadcast(match.MatchId, new MatchSnapshot(
                match.MatchId,
                System.Math.Max(0, match.EndsAt - _clock.NowSeconds),
                match.RedScore,
                match.BlueScore,
                new BallSnapshot(ball.Position, ball.Velocity),
                players));
        }
        return finishedNow;
    }
}
