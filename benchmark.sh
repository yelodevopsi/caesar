#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$ROOT"

SHIFT=5
RUNS=5
FILES=("bench_100.txt" "bench_5000.txt" "bench_100000.txt")
LANGS=("go" "nodejs" "bun" "python")

# ── Find Go (handles WSL paths with spaces and Git Bash) ──────────────────────
GO_CMD=$(command -v go 2>/dev/null || command -v go.exe 2>/dev/null || true)
if [ -z "$GO_CMD" ]; then
  for candidate in \
    "/mnt/c/Program Files/Go/bin/go" \
    "/c/Program Files/Go/bin/go" \
    "/usr/local/go/bin/go"; do
    if [ -x "$candidate" ] || [ -x "${candidate}.exe" ]; then
      GO_CMD="$candidate"; break
    fi
  done
fi

# ── Build Go binary ───────────────────────────────────────────────────────────
GO_BIN="./golang/caesar"
if [ -n "$GO_CMD" ]; then
  printf "Building Go binary...\n"
  "$GO_CMD" build -o golang/caesar ./golang/
elif [ ! -f "$GO_BIN" ] && [ ! -f "${GO_BIN}.exe" ]; then
  printf "Error: 'go' not found. Build manually first:\n  go build -o golang/caesar golang/\n" >&2
  exit 1
fi
[ -f "${GO_BIN}.exe" ] && GO_BIN="${GO_BIN}.exe"

# ── Helpers ───────────────────────────────────────────────────────────────────

char_count() {
  python -c "print(len(open('$1', encoding='utf-8').read()))"
}

# avg_time <lang> <file> — prints average elapsed seconds over $RUNS runs
avg_time() {
  local lang="$1" file="$2" sum=0 t
  TIMEFORMAT='%R'
  for ((i = 0; i < RUNS; i++)); do
    case "$lang" in
      go)     t=$( { time "$GO_BIN"              "$SHIFT" "$file" > /dev/null; } 2>&1 ) ;;
      nodejs) t=$( { time node ./nodejs/caesar.mjs     "$SHIFT" "$file" > /dev/null; } 2>&1 ) ;;
      bun)    t=$( { time bun  run ./bun/caesar.ts     "$SHIFT" "$file" > /dev/null; } 2>&1 ) ;;
      python) t=$( { time python ./python/caesar.py    "$SHIFT" "$file" > /dev/null; } 2>&1 ) ;;
    esac
    sum=$(awk "BEGIN { print $sum + $t }")
  done
  awk "BEGIN { printf \"%.4f\", $sum / $RUNS }"
}

fmt_cps() {
  # Format integer with thousands separator where supported
  printf "%'d" "$1" 2>/dev/null || printf "%d" "$1"
}

# ── Pre-compute char counts ────────────────────────────────────────────────────
declare -A CHARS
for f in "${FILES[@]}"; do
  CHARS["$f"]=$(char_count "$f")
done

# ── Table header ──────────────────────────────────────────────────────────────
printf "\nCaesar cipher benchmark — avg of %d runs, shift=%d\n\n" "$RUNS" "$SHIFT"

COL_LANG=10
COL_DATA=22
printf "%-${COL_LANG}s" "Language"
for f in "${FILES[@]}"; do
  printf " | %-${COL_DATA}s" "$f"
done
printf "\n"

printf "%${COL_LANG}s" | tr ' ' '-'
for f in "${FILES[@]}"; do
  printf "-|-"
  printf "%${COL_DATA}s" | tr ' ' '-'
done
printf "\n"

# ── Benchmark runs ────────────────────────────────────────────────────────────
for lang in "${LANGS[@]}"; do
  printf "%-${COL_LANG}s" "$lang"
  for file in "${FILES[@]}"; do
    chars="${CHARS[$file]}"
    avg=$(avg_time "$lang" "$file")
    cps=$(awk "BEGIN { printf \"%d\", $chars / $avg }")
    ms=$(awk  "BEGIN { printf \"%.1f\", $avg * 1000 }")
    cell="$(fmt_cps "$cps") c/s (${ms}ms)"
    printf " | %-${COL_DATA}s" "$cell"
  done
  printf "\n"
done

printf "\n(startup time included — dominates at small file sizes)\n\n"
