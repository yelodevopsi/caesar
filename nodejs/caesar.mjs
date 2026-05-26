#!/usr/bin/env node
import { readFileSync, writeFileSync } from "fs";

const upper = [...'ABCDEFGHIJKLMNOPQRSTUVWXYZÆØÅ'];
const lower = [...'abcdefghijklmnopqrstuvwxyzæøå'];
const size  = upper.length;

// Map each char to its index; lower indices are bitwise-NOT encoded (~0=-1, ~1=-2, ...)
const idx = new Map([
  ...upper.map((ch, i) => [ch, i]),
  ...lower.map((ch, i) => [ch, ~i]),
]);

function cipher(text, shift) {
  shift = ((shift % size) + size) % size;
  return [...text].map(ch => {
    const pos = idx.get(ch);
    if (pos === undefined) return ch;
    return pos >= 0
      ? upper[(pos + shift) % size]
      : lower[(~pos + shift) % size];
  }).join('');
}

const args = process.argv.slice(2);
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
let outputFile = null;
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

let text;
try {
  text = readFileSync(inputFile, 'utf8');
} catch (err) {
  process.stderr.write(`could not read file: ${err.message}\n`);
  process.exit(1);
}

const result = cipher(text, decrypt ? -shift : shift);

if (outputFile) {
  try {
    writeFileSync(outputFile, result, 'utf8');
  } catch (err) {
    process.stderr.write(`could not write file: ${err.message}\n`);
    process.exit(1);
  }
} else {
  process.stdout.write(result);
}
