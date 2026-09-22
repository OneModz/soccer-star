using System.Numerics;
using SoccerStar.Shared;

namespace SoccerStar.Client;

public sealed class AimLineController
{
    private readonly IClientWorld _world;
    private readonly ILineRenderer _renderer;
    private readonly AimAssist _aim;
    public AimLineSettings Settings { get; set; } = Defaults.Settings.Lines;

    public AimLineController(IClientWorld world, ILineRenderer renderer, AimAssist aim)
    {
        _world = world; _renderer = renderer; _aim = aim;
    }

    public void Tick()
    {
        if (!Settings.Enabled) { _renderer.Hide(); return; }
        if (Vector3.Distance(_world.LocalPlayerPosition, _world.BallPosition) > 15f)
        {
            if (!Settings.KeepAfterShot) _renderer.Hide();
            return;
        }
        Vector3 dir = _aim.Apply(_world.CameraForward);
        _renderer.Draw(_world.BallPosition, _world.BallPosition + dir * 30f, Settings.Style, Settings.Width, Settings.Opacity);
    }
}
