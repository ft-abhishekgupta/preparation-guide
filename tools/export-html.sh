#!/usr/bin/env bash
# Regenerates every HTML note from its Markdown source and refreshes index.html.
#
# Usage:
#   ./tools/export-html.sh [--filter <text>] [--skip-index] [--index-only]

set -euo pipefail

TOOLS_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

if ! command -v node >/dev/null 2>&1; then
  echo "Node.js is required but was not found on PATH. Install it from https://nodejs.org/." >&2
  exit 1
fi

cd "$TOOLS_DIR"

if [ ! -d node_modules ]; then
  echo "Installing export toolchain (first run only)..."
  npm install --no-audit --no-fund
fi

node export-html.cjs "$@"
