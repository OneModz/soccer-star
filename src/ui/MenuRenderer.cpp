#include "MenuRenderer.h"

#include "imgui.h"

#include "../engine/IGameEngine.h"

namespace
{
const char* matchStateName(MatchState state)
{
    switch (state)
    {
        case MatchState::Idle:
            return "Idle";
        case MatchState::Searching:
            return "Searching";
        case MatchState::Playing:
            return "Playing";
        case MatchState::Finished:
            return "Finished";
    }

    return "Unknown";
}
}

void RenderModMenu(IGameEngine& engine)
{
    GameState state = engine.getGameState();

    ImGui::Begin("Soccer Stars Lab - Sandbox");

    ImGui::Text(
        "Engine: %s",
        engine.isRunning() ? "Running" : "Stopped"
    );

    ImGui::Text(
        "Match: %s",
        matchStateName(state.matchState)
    );

    ImGui::Separator();

    bool autoPlay = state.autoPlayEnabled;

    if (ImGui::Checkbox("Auto Play", &autoPlay))
    {
        engine.toggleAutoPlay(autoPlay);
    }

    int mode = static_cast<int>(state.autoPlayMode);

    const char* modes[] =
    {
        "Disabled",
        "Semi",
        "Full"
    };

    if (
        ImGui::Combo(
            "Mode",
            &mode,
            modes,
            IM_ARRAYSIZE(modes)
        )
    )
    {
        engine.setAutoPlayMode(
            static_cast<AutoPlayMode>(mode)
        );
    }

    float force = engine.getForce();

    if (
        ImGui::SliderFloat(
            "Force",
            &force,
            0.0f,
            100.0f
        )
    )
    {
        engine.setForce(force);
    }

    int interval = engine.getActionInterval();

    if (
        ImGui::SliderInt(
            "Action Interval",
            &interval,
            100,
            3000,
            "%d ms"
        )
    )
    {
        engine.setActionInterval(interval);
    }

    bool autoQueue = state.autoQueueEnabled;

    if (
        ImGui::Checkbox(
            "Auto Queue",
            &autoQueue
        )
    )
    {
        engine.toggleAutoQueue(autoQueue);
    }

    bool paused = state.paused;

    if (
        ImGui::Checkbox(
            "Pause Engine",
            &paused
        )
    )
    {
        engine.pause(paused);
    }

    ImGui::Separator();

    if (!engine.isRunning())
    {
        if (ImGui::Button("Start Engine"))
            engine.start();
    }
    else
    {
        if (ImGui::Button("Stop Engine"))
            engine.stop();
    }

    ImGui::Separator();

    if (ImGui::Button("Simulate Match Start"))
        engine.simulateMatchStart();

    ImGui::SameLine();

    if (ImGui::Button("Simulate Shot"))
        engine.simulateShot();

    if (ImGui::Button("Simulate Match End"))
        engine.simulateMatchEnd();

    ImGui::Separator();

    state = engine.getGameState();

    ImGui::Text(
        "Player Score: %d",
        state.simulatedScorePlayer
    );

    ImGui::Text(
        "Opponent Score: %d",
        state.simulatedScoreOpponent
    );

    ImGui::End();
}
