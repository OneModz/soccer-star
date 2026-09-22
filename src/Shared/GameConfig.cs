namespace SoccerStar.Shared;

public static class GameConfig
{
    public const double MatchDurationSeconds = 300;
    public const float MaxKickStrength = 1.0f;
    public const float MaxKickImpulse = 100.0f;
    public const float MaxKickDistance = 5.0f;
    public const float MinKickForwardDot = 0.50f;
    public const float MaxAimAssistBallDistance = 10.0f;
    public const float AimAssistGoalSearchDistance = 40.0f;

    public const double ManualShootCooldown = 0.50;
    public const double FullAutoShootCooldown = 2.00;
    public const double MinFastAssistInterval = 0.35;
    public const double SlowAssistInterval = 1.25;

    public const float MaxDashSpeed = 9.0f;
    public const float MaxNormalSpeed = 6.0f;
    public const int MaxPacketBytes = 8 * 1024;

    public const int SemiAutoRequiredLevel = 1;
    public const int FullAutoRequiredLevel = 5;
    public const int SlowModeRequiredLevel = 1;
    public const int GlobalModeRequiredLevel = 10;
    public const int FastModeRequiredLevel = 10;
}
