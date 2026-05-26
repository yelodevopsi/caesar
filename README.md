# caesar

A Caesar cipher CLI with support for Scandinavian characters (Æ, Ø, Å), implemented in six languages.

## Alphabet

Shifts operate on a 29-character alphabet:

```
A B C D E F G H I J K L M N O P Q R S T U V W X Y Z Æ Ø Å
```

Both upper and lowercase are supported. All other characters (numbers, punctuation, spaces) are passed through unchanged.

## Implementations

| Folder | Language | Run | Test |
|--------|----------|-----|------|
| `golang/` | Go 1.23 | `go run . <shift> <file>` | `go test ./...` |
| `dotnet/` | C# / .NET 8 | `dotnet run --project dotnet -- <shift> <file>` | `dotnet test dotnet.tests` |
| `winforms/` | C# / .NET 8 WinForms | `dotnet run --project winforms` | — |
| `java/` | Java 21 | `java -cp java/out Caesar <shift> <file>` | `java -cp java/out CaesarTest` |
| `nodejs/` | Node.js | `node nodejs/caesar.mjs <shift> <file>` | `node --test nodejs/caesar.test.mjs` |
| `bun/` | Bun + TypeScript | `bun bun/caesar.ts <shift> <file>` | `bun test bun/` |
| `python/` | Python 3.12 | `python python/caesar.py <shift> <file>` | `python -m unittest test_caesar -v` |

## Usage

All implementations share the same interface:

```
caesar <shift> <file> [-d|--decrypt] [-o|--output <file>]
```

| Argument | Description |
|---|---|
| `shift` | Number of positions to shift (integer) |
| `file` | Path to the input `.txt` file |
| `-d`, `--decrypt` | Decrypt instead of encrypt |
| `-o`, `--output <file>` | Write output to a file instead of stdout |

## Examples

**Encrypt** `plain.txt` with a shift of 5, write to `secret.txt`:
```sh
caesar 5 plain.txt -o secret.txt
```

**Decrypt** `secret.txt` back, print to stdout:
```sh
caesar 5 secret.txt -d
```

**Decrypt** to a file:
```sh
caesar 5 secret.txt -d -o plain_out.txt
```

## WinForms GUI (`winforms/`)

A Windows desktop application built on top of the `dotnet/` Caesar cipher library.

**Run:**
```sh
dotnet run --project winforms
```

**Or build a self-contained executable first:**
```sh
dotnet publish winforms -c Release -r win-x64 --self-contained
```
The binary is written to `winforms/bin/Release/net8.0-windows/win-x64/publish/`.

**Features:**

- Drag and drop a `.txt` file anywhere onto the window to load it
- Side-by-side **Input** / **Output** panels with scroll
- **Shift** spinner (1–28) — output updates live as you turn it
- **Encrypt / Decrypt** radio buttons — output updates live on toggle
- **Save Output…** button opens a save dialog and writes UTF-8

> Requires Windows. The 29-character alphabet (A–Z + Æ Ø Å) is noted in the info strip at the top of the window.

## Benchmarking

Four shared input files are included for benchmarking:

| File | Words | Chars |
|------|-------|-------|
| `bench_100.txt` | 100 | ~416 |
| `bench_5000.txt` | 5 000 | ~21 000 |
| `bench_100000.txt` | 100 000 | ~420 000 |
| `bench_1000000.txt` | 1 000 000 | ~4 200 000 |

Run the benchmark across all implementations:

```sh
python benchmark.py
```

This runs each implementation 5 times per file and reports average **characters per second** (including process startup time).

To regenerate the input files with a new random seed, edit and run:

```sh
python generate_bench.py
```
