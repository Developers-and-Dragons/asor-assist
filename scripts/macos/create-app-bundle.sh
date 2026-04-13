#!/usr/bin/env bash
# Assemble a .app bundle from a dotnet publish output directory (osx-* RID).
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"

usage() {
  echo "Usage: $0 <publish-directory> <OutputName.app>" >&2
  exit 1
}

[[ $# -eq 2 ]] || usage
PUBLISH_DIR="$(cd "$1" && pwd)"
OUT_PATH="$2"

case "$OUT_PATH" in
  *.app) ;;
  *) echo "Error: output must end with .app (got: $OUT_PATH)" >&2; exit 1 ;;
esac

if [[ ! -d "$PUBLISH_DIR" ]]; then
  echo "Error: publish directory not found: $PUBLISH_DIR" >&2
  exit 1
fi

MAIN_EXE="$PUBLISH_DIR/AsorAssistant.App"
if [[ ! -f "$MAIN_EXE" ]]; then
  echo "Error: expected main executable not found: $MAIN_EXE" >&2
  exit 1
fi

ICNS_SRC="$REPO_ROOT/src/AsorAssistant.App/Assets/app-icon.icns"
if [[ ! -f "$ICNS_SRC" ]]; then
  echo "Error: macOS icon not found: $ICNS_SRC" >&2
  exit 1
fi

PLIST_SRC="$REPO_ROOT/packaging/macos/Info.plist"
if [[ ! -f "$PLIST_SRC" ]]; then
  echo "Error: Info.plist not found: $PLIST_SRC" >&2
  exit 1
fi

OUT_DIR="$(dirname "$OUT_PATH")"
mkdir -p "$OUT_DIR"
OUT_ABS="$(cd "$OUT_DIR" && pwd)/$(basename "$OUT_PATH")"
rm -rf "$OUT_ABS"
mkdir -p "$OUT_ABS/Contents/MacOS" "$OUT_ABS/Contents/Resources"

cp -R "$PUBLISH_DIR/"* "$OUT_ABS/Contents/MacOS/"
cp "$ICNS_SRC" "$OUT_ABS/Contents/Resources/"
cp "$PLIST_SRC" "$OUT_ABS/Contents/Info.plist"

chmod +x "$OUT_ABS/Contents/MacOS/AsorAssistant.App"

# `dotnet publish` can leave many managed assemblies marked executable. On macOS this
# can cause Gatekeeper/codesign to treat them as nested code, breaking launch/signing.
find "$OUT_ABS/Contents/MacOS" -type f \( -name "*.dll" -o -name "*.json" -o -name "*.pdb" -o -name "*.dat" -o -name "*.txt" \) -exec chmod a-x {} \; 2>/dev/null || true

echo "Created: $OUT_ABS"
