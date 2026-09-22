#pragma once

enum class AutoPlayMode
{
    Disabled = 0,
    Semi,
    Full
};

enum class MatchState
{
    Idle = 0,
    Searching,
    Playing,
    Finished
};

struct GameState
{
    bool autoPlayEnabled = false;
    bool autoQueueEnabled = false;

    AutoPlayMode autoPlayMode = AutoPlayMode::Disabled;
    MatchState matchState = MatchState::Idle;

    float force = 50.0f;
    int actionIntervalMs = 1000;

    bool paused = false;

    int simulatedScorePlayer = 0;
    int simulatedScoreOpponent = 0;
};
