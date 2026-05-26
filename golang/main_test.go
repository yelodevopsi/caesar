package main

import (
	"strings"
	"testing"
)

func run(t *testing.T, input string, shift int) string {
	t.Helper()
	var out strings.Builder
	if err := cipher(strings.NewReader(input), &out, shift); err != nil {
		t.Fatalf("cipher error: %v", err)
	}
	return out.String()
}

func TestEncrypt(t *testing.T) {
	tests := []struct {
		input, want string
		shift       int
	}{
		{"ABC", "BCD", 1},
		{"abc", "bcd", 1},
		{"XYZ", "YZÆ", 1}, // wraps into Scandinavian
		{"ÆØÅ", "ØÅA", 1}, // wraps back to start
		{"Hello, World!", "Mjqqt, Øtwqi!", 5},
		{"Hei æøå!", "Mjn cde!", 5},
		{"ABC", "ABC", 0},
		{"ABC", "ABC", 29}, // full rotation
		{"Hello Æøå!", "Khoor Abc!", 3},
		{"123 !?", "123 !?", 7}, // symbols untouched
	}
	for _, tc := range tests {
		got := run(t, tc.input, tc.shift)
		if got != tc.want {
			t.Errorf("encrypt(%q, %d) = %q, want %q", tc.input, tc.shift, got, tc.want)
		}
	}
}

func TestDecrypt(t *testing.T) {
	tests := []struct {
		input, want string
		shift       int
	}{
		{"BCD", "ABC", 1},
		{"bcd", "abc", 1},
		{"YZÆ", "XYZ", 1},
		{"ØÅA", "ÆØÅ", 1},
		{"Mjqqt, Øtwqi!", "Hello, World!", 5},
		{"ABC", "ABC", 0},
		{"ABC", "ABC", 29},
	}
	for _, tc := range tests {
		got := run(t, tc.input, -tc.shift)
		if got != tc.want {
			t.Errorf("decrypt(%q, %d) = %q, want %q", tc.input, tc.shift, got, tc.want)
		}
	}
}

func TestRoundtrip(t *testing.T) {
	cases := []struct {
		text  string
		shift int
	}{
		{"Hello, World!", 5},
		{"Hei på deg, æøå!", 13},
		{"ABCDEFGHIJKLMNOPQRSTUVWXYZÆØÅ", 7},
		{"abcdefghijklmnopqrstuvwxyzæøå", 7},
		{"The quick brown fox!", 29},
	}
	for _, tc := range cases {
		encrypted := run(t, tc.text, tc.shift)
		decrypted := run(t, encrypted, -tc.shift)
		if decrypted != tc.text {
			t.Errorf("roundtrip(%q, shift=%d): got %q", tc.text, tc.shift, decrypted)
		}
	}
}
