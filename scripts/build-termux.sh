#!/data/data/com.termux/files/usr/bin/bash
set -euo pipefail

cd "$(dirname "$0")/.."

echo "== Soccer Star / Termux build =="
echo "Architecture: $(dpkg --print-architecture 2>/dev/null || uname -m)"

if ! command -v dotnet >/dev/null 2>&1; then
  echo "dotnet não encontrado. Instale com:"
  echo "  pkg update && pkg install -y dotnet-sdk-8.0 git"
  exit 1
fi

dotnet --version
dotnet restore SoccerStar.Core.csproj
dotnet build SoccerStar.Core.csproj -c Release --no-restore

echo
echo "Build concluído. Saída:"
echo "  $(pwd)/bin/Release/net8.0/"
