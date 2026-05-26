import java.util.*;

public class CaesarTest {

    record Case(String input, String want, int shift) {}

    static final List<Case> ENCRYPT = List.of(
        new Case("ABC",           "BCD",           1),
        new Case("abc",           "bcd",           1),
        new Case("XYZ",           "YZ\u00C6",      1),
        new Case("\u00C6\u00D8\u00C5", "\u00D8\u00C5A", 1),
        new Case("Hello, World!", "Mjqqt, \u00D8twqi!", 5),
        new Case("Hei \u00E6\u00F8\u00E5!", "Mjn cde!", 5),
        new Case("Hello \u00C6\u00F8\u00E5!", "Khoor Abc!", 3),
        new Case("ABC",   "ABC",   0),
        new Case("ABC",   "ABC",   29),
        new Case("123 !?", "123 !?", 7)
    );

    static final List<Case> DECRYPT = List.of(
        new Case("BCD",    "ABC",   1),
        new Case("bcd",    "abc",   1),
        new Case("YZ\u00C6", "XYZ", 1),
        new Case("\u00D8\u00C5A", "\u00C6\u00D8\u00C5", 1),
        new Case("Mjqqt, \u00D8twqi!", "Hello, World!", 5),
        new Case("ABC",   "ABC",   0),
        new Case("ABC",   "ABC",   29)
    );

    static final List<Case> ROUNDTRIP = List.of(
        new Case("Hello, World!",                         null, 5),
        new Case("Hei p\u00E5 deg, \u00E6\u00F8\u00E5!", null, 13),
        new Case("ABCDEFGHIJKLMNOPQRSTUVWXYZ\u00C6\u00D8\u00C5", null, 7),
        new Case("abcdefghijklmnopqrstuvwxyz\u00E6\u00F8\u00E5", null, 7),
        new Case("The quick brown fox!", null, 29)
    );

    public static void main(String[] args) {
        int pass = 0, fail = 0;

        for (Case c : ENCRYPT) {
            String got = Caesar.cipher(c.input(), c.shift());
            if (got.equals(c.want())) { pass++; }
            else { fail++; System.err.printf("FAIL encrypt(%s, %d): got %s, want %s%n", c.input(), c.shift(), got, c.want()); }
        }

        for (Case c : DECRYPT) {
            String got = Caesar.cipher(c.input(), -c.shift());
            if (got.equals(c.want())) { pass++; }
            else { fail++; System.err.printf("FAIL decrypt(%s, %d): got %s, want %s%n", c.input(), c.shift(), got, c.want()); }
        }

        for (Case c : ROUNDTRIP) {
            String enc = Caesar.cipher(c.input(), c.shift());
            String dec = Caesar.cipher(enc, -c.shift());
            if (dec.equals(c.input())) { pass++; }
            else { fail++; System.err.printf("FAIL roundtrip(%s, %d): got %s%n", c.input(), c.shift(), dec); }
        }

        System.out.printf("%d/%d tests passed%n", pass, pass + fail);
        if (fail > 0) System.exit(1);
    }
}
