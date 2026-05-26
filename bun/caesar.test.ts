import { describe, it, expect } from 'bun:test';
import { cipher } from './caesar';

describe('encrypt', () => {
  const cases: [string, string, number][] = [
    ['ABC',           'BCD',           1],
    ['abc',           'bcd',           1],
    ['XYZ',           'YZÆ',           1],
    ['ÆØÅ',           'ØÅA',           1],
    ['Hello, World!', 'Mjqqt, Øtwqi!', 5],
    ['Hei æøå!',      'Mjn cde!',      5],
    ['Hello Æøå!',    'Khoor Abc!',    3],
    ['ABC',           'ABC',           0],
    ['ABC',           'ABC',           29],
    ['123 !?',        '123 !?',        7],
  ];
  for (const [input, want, shift] of cases) {
    it(`cipher(${JSON.stringify(input)}, ${shift}) === ${JSON.stringify(want)}`, () => {
      expect(cipher(input, shift)).toBe(want);
    });
  }
});

describe('decrypt', () => {
  const cases: [string, string, number][] = [
    ['BCD',            'ABC',           1],
    ['bcd',            'abc',           1],
    ['YZÆ',            'XYZ',           1],
    ['ØÅA',            'ÆØÅ',           1],
    ['Mjqqt, Øtwqi!',  'Hello, World!', 5],
    ['ABC',            'ABC',           0],
    ['ABC',            'ABC',           29],
  ];
  for (const [input, want, shift] of cases) {
    it(`cipher(${JSON.stringify(input)}, -${shift}) === ${JSON.stringify(want)}`, () => {
      expect(cipher(input, -shift)).toBe(want);
    });
  }
});

describe('roundtrip', () => {
  const cases: [string, number][] = [
    ['Hello, World!',                  5],
    ['Hei på deg, æøå!',               13],
    ['ABCDEFGHIJKLMNOPQRSTUVWXYZÆØÅ',  7],
    ['abcdefghijklmnopqrstuvwxyzæøå',  7],
    ['The quick brown fox!',           29],
  ];
  for (const [text, shift] of cases) {
    it(`roundtrip ${JSON.stringify(text)} shift=${shift}`, () => {
      expect(cipher(cipher(text, shift), -shift)).toBe(text);
    });
  }
});
