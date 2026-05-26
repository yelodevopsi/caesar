use std::collections::HashMap;
use std::env;
use std::fs;
use std::io::{self, Write};
use std::process;

const UPPER: &[char] = &[
    'A','B','C','D','E','F','G','H','I','J','K','L','M',
    'N','O','P','Q','R','S','T','U','V','W','X','Y','Z',
    '\u{00C6}', '\u{00D8}', '\u{00C5}',
];
const LOWER: &[char] = &[
    'a','b','c','d','e','f','g','h','i','j','k','l','m',
    'n','o','p','q','r','s','t','u','v','w','x','y','z',
    '\u{00E6}', '\u{00F8}', '\u{00E5}',
];
const SIZE: usize = 29;

fn build_index() -> HashMap<char, i32> {
    let mut idx = HashMap::with_capacity(58);
    for (i, &c) in UPPER.iter().enumerate() {
        idx.insert(c, i as i32);
    }
    for (i, &c) in LOWER.iter().enumerate() {
        idx.insert(c, !(i as i32)); // !i encodes lowercase: !0=-1, !1=-2, ...
    }
    idx
}

fn cipher(text: &str, shift: i32, idx: &HashMap<char, i32>) -> String {
    let size = SIZE as i32;
    let shift = ((shift % size) + size) % size;
    text.chars()
        .map(|ch| match idx.get(&ch) {
            Some(&pos) if pos >= 0 => UPPER[((pos + shift) % size) as usize],
            Some(&pos)             => LOWER[((!pos + shift) % size) as usize],
            None                   => ch,
        })
        .collect()
}

fn main() {
    let args: Vec<String> = env::args().collect();
    if args.len() < 3 {
        eprintln!("usage: caesar <shift> <file> [-d|--decrypt] [-o|--output <file>]");
        process::exit(1);
    }

    let shift: i32 = args[1].parse().unwrap_or_else(|_| {
        eprintln!("invalid shift: {}", args[1]);
        process::exit(1);
    });

    let input_file = &args[2];
    let mut output_file: Option<&str> = None;
    let mut decrypt = false;

    let mut i = 3;
    while i < args.len() {
        match args[i].as_str() {
            "-d" | "--decrypt" => decrypt = true,
            "-o" | "--output"  => {
                if i + 1 >= args.len() {
                    eprintln!("missing value for -o/--output");
                    process::exit(1);
                }
                i += 1;
                output_file = Some(&args[i]);
            }
            flag => {
                eprintln!("unknown flag: {}", flag);
                process::exit(1);
            }
        }
        i += 1;
    }

    let text = fs::read_to_string(input_file).unwrap_or_else(|e| {
        eprintln!("could not read file: {}", e);
        process::exit(1);
    });

    let idx = build_index();
    let result = cipher(&text, if decrypt { -shift } else { shift }, &idx);

    match output_file {
        Some(path) => fs::write(path, result.as_bytes()).unwrap_or_else(|e| {
            eprintln!("could not write file: {}", e);
            process::exit(1);
        }),
        None => {
            io::stdout().write_all(result.as_bytes()).unwrap();
        }
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    fn idx() -> HashMap<char, i32> { build_index() }

    fn enc(text: &str, shift: i32) -> String { cipher(text, shift,  &idx()) }
    fn dec(text: &str, shift: i32) -> String { cipher(text, -shift, &idx()) }

    #[test]
    fn encrypt() {
        assert_eq!(enc("ABC",           1), "BCD");
        assert_eq!(enc("abc",           1), "bcd");
        assert_eq!(enc("XYZ",           1), "YZ\u{00C6}");
        assert_eq!(enc("\u{00C6}\u{00D8}\u{00C5}", 1), "\u{00D8}\u{00C5}A");
        assert_eq!(enc("Hello, World!", 5), "Mjqqt, \u{00D8}twqi!");
        assert_eq!(enc("Hei \u{00E6}\u{00F8}\u{00E5}!", 5), "Mjn cde!");
        assert_eq!(enc("Hello \u{00C6}\u{00F8}\u{00E5}!", 3), "Khoor Abc!");
        assert_eq!(enc("ABC",  0),  "ABC");
        assert_eq!(enc("ABC",  29), "ABC");
        assert_eq!(enc("123 !?", 7), "123 !?");
    }

    #[test]
    fn decrypt() {
        assert_eq!(dec("BCD",  1), "ABC");
        assert_eq!(dec("bcd",  1), "abc");
        assert_eq!(dec("YZ\u{00C6}", 1), "XYZ");
        assert_eq!(dec("\u{00D8}\u{00C5}A", 1), "\u{00C6}\u{00D8}\u{00C5}");
        assert_eq!(dec("Mjqqt, \u{00D8}twqi!", 5), "Hello, World!");
        assert_eq!(dec("ABC",  0),  "ABC");
        assert_eq!(dec("ABC",  29), "ABC");
    }

    #[test]
    fn roundtrip() {
        for (text, shift) in [
            ("Hello, World!",                              5),
            ("Hei p\u{00E5} deg, \u{00E6}\u{00F8}\u{00E5}!", 13),
            ("ABCDEFGHIJKLMNOPQRSTUVWXYZ\u{00C6}\u{00D8}\u{00C5}", 7),
            ("abcdefghijklmnopqrstuvwxyz\u{00E6}\u{00F8}\u{00E5}", 7),
            ("The quick brown fox!", 29),
        ] {
            assert_eq!(dec(&enc(text, shift), shift), text);
        }
    }
}

