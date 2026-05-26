#!/usr/bin/env python3
"""
Caesar cipher benchmark — runs each implementation RUNS times per input file
and reports average characters per second (startup time included).

Usage: python benchmark.py
"""
import os
import platform
import shutil
import subprocess
import sys
import time
from pathlib import Path

ROOT = Path(__file__).parent
os.chdir(ROOT)

SHIFT = "5"
RUNS  = 5
FILES = ["bench_100.txt", "bench_5000.txt", "bench_100000.txt", "bench_1000000.txt"]
DOTNET_BIN = ROOT / "dotnet" / "publish" / ("dotnet.exe" if platform.system() == "Windows" else "dotnet")

# ── Build Go binary ────────────────────────────────────────────────────────────
go_cmd = shutil.which("go")
go_bin = ROOT / "golang" / ("caesar.exe" if platform.system() == "Windows" else "caesar")

if go_cmd:
    print("Building Go binary...")
    subprocess.run([go_cmd, "build", "-o", str(go_bin), "."],
                   check=True, cwd=ROOT / "golang")
elif not go_bin.exists():
    sys.exit(f"Error: 'go' not found. Build manually:\n  go build -o {go_bin} ./golang/")

# ── Build .NET Release binary ─────────────────────────────────────────────────
dotnet_cmd = shutil.which("dotnet")
if dotnet_cmd:
    print("Publishing .NET binary...")
    subprocess.run(
        [dotnet_cmd, "publish", "dotnet", "-c", "Release", "-o", "dotnet/publish", "--nologo"],
        check=True, stdout=subprocess.DEVNULL,
    )
elif not DOTNET_BIN.exists():
    sys.exit("Error: 'dotnet' not found. Run: dotnet publish dotnet -c Release -o dotnet/publish")

# ── Commands (shift and file appended at runtime) ─────────────────────────────
def find(name: str) -> str:
    exe = shutil.which(name)
    if exe is None:
        sys.exit(f"Error: '{name}' not found in PATH.")
    return exe

CMDS: dict[str, list[str]] = {
    "go":     [str(go_bin)],
    "dotnet": [str(DOTNET_BIN)],
    "nodejs": [find("node"), "nodejs/caesar.mjs"],
    "bun":    [find("bun"),  "bun/caesar.ts"],
    "python": [sys.executable, "python/caesar.py"],
}

# ── Helpers ───────────────────────────────────────────────────────────────────
def avg_seconds(cmd: list[str], file: str) -> float:
    full = cmd + [SHIFT, file]
    total = 0.0
    for _ in range(RUNS):
        t = time.perf_counter()
        subprocess.run(full, stdout=subprocess.DEVNULL, check=True)
        total += time.perf_counter() - t
    return total / RUNS

def char_count(file: str) -> int:
    return len(Path(file).read_text(encoding="utf-8"))

# ── Pre-compute ───────────────────────────────────────────────────────────────
chars = {f: char_count(f) for f in FILES}

# ── Table ─────────────────────────────────────────────────────────────────────
C0, C1 = 10, 24   # column widths: language, data cell

print(f"\nCaesar cipher benchmark  ·  {RUNS} runs avg  ·  shift={SHIFT}\n")

header = f"{'Language':<{C0}}" + "".join(f"  {f:<{C1}}" for f in FILES)
print(header)
print("-" * len(header))

for lang, cmd in CMDS.items():
    row = f"{lang:<{C0}}"
    for file in FILES:
        avg = avg_seconds(cmd, file)
        cps = int(chars[file] / avg)
        cell = f"{cps:>12,} c/s  ({avg*1000:.0f}ms)"
        row += f"  {cell:<{C1}}"
    print(row)

print(f"\n* startup time included — dominates bench_100.txt results\n")
