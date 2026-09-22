#!/data/data/com.termux/files/usr/bin/bash
set -e

BUILD_TYPE="${1:-Debug}"

cmake \
    -S . \
    -B build \
    -G Ninja \
    -DCMAKE_BUILD_TYPE="${BUILD_TYPE}"

cmake --build build -j4

echo
echo "Build complete:"
echo "  ./build/soccer_stars_lab"
