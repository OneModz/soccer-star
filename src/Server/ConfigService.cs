using SoccerStar.Shared;

namespace SoccerStar.Server;

public sealed class ConfigService
{
    public bool TryApply(ServerPlayer player, ClientSettings requested)
    {
        if (requested is null) return false;
        var a = requested.AutoPlay;
        if (a.Strength < 0 || a.Strength > 1) return false;
        if (a.ActionIntervalSeconds < 0.1f || a.ActionIntervalSeconds > 10f) return false;
        if (a.CoinsToUse < 0) return false;
        if (requested.Lines.Width <= 0 || requested.Lines.Width > 2) return false;
        if (requested.Lines.Opacity < 0 || requested.Lines.Opacity > 1) return false;

        int required = a.Mode switch
        {
            AssistMode.FullAuto => GameConfig.FullAutoRequiredLevel,
            AssistMode.GlobalMode => GameConfig.GlobalModeRequiredLevel,
            AssistMode.FastMode => GameConfig.FastModeRequiredLevel,
            AssistMode.SlowMode => GameConfig.SlowModeRequiredLevel,
            AssistMode.SemiAuto => GameConfig.SemiAutoRequiredLevel,
            _ => 0
        };
        if (player.Level < required) return false;

        // FastMode é uma opção legítima de UX, mas nunca pode ultrapassar limites do servidor.
        double minInterval = a.Mode switch
        {
            AssistMode.FullAuto => GameConfig.FullAutoShootCooldown,
            AssistMode.SlowMode => GameConfig.SlowAssistInterval,
            AssistMode.FastMode => GameConfig.MinFastAssistInterval,
            _ => GameConfig.ManualShootCooldown
        };

        var normalizedAuto = a with { ActionIntervalSeconds = (float)System.Math.Max(a.ActionIntervalSeconds, minInterval) };
        player.Settings = requested with { AutoPlay = normalizedAuto };
        return true;
    }
}
