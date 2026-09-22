# Soccer Stars Lab

Standalone C++ sandbox used to develop and test an ImGui-based control panel
without depending on any external game process.

The project separates the UI from the simulated engine through the
`IGameEngine` interface.

## Architecture

```text
MenuRenderer
     |
     v
IGameEngine
     ^
     |
MockGameEngine
```

The UI knows only the interface. `MockGameEngine` provides a safe standalone
simulation that writes actions to the console.

## Features

- Dear ImGui UI
- SDL2 renderer
- CMake build
- Abstract `IGameEngine` contract
- `MockGameEngine` test implementation
- Auto Play state simulation
- Semi / Full modes
- Force control
- Action interval
- Auto Queue state
- Match start / shot / end simulation
- Console logs
- Linux / Android-Termux oriented C++ code

## Termux dependencies

```bash
pkg update
pkg upgrade

pkg install git clang cmake ninja pkg-config
pkg install x11-repo
pkg update
pkg install sdl2
```

A graphical X11 environment is required to display the SDL2 window from Termux.

## Build

```bash
cmake -S . -B build -G Ninja -DCMAKE_BUILD_TYPE=Debug
cmake --build build -j4
```

Run:

```bash
./build/soccer_stars_lab
```

## Release build

```bash
cmake -S . -B build-release -G Ninja -DCMAKE_BUILD_TYPE=Release
cmake --build build-release -j4
```

## GitHub

After extracting this repository:

```bash
git init
git add .
git commit -m "feat: initial standalone engine sandbox"
git branch -M main
git remote add origin https://github.com/SEU-USUARIO/SEU-REPOSITORIO.git
git push -u origin main
```

## Project layout

```text
soccer-stars-lab/
├── CMakeLists.txt
├── README.md
├── .gitignore
└── src/
    ├── main.cpp
    ├── engine/
    │   ├── GameState.h
    │   ├── IGameEngine.h
    │   ├── MockGameEngine.h
    │   └── MockGameEngine.cpp
    └── ui/
        ├── MenuRenderer.h
        └── MenuRenderer.cpp
```

## Scope

This repository is a standalone simulation/test harness. It does not inject
code into, read memory from, or modify another application.

## Publish to OneModz/soccer-star

From the extracted project directory in Termux:

```bash
chmod +x scripts/push_to_github.sh
./scripts/push_to_github.sh
```

The script clones or updates `https://github.com/OneModz/soccer-star`,
copies the sandbox files into the repository, creates a commit, and pushes
to the `main` branch. GitHub authentication must already be configured in
Termux.
