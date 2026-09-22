using System.Numerics;
using SoccerStar.Shared;

namespace SoccerStar.Client;

public sealed class AutoPlayController
{
    private readonly IClientWorld _world;
    private readonly IClientTransport _net;
    private readonly IClientClock _clock;
    private readonly InputController _inputController;
    private readonly AimAssist _aim;
    private double _nextActionAt;
    public ClientSettings Settings { get; set; } = Defaults.Settings;
    public bool Running { get; set; }

    public AutoPlayController(IClientWorld world, IClientTransport net, IClientClock clock, InputController inputController, AimAssist aim)
    {
        _world = world; _net = net; _clock = clock; _inputController = inputController; _aim = aim;
    }

    public void Tick(bool semiAutoShootHeld)
    {
        var cfg = Settings.AutoPlay;
        if (!Running || !cfg.Enabled || cfg.Mode == AssistMode.Off) return;
        if (_clock.NowSeconds < _nextActionAt) return;
        if (Vector3.Distance(_world.LocalPlayerPosition, _world.BallPosition) > GameConfig.MaxKickDistance) return;

        bool shouldShoot = cfg.Mode switch
        {
            AssistMode.SemiAuto => semiAutoShootHeld,
            AssistMode.FullAuto => true,
            AssistMode.SlowMode => true,
            AssistMode.FastMode => true,
            AssistMode.GlobalMode => true,
            _ => false
        };
        if (!shouldShoot) return;

        var dir = _aim.Apply(_world.CameraForward);
        _net.Send(new ShotRequest(_inputController.NextSequence(), _clock.NowSeconds, dir, MathGuard.Clamp01(cfg.Strength), true));

        double interval = cfg.Mode switch
        {
            AssistMode.FullAuto => System.Math.Max(cfg.ActionIntervalSeconds, GameConfig.FullAutoShootCooldown),
            AssistMode.SlowMode => System.Math.Max(cfg.ActionIntervalSeconds, GameConfig.SlowAssistInterval),
            AssistMode.FastMode => System.Math.Max(cfg.ActionIntervalSeconds, GameConfig.MinFastAssistInterval),
            _ => System.Math.Max(cfg.ActionIntervalSeconds, GameConfig.ManualShootCooldown)
        };
        _nextActionAt = _clock.NowSeconds + interval;
    }
}
