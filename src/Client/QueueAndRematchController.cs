using SoccerStar.Shared;

namespace SoccerStar.Client;

public sealed class QueueAndRematchController
{
    private readonly IClientTransport _net;
    private readonly IClientClock _clock;
    private readonly InputController _sequence;
    public ClientSettings Settings { get; set; } = Defaults.Settings;

    public QueueAndRematchController(IClientTransport net, IClientClock clock, InputController sequence)
    {
        _net = net; _clock = clock; _sequence = sequence;
    }

    public void TryAutoQueue()
    {
        var a = Settings.AutoPlay;
        if (!a.AutoQueue) return;
        _net.Send(new QueueRequest(_sequence.NextSequence(), _clock.NowSeconds, a.TableFilter, a.FixedTableId, a.CoinsToUse));
    }

    public void OnMatchFinished(MatchFinishedNotice notice)
    {
        if (Settings.AutoPlay.AutoRematch && notice.CanRematch)
            _net.Send(new RematchRequest(_sequence.NextSequence(), _clock.NowSeconds, notice.MatchId));
    }
}
