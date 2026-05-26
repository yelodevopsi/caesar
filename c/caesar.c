/* caesar.c — Caesar cipher CLI  (C17, UTF-8, MSVC/gcc/clang compatible)
 *
 * Usage: caesar [-d] [-o output] <shift> <file>
 *
 * Alphabet: ABCDEFGHIJKLMNOPQRSTUVWXYZÆØÅ  (29 chars, both cases)
 * All other bytes are passed through unchanged.
 * Compile: cl /O2 /W3 /std:c17 /nologo /Fe:caesar.exe caesar.c
 */

#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#ifdef _WIN32
#  include <fcntl.h>
#  include <io.h>
#endif

#define ALPHA_SIZE 29
#define IDX_NONE   INT32_MIN

/* Scandinavian extensions: Æ=U+00C6, Ø=U+00D8, Å=U+00C5 (upper)
 *                          æ=U+00E6, ø=U+00F8, å=U+00E5 (lower)
 * All codepoints fit in uint8_t, so a 256-entry lookup table covers them. */
static const uint32_t UPPER[ALPHA_SIZE] = {
    'A','B','C','D','E','F','G','H','I','J','K','L','M',
    'N','O','P','Q','R','S','T','U','V','W','X','Y','Z',
    0xC6u, 0xD8u, 0xC5u   /* Æ Ø Å */
};
static const uint32_t LOWER[ALPHA_SIZE] = {
    'a','b','c','d','e','f','g','h','i','j','k','l','m',
    'n','o','p','q','r','s','t','u','v','w','x','y','z',
    0xE6u, 0xF8u, 0xE5u   /* æ ø å */
};

/* idx[cp] == i     → uppercase letter at alphabet position i
 * idx[cp] == ~i    → lowercase letter at alphabet position i
 * idx[cp] == IDX_NONE → not in alphabet */
static int32_t idx[256];

void init(void) {
    for (int i = 0; i < 256; i++) idx[i] = IDX_NONE;
    for (int i = 0; i < ALPHA_SIZE; i++) {
        idx[UPPER[i]] = (int32_t)i;
        idx[LOWER[i]] = ~(int32_t)i;
    }
}

/* Write Unicode codepoint cp as UTF-8 into buf; return bytes written.
 * Our alphabet codepoints are all ≤ 0xFF, so at most 2 bytes. */
static int write_cp(uint8_t *buf, uint32_t cp) {
    if (cp < 0x80u) { buf[0] = (uint8_t)cp; return 1; }
    buf[0] = (uint8_t)(0xC0u | (cp >> 6));
    buf[1] = (uint8_t)(0x80u | (cp & 0x3Fu));
    return 2;
}

/* Cipher in[0..n) → out, applying shift.  Returns output length.
 * out must be at least 2*n bytes (worst case: every ASCII letter → 2-byte cp).
 * shift may be any integer; normalisation is applied internally. */
size_t caesar_cipher(const uint8_t *in, size_t n, uint8_t *out, int shift) {
    shift = ((shift % ALPHA_SIZE) + ALPHA_SIZE) % ALPHA_SIZE;
    size_t i = 0, j = 0;

    while (i < n) {
        uint8_t  b = in[i];
        uint32_t cp;
        int      seq;   /* byte length of this UTF-8 sequence */

        if (b < 0x80u) {
            cp = b; seq = 1;
        } else if (b < 0xC0u) {
            /* Stray continuation byte — pass through as-is */
            out[j++] = b; i++; continue;
        } else if (b < 0xE0u) {
            seq = 2;
            if (i + 1 >= n) { out[j++] = b; i++; continue; }
            cp = ((uint32_t)(b & 0x1Fu) << 6) | (in[i + 1] & 0x3Fu);
        } else if (b < 0xF0u) {
            seq = 3;
            if (i + 2 >= n) {
                memcpy(out + j, in + i, n - i); j += n - i; i = n; continue;
            }
            cp = ((uint32_t)(b & 0x0Fu) << 12)
               | ((uint32_t)(in[i + 1] & 0x3Fu) << 6)
               |  (uint32_t)(in[i + 2] & 0x3Fu);
        } else {
            seq = 4;
            if (i + 3 >= n) {
                memcpy(out + j, in + i, n - i); j += n - i; i = n; continue;
            }
            cp = ((uint32_t)(b & 0x07u) << 18)
               | ((uint32_t)(in[i + 1] & 0x3Fu) << 12)
               | ((uint32_t)(in[i + 2] & 0x3Fu) << 6)
               |  (uint32_t)(in[i + 3] & 0x3Fu);
        }

        if (cp < 256u && idx[cp] != IDX_NONE) {
            int32_t pos   = idx[cp];
            int     lower = pos < 0;
            int     alpha = lower ? ~pos : (int)pos;
            int     ni    = (alpha + shift) % ALPHA_SIZE;
            j += write_cp(out + j, lower ? LOWER[ni] : UPPER[ni]);
        } else {
            memcpy(out + j, in + i, (size_t)seq);
            j += (size_t)seq;
        }
        i += (size_t)seq;
    }
    return j;
}

/* ── CLI (excluded when building tests) ─────────────────────────────────── */
#ifndef TEST

static uint8_t *read_whole(const char *path, size_t *sz) {
    FILE *f = fopen(path, "rb");
    if (!f) return NULL;
    if (fseek(f, 0, SEEK_END) != 0) { fclose(f); return NULL; }
    long len = ftell(f);
    if (len < 0) { fclose(f); return NULL; }
    rewind(f);
    uint8_t *buf = (uint8_t *)malloc((size_t)len + 1u);
    if (!buf) { fclose(f); return NULL; }
    *sz = fread(buf, 1u, (size_t)len, f);
    fclose(f);
    return buf;
}

int main(int argc, char *argv[]) {
#ifdef _WIN32
    _setmode(_fileno(stdout), _O_BINARY);
#endif
    init();

    int         shift     = 0;
    const char *infile    = NULL;
    const char *outfile   = NULL;
    int         decrypt   = 0;
    int         positional = 0;   /* 0=want shift, 1=want file, 2=done */

    for (int i = 1; i < argc; i++) {
        if (strcmp(argv[i], "-d") == 0 || strcmp(argv[i], "--decrypt") == 0) {
            decrypt = 1;
        } else if (strcmp(argv[i], "-o") == 0 || strcmp(argv[i], "--output") == 0) {
            if (++i < argc) outfile = argv[i];
        } else if (positional == 0) {
            shift = atoi(argv[i]);
            positional = 1;
        } else {
            infile = argv[i];
            positional = 2;
        }
    }

    if (positional < 2) {
        fprintf(stderr, "Usage: caesar [-d] [-o output] <shift> <file>\n");
        return 1;
    }

    if (decrypt) shift = -shift;

    size_t   in_sz;
    uint8_t *in = read_whole(infile, &in_sz);
    if (!in) {
        fprintf(stderr, "caesar: cannot open '%s'\n", infile);
        return 1;
    }

    uint8_t *out = (uint8_t *)malloc(2u * in_sz + 1u);
    if (!out) { free(in); return 1; }

    size_t out_sz = caesar_cipher(in, in_sz, out, shift);
    free(in);

    if (outfile) {
        FILE *f = fopen(outfile, "wb");
        if (!f) { free(out); return 1; }
        fwrite(out, 1u, out_sz, f);
        fclose(f);
    } else {
        fwrite(out, 1u, out_sz, stdout);
    }

    free(out);
    return 0;
}

#endif /* !TEST */
