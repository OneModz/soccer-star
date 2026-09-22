using System;
using System.Numerics;

namespace SoccerStar.Shared;

public static class MathGuard
{
    public static bool IsFinite(float v) => !float.IsNaN(v) && !float.IsInfinity(v);
    public static bool IsFinite(Vector3 v) => IsFinite(v.X) && IsFinite(v.Y) && IsFinite(v.Z);

    public static Vector3 SafeNormalize(Vector3 v)
    {
        if (!IsFinite(v) || v.LengthSquared() < 0.000001f) return Vector3.Zero;
        return Vector3.Normalize(v);
    }

    public static float Clamp01(float value) => Math.Clamp(value, 0f, 1f);
}
