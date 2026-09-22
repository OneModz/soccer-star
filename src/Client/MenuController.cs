using SoccerStar.Shared;

namespace SoccerStar.Client;

public sealed class MenuController
{
    private readonly IOverlayUi _ui;
    private readonly IClientTransport _net;
    private readonly IClientClock _clock;
    private readonly InputController _input;
    private bool _visible;

    public ClientSettings Settings { get; private set; } = Defaults.Settings;

    public MenuController(IOverlayUi ui, IClientTransport net, IClientClock clock, InputController input)
    {
        _ui = ui; _net = net; _clock = clock; _input = input;
        _ui.ToggleRequested += () => { _visible = !_visible; _ui.SetVisible(_visible); };
        _ui.SettingsChanged += OnSettingsChanged;
    }

    private void OnSettingsChanged(ClientSettings next)
    {
        Settings = next;
        _net.Send(new ConfigRequest(_input.NextSequence(), _clock.NowSeconds, next));
    }
}
