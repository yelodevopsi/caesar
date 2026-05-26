import { describe, it } from 'node:test';
import assert from 'node:assert/strict';

const upper = [...'ABCDEFGHIJKLMNOPQRSTUVWXYZÆØÅ'];
const lower = [...'abcdefghijklmnopqrstuvwxyzæøå'];
const size  = upper.length;

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

describe('encrypt', () => {
  const cases = [
    ['ABC',            'BCD',            1],
    ['abc',            'bcd',            1],
    ['XYZ',            'YZÆ',            1],
    ['ÆØÅ',            'ØÅA',            1],
    ['Hello, World!',  'Mjqqt, Øtwqi!',  5],
    ['Hei æøå!',       'Mjn cde!',       5],
    ['Hello Æøå!',     'Khoor Abc!',     3],
    ['ABC',            'ABC',            0],
    ['ABC',            'ABC',            29],
    ['123 !?',         '123 !?',         7],
  ];
  for (const [input, want, shift] of cases) {
    it(`cipher(${JSON.stringify(input)}, ${shift}) === ${JSON.stringify(want)}`, () => {
      assert.equal(cipher(input, shift), want);
    });
  }
});

describe('decrypt', () => {
  const cases = [
    ['BCD',            'ABC',            1],
    ['bcd',            'abc',            1],
    ['YZÆ',            'XYZ',            1],
    ['ØÅA',            'ÆØÅ',            1],
    ['Mjqqt, Øtwqi!',  'Hello, World!',  5],
    ['ABC',            'ABC',            0],
    ['ABC',            'ABC',            29],
  ];
  for (const [input, want, shift] of cases) {
    it(`cipher(${JSON.stringify(input)}, -${shift}) === ${JSON.stringify(want)}`, () => {
      assert.equal(cipher(input, -shift), want);
    });
  }
});

describe('roundtrip', () => {
  const cases = [
    ['Hello, World!',                         5],
    ['Hei på deg, æøå!',                      13],
    ['ABCDEFGHIJKLMNOPQRSTUVWXYZÆØÅ',         7],
    ['abcdefghijklmnopqrstuvwxyzæøå',         7],
    ['The quick brown fox!',                  29],
  ];
  for (const [text, shift] of cases) {
    it(`roundtrip ${JSON.stringify(text)} shift=${shift}`, () => {
      assert.equal(cipher(cipher(text, shift), -shift), text);
    });
  }
});
