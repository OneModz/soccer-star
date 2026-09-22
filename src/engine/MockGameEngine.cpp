#include "MockGameEngine.h"

#include <algorithm>
#include <iostream>

MockGameEngine::MockGameEngine()
{
    lastAction_ = std::chrono::steady_clock::now();
    log("MockGameEngine initialized.");
}

void MockGameEngine::update()
{
    if (!running_ || state_.paused || !state_.autoPlayEnabled)
        return;

    if (state_.matchState != MatchState::Playing)
        return;

    const auto now = std::chrono::steady_clock::now();

    const auto elapsed =
        std::chrono::duration_cast<std::chrono::milliseconds>(
            now - lastAction_
        );

    if (elapsed.count() >= state_.actionIntervalMs)
    {
        processAutoPlay();
        lastAction_ = now;
    }
}

void MockGameEngine::toggleAutoPlay(bool enabled)
{
    state_.autoPlayEnabled = enabled;

    std::cout
        << "[MockEngine] AutoPlay: "
        << (enabled ? "ON" : "OFF")
        << '\n';
}

void MockGameEngine::setAutoPlayMode(AutoPlayMode mode)
{
    state_.autoPlayMode = mode;

    std::cout
        << "[MockEngine] AutoPlay mode: "
        << static_cast<int>(mode)
        << '\n';
}

void MockGameEngine::setForce(float force)
{
    state_.force = std::clamp(force, 0.0f, 100.0f);

    std::cout
        << "[MockEngine] Force: "
        << state_.force
        << '\n';
}

float MockGameEngine::getForce() const
{
    return state_.force;
}

void MockGameEngine::setActionInterval(int milliseconds)
{
    state_.actionIntervalMs =
        std::clamp(milliseconds, 100, 5000);

    std::cout
        << "[MockEngine] Action interval: "
        << state_.actionIntervalMs
        << " ms\n";
}

int MockGameEngine::getActionInterval() const
{
    return state_.actionIntervalMs;
}

void MockGameEngine::toggleAutoQueue(bool enabled)
{
    state_.autoQueueEnabled = enabled;

    std::cout
        << "[MockEngine] AutoQueue: "
        << (enabled ? "ON" : "OFF")
        << '\n';
}

void MockGameEngine::start()
{
    if (running_)
        return;

    running_ = true;
    lastAction_ = std::chrono::steady_clock::now();

    log("Engine started.");
}

void MockGameEngine::stop()
{
    if (!running_)
        return;

    running_ = false;
    log("Engine stopped.");
}

void MockGameEngine::pause(bool paused)
{
    state_.paused = paused;

    std::cout
        << "[MockEngine] Pause: "
        << (paused ? "YES" : "NO")
        << '\n';
}

bool MockGameEngine::isRunning() const
{
    return running_;
}

GameState MockGameEngine::getGameState() const
{
    return state_;
}

void MockGameEngine::simulateMatchStart()
{
    state_.matchState = MatchState::Playing;
    state_.simulatedScorePlayer = 0;
    state_.simulatedScoreOpponent = 0;

    log("Simulated match started.");
}

void MockGameEngine::simulateShot()
{
    if (state_.matchState != MatchState::Playing)
    {
        log("Shot ignored: no active simulated match.");
        return;
    }

    std::cout
        << "[MockEngine] Simulated shot | force="
        << state_.force
        << '\n';

    ++state_.simulatedScorePlayer;
}

void MockGameEngine::simulateMatchEnd()
{
    if (state_.matchState != MatchState::Playing)
        return;

    state_.matchState = MatchState::Finished;

    log("Simulated match ended.");
}

void MockGameEngine::processAutoPlay()
{
    switch (state_.autoPlayMode)
    {
        case AutoPlayMode::Disabled:
            return;

        case AutoPlayMode::Semi:
            log("Semi Auto: evaluating simulated shot.");
            break;

        case AutoPlayMode::Full:
            log("Full Auto: performing simulated action.");
            simulateShot();
            break;
    }
}

void MockGameEngine::log(const char* message) const
{
    std::cout
        << "[MockEngine] "
        << message
        << '\n';
}
