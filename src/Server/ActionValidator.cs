using System;
using System.Numerics;
using SoccerStar.Shared;

namespace SoccerStar.Server;

public sealed class ActionValidator
{
    private readonly RateLimiter _rate;
    private readonly IServerClock _clock;
    private readonly System.Collections.Concurrent.ConcurrentDictionary<string, double> _last = new();

    public ActionValidator(RateLimiter rate, IServerClock clock)
    {
        _rate = rate;
        _clock = clock;
    }

    public bool ValidateShot(ServerPlayer player, BallState ball, ShotRequest req, out Vector3 direction, out float strength)
    {
        direction = Vector3.Zero;
        strength = 0;
        if (player.MatchId is null || !MathGuard.IsFinite(req.Direction) || !MathGuard.IsFinite(req.Strength)) return false;
        if (req.Strength < 0f || req.Strength > GameConfig.MaxKickStrength) return false;
        if (!_rate.Allow(player.PlayerId, "shoot-packet", 4, 1.0)) return false;

        float distance = Vector3.Distance(player.Position, ball.Position);
        if (distance > GameConfig.MaxKickDistance) return false;

        var normalized = MathGuard.SafeNormalize(req.Direction);
        var forward = MathGuard.SafeNormalize(player.Forward);
        if (normalized == Vector3.Zero || forward == Vector3.Zero) return false;
        if (Vector3.Dot(forward, normalized) < GameConfig.MinKickForwardDot) return false;

        double cooldown = req.FromAssist && player.Settings.AutoPlay.Mode == AssistMode.FullAuto
            ? GameConfig.FullAutoShootCooldown
            : GameConfig.ManualShootCooldown;

        string key = $"{player.PlayerId:N}:shoot";
        double now = _clock.NowSeconds;
        if (_last.TryGetValue(key, out var previous) && now - previous < cooldown) return false;
        _last[key] = now;

        direction = normalized;
        strength = req.Strength;
        return true;
    }

    public bool ValidateAction(ServerPlayer player, ActionRequest req)
    {
        if (player.MatchId is null || !MathGuard.IsFinite(req.Direction)) return false;
        int maxPerSecond = req.Action switch
        {
            PlayerAction.Pass => 4,
            PlayerAction.Tackle => 2,
            PlayerAction.Spin => 3,
            PlayerAction.Dash => 3,
            _ => 1
        };
        return _rate.Allow(player.PlayerId, $"action:{req.Action}", maxPerSecond, 1.0);
    }
}
