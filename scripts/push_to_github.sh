#!/data/data/com.termux/files/usr/bin/bash
set -e

REPO_URL="https://github.com/OneModz/soccer-star.git"
WORKDIR="$HOME/soccer-star"
SOURCE_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

echo "== Soccer Stars Lab: GitHub publish =="

if [ -d "$WORKDIR/.git" ]; then
    echo "Repository already exists at $WORKDIR"
    cd "$WORKDIR"
    git pull --ff-only origin main
else
    rm -rf "$WORKDIR"
    git clone "$REPO_URL" "$WORKDIR"
fi


echo "Copying sandbox files into repository..."

cp "$SOURCE_DIR/CMakeLists.txt" "$WORKDIR/CMakeLists.txt"
cp "$SOURCE_DIR/README.md" "$WORKDIR/README.md"
cp "$SOURCE_DIR/.gitignore" "$WORKDIR/.gitignore"

mkdir -p "$WORKDIR/src/engine"
mkdir -p "$WORKDIR/src/ui"
mkdir -p "$WORKDIR/scripts"

cp "$SOURCE_DIR/src/main.cpp" "$WORKDIR/src/main.cpp"
cp "$SOURCE_DIR/src/engine/"* "$WORKDIR/src/engine/"
cp "$SOURCE_DIR/src/ui/"* "$WORKDIR/src/ui/"
cp "$SOURCE_DIR/scripts/bootstrap_termux.sh" "$WORKDIR/scripts/bootstrap_termux.sh"
cp "$SOURCE_DIR/scripts/build.sh" "$WORKDIR/scripts/build.sh"
cp "$SOURCE_DIR/scripts/push_to_github.sh" "$WORKDIR/scripts/push_to_github.sh"

cd "$WORKDIR"

git add CMakeLists.txt README.md .gitignore src scripts

if git diff --cached --quiet; then
    echo "No changes to commit."
    exit 0
fi

git commit -m "feat: add standalone engine sandbox"
git push origin main

echo
echo "Published successfully:"
echo "https://github.com/OneModz/soccer-star"
