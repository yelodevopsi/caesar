import argparse
import sys

upper = list('ABCDEFGHIJKLMNOPQRSTUVWXYZÆØÅ')
lower = list('abcdefghijklmnopqrstuvwxyzæøå')
size  = len(upper)

# ~i encodes lowercase: ~0=-1, ~1=-2, ... mirrors the Go/JS implementations
idx: dict[str, int] = {ch: i for i, ch in enumerate(upper)}
idx.update({ch: ~i for i, ch in enumerate(lower)})


def cipher(text: str, shift: int) -> str:
    shift = (shift % size + size) % size
    out = []
    for ch in text:
        pos = idx.get(ch)
        if pos is None:
            out.append(ch)
        elif pos >= 0:
            out.append(upper[(pos + shift) % size])
        else:
            out.append(lower[(~pos + shift) % size])
    return ''.join(out)


def main() -> None:
    parser = argparse.ArgumentParser(prog='caesar')
    parser.add_argument('shift', type=int)
    parser.add_argument('file')
    parser.add_argument('-d', '--decrypt', action='store_true')
    parser.add_argument('-o', '--output')
    args = parser.parse_args()

    try:
        text = open(args.file, encoding='utf-8').read()
    except OSError as e:
        print(f'could not read file: {e}', file=sys.stderr)
        sys.exit(1)

    result = cipher(text, -args.shift if args.decrypt else args.shift)

    if args.output:
        try:
            with open(args.output, 'w', encoding='utf-8') as f:
                f.write(result)
        except OSError as e:
            print(f'could not write file: {e}', file=sys.stderr)
            sys.exit(1)
    else:
        sys.stdout.write(result)


if __name__ == '__main__':
    main()
