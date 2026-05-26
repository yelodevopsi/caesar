package main

import (
	"bufio"
	"fmt"
	"io"
	"os"
	"strconv"
)

var (
	upper = []rune("ABCDEFGHIJKLMNOPQRSTUVWXYZÆØÅ")
	lower = []rune("abcdefghijklmnopqrstuvwxyzæøå")
	idx   = make(map[rune]int, 58)
)

func init() {
	for i, r := range upper {
		idx[r] = i
	}
	for i, r := range lower {
		idx[r] = ^i
	} // ^i encodes lower: ^0=-1, ^1=-2, ...
}

func cipher(reader io.Reader, writer io.Writer, shift int) error {
	size := len(upper)
	shift = ((shift % size) + size) % size
	br, bw := bufio.NewReader(reader), bufio.NewWriter(writer)
	for {
		ch, _, err := br.ReadRune()
		if err == io.EOF {
			break
		}
		if err != nil {
			return err
		}
		if pos, ok := idx[ch]; ok {
			if pos >= 0 {
				ch = upper[(pos+shift)%size]
			} else {
				ch = lower[(^pos+shift)%size]
			}
		}
		if _, err := bw.WriteRune(ch); err != nil {
			return err
		}
	}
	return bw.Flush()
}

func main() {
	if len(os.Args) < 3 {
		fmt.Fprintln(os.Stderr, "usage: caesar <shift> <file> [-d|--decrypt] [-o|--output <file>]")
		os.Exit(1)
	}
	shift, err := strconv.Atoi(os.Args[1])
	if err != nil {
		fmt.Fprintf(os.Stderr, "invalid shift: %v\n", err)
		os.Exit(1)
	}
	var outputFile string
	decrypt := false
	for i := 3; i < len(os.Args); i++ {
		switch os.Args[i] {
		case "-d", "--decrypt":
			decrypt = true
		case "-o", "--output":
			if i+1 >= len(os.Args) {
				fmt.Fprintln(os.Stderr, "missing value for -o/--output")
				os.Exit(1)
			}
			i++
			outputFile = os.Args[i]
		default:
			fmt.Fprintf(os.Stderr, "unknown flag: %s\n", os.Args[i])
			os.Exit(1)
		}
	}
	if decrypt {
		shift = -shift
	}
	in, err := os.Open(os.Args[2])
	if err != nil {
		fmt.Fprintf(os.Stderr, "could not open file: %v\n", err)
		os.Exit(1)
	}
	defer in.Close()
	var out io.Writer = os.Stdout
	if outputFile != "" {
		outFile, err := os.Create(outputFile)
		if err != nil {
			fmt.Fprintf(os.Stderr, "could not create output file: %v\n", err)
			os.Exit(1)
		}
		defer outFile.Close()
		out = outFile
	}
	if err := cipher(in, out, shift); err != nil {
		fmt.Fprintf(os.Stderr, "cipher error: %v\n", err)
		os.Exit(1)
	}
}
