using System;
using SoccerStar.Shared;

namespace SoccerStar.Server;

public sealed class ServerMessageRouter
{
    private readonly IWorldPhysics _physics;
    private readonly IServerTransport _transport;
    private readonly AntiCheatService _antiCheat;
    private readonly ActionValidator _validator;
    private readonly ConfigService _config;
    private readonly IPermissionService _permissions;

    public ServerMessageRouter(IWorldPhysics physics, IServerTransport transport, AntiCheatService antiCheat,
        ActionValidator validator, ConfigService config, IPermissionService permissions)
    {
        _physics = physics;
        _transport = transport;
        _antiCheat = antiCheat;
        _validator = validator;
        _config = config;
        _permissions = permissions;
    }

    public void Handle(ServerPlayer player, IClientMessage message)
    {
        if (!_antiCheat.ValidateSequence(player, message)) return;

        switch (message)
        {
            case ShotRequest shot:
                HandleShot(player, shot);
                break;
            case ActionRequest action:
                HandleAction(player, action);
                break;
            case ConfigRequest cfg:
                if (!_config.TryApply(player, cfg.Settings))
                    _transport.Send(player.PlayerId, new ServerNotice("invalid_config", "Configuração rejeitada pelo servidor."));
                break;
            case ModerationHighlightRequest mod:
                HandleModeration(player, mod);
                break;
        }
    }

    private void HandleShot(ServerPlayer player, ShotRequest req)
    {
        if (player.MatchId is null) return;
        var ball = _physics.GetBall(player.MatchId);
        if (!_validator.ValidateShot(player, ball, req, out var dir, out var strength))
        {
            _antiCheat.Report(player.PlayerId, severe: false);
            return;
        }
        _physics.ApplyBallImpulse(player.MatchId, dir * (strength * GameConfig.MaxKickImpulse));
    }

    private void HandleAction(ServerPlayer player, ActionRequest req)
    {
        if (player.MatchId is null || !_validator.ValidateAction(player, req)) return;
        var dir = MathGuard.SafeNormalize(req.Direction);
        _physics.ApplyPlayerAction(player.MatchId, player.PlayerId, req.Action, dir);
    }

    private void HandleModeration(ServerPlayer admin, ModerationHighlightRequest req)
    {
        if (!_permissions.IsModerator(admin.PlayerId))
        {
            _antiCheat.Report(admin.PlayerId, severe: true);
            return;
        }
        // Destaque é enviado apenas ao cliente do moderador, não altera a simulação.
        _transport.Send(admin.PlayerId, new HighlightInstruction(req.TargetPlayerId, req.Enabled));
    }
}
