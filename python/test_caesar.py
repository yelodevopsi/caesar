import unittest
from caesar import cipher


class TestEncrypt(unittest.TestCase):
    cases = [
        ('ABC',           'BCD',           1),
        ('abc',           'bcd',           1),
        ('XYZ',           'YZÆ',           1),
        ('ÆØÅ',           'ØÅA',           1),
        ('Hello, World!', 'Mjqqt, Øtwqi!', 5),
        ('Hei æøå!',      'Mjn cde!',      5),
        ('Hello Æøå!',    'Khoor Abc!',    3),
        ('ABC',           'ABC',           0),
        ('ABC',           'ABC',           29),
        ('123 !?',        '123 !?',        7),
    ]

    def test_encrypt(self):
        for text, want, shift in self.cases:
            with self.subTest(text=text, shift=shift):
                self.assertEqual(cipher(text, shift), want)


class TestDecrypt(unittest.TestCase):
    cases = [
        ('BCD',            'ABC',           1),
        ('bcd',            'abc',           1),
        ('YZÆ',            'XYZ',           1),
        ('ØÅA',            'ÆØÅ',           1),
        ('Mjqqt, Øtwqi!',  'Hello, World!', 5),
        ('ABC',            'ABC',           0),
        ('ABC',            'ABC',           29),
    ]

    def test_decrypt(self):
        for text, want, shift in self.cases:
            with self.subTest(text=text, shift=shift):
                self.assertEqual(cipher(text, -shift), want)


class TestRoundtrip(unittest.TestCase):
    cases = [
        ('Hello, World!',                 5),
        ('Hei på deg, æøå!',              13),
        ('ABCDEFGHIJKLMNOPQRSTUVWXYZÆØÅ', 7),
        ('abcdefghijklmnopqrstuvwxyzæøå', 7),
        ('The quick brown fox!',          29),
    ]

    def test_roundtrip(self):
        for text, shift in self.cases:
            with self.subTest(text=text, shift=shift):
                self.assertEqual(cipher(cipher(text, shift), -shift), text)


if __name__ == '__main__':
    unittest.main()
