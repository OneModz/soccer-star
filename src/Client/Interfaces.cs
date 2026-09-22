using System;
using System.Numerics;
using SoccerStar.Shared;

namespace SoccerStar.Client;

public interface IClientClock { double NowSeconds { get; } }
public interface IClientTransport { void Send(IClientMessage message); }
public interface IInputSource
{
    bool IsHeld(string action);
    bool WasPressed(string action);
    bool WasReleased(string action);
}
public interface IClientWorld
{
    Vector3 LocalPlayerPosition { get; }
    Vector3 LocalPlayerForward { get; }
    Team LocalTeam { get; }
    Vector3 BallPosition { get; }
    Vector3 CameraForward { get; }
    Vector3 RedGoalPosition { get; }
    Vector3 BlueGoalPosition { get; }
}
public interface ILineRenderer
{
    void Draw(Vector3 from, Vector3 to, LineStyle style, float width, float opacity);
    void Hide();
}
public interface IOverlayUi
{
    event Action ToggleRequested;
    event Action<ClientSettings> SettingsChanged;
    void SetVisible(bool visible);
    void ShowPlayerInfo(string text);
}
