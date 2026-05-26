const upper = [...'ABCDEFGHIJKLMNOPQRSTUVWXYZÆØÅ'];
const lower = [...'abcdefghijklmnopqrstuvwxyzæøå'];
const size  = upper.length;

const idx = new Map<string, number>([
  ...upper.map((ch, i): [string, number] => [ch, i]),
  ...lower.map((ch, i): [string, number] => [ch, ~i]),
]);

export function cipher(text: string, shift: number): string {
  shift = ((shift % size) + size) % size;
  return [...text].map(ch => {
    const pos = idx.get(ch);
    if (pos === undefined) return ch;
    return pos >= 0
      ? upper[(pos + shift) % size]
      : lower[(~pos + shift) % size];
  }).join('');
}

if (import.meta.main) {
  const args = Bun.argv.slice(2);
  if (args.length < 2) {
    process.stderr.write('usage: caesar <shift> <file> [-d|--decrypt] [-o|--output <file>]\n');
    process.exit(1);
  }

  const shift = parseInt(args[0], 10);
  if (isNaN(shift)) {
    process.stderr.write(`invalid shift: ${args[0]}\n`);
    process.exit(1);
  }

  const inputFile = args[1];
  let outputFile: string | null = null;
  let decrypt = false;

  for (let i = 2; i < args.length; i++) {
    switch (args[i]) {
      case '-d':
      case '--decrypt':
        decrypt = true;
        break;
      case '-o':
      case '--output':
        if (i + 1 >= args.length) {
          process.stderr.write('missing value for -o/--output\n');
          process.exit(1);
        }
        outputFile = args[++i];
        break;
      default:
        process.stderr.write(`unknown flag: ${args[i]}\n`);
        process.exit(1);
    }
  }

  const text = await Bun.file(inputFile).text();
  const result = cipher(text, decrypt ? -shift : shift);

  if (outputFile) {
    await Bun.write(outputFile, result);
  } else {
    process.stdout.write(result);
  }
}
