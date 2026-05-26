# caesar

A Caesar cipher CLI with support for Scandinavian characters (Æ, Ø, Å), implemented in four languages.

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
| `nodejs/` | Node.js | `node caesar.mjs <shift> <file>` | `node --test caesar.test.mjs` |
| `bun/` | Bun + TypeScript | `bun caesar.ts <shift> <file>` | `bun test` |
| `python/` | Python 3.12 | `python caesar.py <shift> <file>` | `python -m unittest test_caesar -v` |

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

## Benchmarking

Three shared input files are included for benchmarking:

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
