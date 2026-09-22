using System.Numerics;
using SoccerStar.Shared;

namespace SoccerStar.Client;

public sealed class AimAssist
{
    private readonly IClientWorld _world;
    public float AssistStrength { get; set; } = 0.30f;
    public bool Enabled { get; set; }

    public AimAssist(IClientWorld world) => _world = world;

    public Vector3 Apply(Vector3 rawDirection)
    {
        if (!Enabled) return MathGuard.SafeNormalize(rawDirection);
        if (Vector3.Distance(_world.LocalPlayerPosition, _world.BallPosition) > GameConfig.MaxAimAssistBallDistance)
            return MathGuard.SafeNormalize(rawDirection);

        Vector3 goal = _world.LocalTeam == Team.Red ? _world.BlueGoalPosition : _world.RedGoalPosition;
        Vector3 goalDir = MathGuard.SafeNormalize(goal - _world.BallPosition);
        Vector3 raw = MathGuard.SafeNormalize(rawDirection);
        if (raw == Vector3.Zero || goalDir == Vector3.Zero) return raw;
        float t = System.Math.Clamp(AssistStrength, 0f, 1f);
        return MathGuard.SafeNormalize(Vector3.Lerp(raw, goalDir, t));
    }
}
