using System;
using System.Numerics;

namespace SoccerStar.Shared;

public enum Team { Red, Blue }
public enum AssistMode { Off, SemiAuto, FullAuto, SlowMode, GlobalMode, FastMode }
public enum TableFilter { MixedJoin, FixOnSingleTable }
public enum LineStyle { Solid, Dashed, Dotted }
public enum PlayerAction { Shoot, Pass, Tackle, Spin, Dash }

public sealed record AutoPlaySettings(
    bool Enabled,
    AssistMode Mode,
    float Strength,
    float ActionIntervalSeconds,
    bool AutoQueue,
    int CoinsToUse,
    TableFilter TableFilter,
    string? FixedTableId,
    bool AutoRematch);

public sealed record AimLineSettings(
    bool Enabled,
    bool KeepAfterShot,
    LineStyle Style,
    float Width,
    float Opacity);

public sealed record PlayerInfoSettings(bool ShowInfo, bool ShowStats, bool ShowTeam);
public sealed record OverlaySettings(float X, float Y, float Width, float Height, float Opacity, bool Visible);

public sealed record ClientSettings(
    AutoPlaySettings AutoPlay,
    AimLineSettings Lines,
    PlayerInfoSettings PlayerInfo,
    OverlaySettings Overlay,
    string Language,
    string ProfileName);

public sealed record PlayerProfile(Guid PlayerId, string Name, int Level, long Xp, long Coins, long Version);

public sealed class ServerPlayer
{
    public required Guid PlayerId { get; init; }
    public required string Name { get; init; }
    public int Level { get; set; }
    public long Coins { get; set; }
    public Vector3 Position { get; set; }
    public Vector3 Forward { get; set; } = Vector3.UnitZ;
    public Team Team { get; set; }
    public long LastAcceptedSequence { get; set; }
    public string? MatchId { get; set; }
    public ClientSettings Settings { get; set; } = Defaults.Settings;
}

public static class Defaults
{
    public static readonly ClientSettings Settings = new(
        new AutoPlaySettings(false, AssistMode.SemiAuto, 0.5f, 0.5f, false, 0, TableFilter.MixedJoin, null, false),
        new AimLineSettings(true, false, LineStyle.Solid, 0.15f, 0.6f),
        new PlayerInfoSettings(true, true, true),
        new OverlaySettings(0.90f, 0.08f, 60, 60, 0.30f, true),
        "pt-BR",
        "Padrao");
}
