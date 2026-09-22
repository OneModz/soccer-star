#pragma once

#include "IGameEngine.h"

#include <chrono>

class MockGameEngine final : public IGameEngine
{
public:
    MockGameEngine();

    void update() override;

    void toggleAutoPlay(bool enabled) override;
    void setAutoPlayMode(AutoPlayMode mode) override;

    void setForce(float force) override;
    float getForce() const override;

    void setActionInterval(int milliseconds) override;
    int getActionInterval() const override;

    void toggleAutoQueue(bool enabled) override;

    void start() override;
    void stop() override;
    void pause(bool paused) override;

    bool isRunning() const override;

    GameState getGameState() const override;

    void simulateMatchStart() override;
    void simulateShot() override;
    void simulateMatchEnd() override;

private:
    void log(const char* message) const;
    void processAutoPlay();

    GameState state_;
    bool running_ = false;

    std::chrono::steady_clock::time_point lastAction_;
};
