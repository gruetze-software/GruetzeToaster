#!/usr/bin/env bash
set -euo pipefail
ROOT="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")/.." && pwd)"
case "${1:-}" in
  linux) exec bash "$ROOT/scripts/build-linux.sh" "${2:-x64}" ;;
  macos) exec bash "$ROOT/scripts/build-macos.sh" "${2:-arm64}" ;;
  *) echo "Usage: $0 linux [x64|arm64] | macos [x64|arm64]" >&2; exit 2 ;;
esac
