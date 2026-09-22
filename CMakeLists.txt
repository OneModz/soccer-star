cmake_minimum_required(VERSION 3.20)

project(
    SoccerStarsLab
    VERSION 0.2.0
    LANGUAGES CXX
)

set(CMAKE_CXX_STANDARD 20)
set(CMAKE_CXX_STANDARD_REQUIRED ON)
set(CMAKE_CXX_EXTENSIONS OFF)

include(FetchContent)

find_package(PkgConfig REQUIRED)
pkg_check_modules(SDL2 REQUIRED sdl2)

FetchContent_Declare(
    imgui
    GIT_REPOSITORY https://github.com/ocornut/imgui.git
    GIT_TAG v1.92.2b
)

FetchContent_GetProperties(imgui)

if(NOT imgui_POPULATED)
    FetchContent_Populate(imgui)
endif()

add_executable(
    soccer_stars_lab

    src/main.cpp

    src/engine/MockGameEngine.cpp
    src/ui/MenuRenderer.cpp

    ${imgui_SOURCE_DIR}/imgui.cpp
    ${imgui_SOURCE_DIR}/imgui_draw.cpp
    ${imgui_SOURCE_DIR}/imgui_tables.cpp
    ${imgui_SOURCE_DIR}/imgui_widgets.cpp

    ${imgui_SOURCE_DIR}/backends/imgui_impl_sdl2.cpp
    ${imgui_SOURCE_DIR}/backends/imgui_impl_sdlrenderer2.cpp
)

target_include_directories(
    soccer_stars_lab
    PRIVATE
    src
    src/engine
    src/ui

    ${imgui_SOURCE_DIR}
    ${imgui_SOURCE_DIR}/backends

    ${SDL2_INCLUDE_DIRS}
)

target_link_directories(
    soccer_stars_lab
    PRIVATE
    ${SDL2_LIBRARY_DIRS}
)

target_link_libraries(
    soccer_stars_lab
    PRIVATE
    ${SDL2_LIBRARIES}
)

target_compile_options(
    soccer_stars_lab
    PRIVATE
    -Wall
    -Wextra
    -Wpedantic
)

if(CMAKE_BUILD_TYPE STREQUAL "Release")
    target_compile_options(
        soccer_stars_lab
        PRIVATE
        -O2
    )
endif()
