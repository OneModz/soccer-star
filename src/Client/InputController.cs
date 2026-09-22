using System;
using SoccerStar.Shared;

namespace SoccerStar.Client;

public sealed class InputController
{
    private readonly IInputSource _input;
    private readonly IClientTransport _net;
    private readonly IClientClock _clock;
    private readonly IClientWorld _world;
    private long _sequence;
    private bool _charging;
    private double _chargeStarted;
    private const double MaxCharge = 1.5;

    public InputController(IInputSource input, IClientTransport net, IClientClock clock, IClientWorld world)
    {
        _input = input; _net = net; _clock = clock; _world = world;
    }

    public void Tick(Func<System.Numerics.Vector3, System.Numerics.Vector3>? aimTransform = null)
    {
        if (_input.WasPressed("Shoot")) { _charging = true; _chargeStarted = _clock.NowSeconds; }
        if (_input.WasReleased("Shoot") && _charging)
        {
            _charging = false;
            float strength = (float)Math.Min((_clock.NowSeconds - _chargeStarted) / MaxCharge, 1.0);
            var dir = _world.LocalPlayerForward;
            if (aimTransform is not null) dir = aimTransform(dir);
            _net.Send(new ShotRequest(++_sequence, _clock.NowSeconds, dir, strength, false));
        }

        if (_input.WasPressed("Pass")) SendAction(PlayerAction.Pass);
        if (_input.WasPressed("Tackle")) SendAction(PlayerAction.Tackle);
        if (_input.WasPressed("Spin")) SendAction(PlayerAction.Spin);
        if (_input.WasPressed("Dash")) SendAction(PlayerAction.Dash);
    }

    private void SendAction(PlayerAction action) =>
        _net.Send(new ActionRequest(++_sequence, _clock.NowSeconds, action, _world.LocalPlayerForward));

    public long NextSequence() => ++_sequence;
}
