#include <SDL.h>

#include "imgui.h"
#include "imgui_impl_sdl2.h"
#include "imgui_impl_sdlrenderer2.h"

#include "engine/MockGameEngine.h"
#include "ui/MenuRenderer.h"

#include <iostream>

int main(int argc, char** argv)
{
    (void)argc;
    (void)argv;

    if (SDL_Init(SDL_INIT_VIDEO | SDL_INIT_TIMER) != 0)
    {
        std::cerr
            << "SDL_Init failed: "
            << SDL_GetError()
            << '\n';

        return 1;
    }

    SDL_Window* window = SDL_CreateWindow(
        "Soccer Stars Lab",
        SDL_WINDOWPOS_CENTERED,
        SDL_WINDOWPOS_CENTERED,
        900,
        650,
        SDL_WINDOW_RESIZABLE | SDL_WINDOW_ALLOW_HIGHDPI
    );

    if (!window)
    {
        std::cerr
            << "SDL_CreateWindow failed: "
            << SDL_GetError()
            << '\n';

        SDL_Quit();
        return 1;
    }

    SDL_Renderer* renderer = SDL_CreateRenderer(
        window,
        -1,
        SDL_RENDERER_ACCELERATED |
        SDL_RENDERER_PRESENTVSYNC
    );

    if (!renderer)
    {
        std::cerr
            << "SDL_CreateRenderer failed: "
            << SDL_GetError()
            << '\n';

        SDL_DestroyWindow(window);
        SDL_Quit();

        return 1;
    }

    IMGUI_CHECKVERSION();
    ImGui::CreateContext();

    ImGuiIO& io = ImGui::GetIO();
    io.ConfigFlags |= ImGuiConfigFlags_NavEnableKeyboard;

    ImGui::StyleColorsDark();

    if (!ImGui_ImplSDL2_InitForSDLRenderer(window, renderer))
    {
        std::cerr << "ImGui SDL2 backend initialization failed.\n";
        ImGui::DestroyContext();
        SDL_DestroyRenderer(renderer);
        SDL_DestroyWindow(window);
        SDL_Quit();
        return 1;
    }

    if (!ImGui_ImplSDLRenderer2_Init(renderer))
    {
        std::cerr << "ImGui SDL_Renderer2 backend initialization failed.\n";
        ImGui_ImplSDL2_Shutdown();
        ImGui::DestroyContext();
        SDL_DestroyRenderer(renderer);
        SDL_DestroyWindow(window);
        SDL_Quit();
        return 1;
    }

    MockGameEngine engine;
    engine.start();

    bool running = true;

    while (running)
    {
        SDL_Event event;

        while (SDL_PollEvent(&event))
        {
            ImGui_ImplSDL2_ProcessEvent(&event);

            if (event.type == SDL_QUIT)
                running = false;

            if (
                event.type == SDL_WINDOWEVENT &&
                event.window.event == SDL_WINDOWEVENT_CLOSE
            )
            {
                running = false;
            }
        }

        engine.update();

        ImGui_ImplSDLRenderer2_NewFrame();
        ImGui_ImplSDL2_NewFrame();

        ImGui::NewFrame();

        RenderModMenu(engine);

        ImGui::Render();

        SDL_SetRenderDrawColor(
            renderer,
            18,
            18,
            18,
            255
        );

        SDL_RenderClear(renderer);

        ImGui_ImplSDLRenderer2_RenderDrawData(
            ImGui::GetDrawData(),
            renderer
        );

        SDL_RenderPresent(renderer);
    }

    engine.stop();

    ImGui_ImplSDLRenderer2_Shutdown();
    ImGui_ImplSDL2_Shutdown();

    ImGui::DestroyContext();

    SDL_DestroyRenderer(renderer);
    SDL_DestroyWindow(window);

    SDL_Quit();

    return 0;
}
