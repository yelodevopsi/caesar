/* caesar_test.c — plain test runner for caesar_cipher()
 *
 * Compile & run (from Developer Command Prompt):
 *   cl /O2 /W3 /std:c17 /nologo /DTEST /Fe:caesar_test.exe caesar.c caesar_test.c
 *   .\caesar_test.exe
 * Or simply:  nmake test
 */

#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>

/* Declarations from caesar.c (compiled with -DTEST, so main() is excluded) */
void   init(void);
size_t caesar_cipher(const uint8_t *in, size_t n, uint8_t *out, int shift);

/* ── UTF-8 byte sequences for Scandinavian chars ─────────────────────────── */
/*  Æ */ #define UC_AE  "\xC3\x86"
/*  Ø */ #define UC_OE  "\xC3\x98"
/*  Å */ #define UC_AA  "\xC3\x85"
/*  æ */ #define LC_AE  "\xC3\xA6"
/*  ø */ #define LC_OE  "\xC3\xB8"
/*  å */ #define LC_AA  "\xC3\xA5"

/* Full alphabet strings */
#define UPPER_ALPHA "ABCDEFGHIJKLMNOPQRSTUVWXYZ" UC_AE UC_OE UC_AA
#define LOWER_ALPHA "abcdefghijklmnopqrstuvwxyz" LC_AE LC_OE LC_AA

/* ── Counters ─────────────────────────────────────────────────────────────── */
static int passed = 0, total = 0;

/* ── Helpers ──────────────────────────────────────────────────────────────── */
static void check(const char *label, int shift,
                  const char *input, const char *expected) {
    total++;
    size_t   n   = strlen(input);
    uint8_t *out = (uint8_t *)malloc(2u * n + 2u);
    if (!out) { fprintf(stderr, "OOM\n"); exit(1); }
    size_t out_n     = caesar_cipher((const uint8_t *)input, n, out, shift);
    size_t exp_n     = strlen(expected);
    int    ok        = (out_n == exp_n) && (memcmp(out, expected, out_n) == 0);
    if (ok) {
        passed++;
    } else {
        out[out_n] = '\0';
        fprintf(stderr, "FAIL [%s]: got \"%s\", want \"%s\"\n",
                label, (char *)out, expected);
    }
    free(out);
}

static void roundtrip(const char *label, int shift, const char *input) {
    total++;
    size_t   n   = strlen(input);
    uint8_t *mid = (uint8_t *)malloc(2u * n + 2u);
    if (!mid) { fprintf(stderr, "OOM\n"); exit(1); }
    size_t   mid_n = caesar_cipher((const uint8_t *)input, n, mid, shift);
    uint8_t *out   = (uint8_t *)malloc(2u * mid_n + 2u);
    if (!out) { fprintf(stderr, "OOM\n"); exit(1); }
    size_t   out_n = caesar_cipher(mid, mid_n, out, -shift);
    int      ok    = (out_n == n) && (memcmp(out, input, n) == 0);
    if (ok) {
        passed++;
    } else {
        out[out_n] = '\0';
        fprintf(stderr, "FAIL roundtrip [%s]: got \"%s\", want \"%s\"\n",
                label, (char *)out, input);
    }
    free(mid);
    free(out);
}

/* ── Tests ────────────────────────────────────────────────────────────────── */
int main(void) {
    init();

    /* ── Encrypt ── */
    check("empty",           0,  "",           "");
    check("a+1",             1,  "a",           "b");
    check("A+1",             1,  "A",           "B");
    check("z+1→æ",           1,  "z",           LC_AE);
    check("Z+1→Æ",           1,  "Z",           UC_AE);
    check("æ+1→ø",           1,  LC_AE,         LC_OE);
    check("å+1→a (wrap)",    1,  LC_AA,         "a");
    check("Å+1→A (wrap)",    1,  UC_AA,         "A");
    check("upper alpha +1",  1,  UPPER_ALPHA,
          "BCDEFGHIJKLMNOPQRSTUVWXYZ" UC_AE UC_OE UC_AA "A");
    check("lower alpha +1",  1,  LOWER_ALPHA,
          "bcdefghijklmnopqrstuvwxyz" LC_AE LC_OE LC_AA "a");
    check("Hello+3",         3,  "Hello, World!", "Khoor, Zruog!");
    check("non-alpha passthru", 5, " ,!123\n",  " ,!123\n");

    /* ── Decrypt ── */
    check("b-1",             -1, "b",           "a");
    check("B-1",             -1, "B",           "A");
    check("a-1→å",           -1, "a",           LC_AA);
    check("A-1→Å",           -1, "A",           UC_AA);
    check("Khoor-3",         -3, "Khoor, Zruog!", "Hello, World!");
    check("upper alpha -1",  -1,
          "BCDEFGHIJKLMNOPQRSTUVWXYZ" UC_AE UC_OE UC_AA "A",
          UPPER_ALPHA);

    /* ── Roundtrip ── */
    roundtrip("ascii",        7,  "Hello, World!");
    roundtrip("scandi",      13,  UC_AE UC_OE UC_AA LC_AE LC_OE LC_AA);
    roundtrip("shift 0",      0,  "No change");
    roundtrip("shift 29",    29,  "Full cycle");

    printf("%d/%d tests passed\n", passed, total);
    return (passed == total) ? 0 : 1;
}
