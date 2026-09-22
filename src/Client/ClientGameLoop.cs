using SoccerStar.Shared;

namespace SoccerStar.Client;

public sealed class ClientGameLoop
{
    private readonly IInputSource _input;
    private readonly InputController _inputController;
    private readonly AimAssist _aim;
    private readonly AutoPlayController _autoPlay;
    private readonly AimLineController _lines;
    private readonly MenuController _menu;

    public ClientGameLoop(IInputSource input, InputController inputController, AimAssist aim, AutoPlayController autoPlay, AimLineController lines, MenuController menu)
    {
        _input = input; _inputController = inputController; _aim = aim; _autoPlay = autoPlay; _lines = lines; _menu = menu;
    }

    public void Tick()
    {
        _aim.Enabled = _menu.Settings.AutoPlay.Enabled;
        _aim.AssistStrength = _menu.Settings.AutoPlay.Strength;
        _autoPlay.Settings = _menu.Settings;
        _lines.Settings = _menu.Settings.Lines;

        _inputController.Tick(_aim.Apply);
        _autoPlay.Tick(_input.IsHeld("Shoot"));
        _lines.Tick();
    }
}
