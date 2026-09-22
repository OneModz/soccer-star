#pragma once

#include "GameState.h"

class IGameEngine
{
public:
    virtual ~IGameEngine() = default;

    virtual void update() = 0;

    virtual void toggleAutoPlay(bool enabled) = 0;
    virtual void setAutoPlayMode(AutoPlayMode mode) = 0;

    virtual void setForce(float force) = 0;
    virtual float getForce() const = 0;

    virtual void setActionInterval(int milliseconds) = 0;
    virtual int getActionInterval() const = 0;

    virtual void toggleAutoQueue(bool enabled) = 0;

    virtual void start() = 0;
    virtual void stop() = 0;
    virtual void pause(bool paused) = 0;

    virtual bool isRunning() const = 0;

    virtual GameState getGameState() const = 0;

    virtual void simulateMatchStart() = 0;
    virtual void simulateShot() = 0;
    virtual void simulateMatchEnd() = 0;
};
