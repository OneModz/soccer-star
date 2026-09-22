#!/data/data/com.termux/files/usr/bin/bash
set -e

pkg update
pkg install -y git clang cmake ninja pkg-config
pkg install -y x11-repo
pkg update
pkg install -y sdl2

echo
echo "Dependencies installed."
echo "Build with:"
echo "  cmake -S . -B build -G Ninja -DCMAKE_BUILD_TYPE=Debug"
echo "  cmake --build build -j4"
