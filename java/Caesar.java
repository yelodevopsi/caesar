import java.io.*;
import java.nio.charset.StandardCharsets;
import java.nio.file.*;
import java.util.*;

public class Caesar {

    private static final char[] UPPER = "ABCDEFGHIJKLMNOPQRSTUVWXYZ\u00C6\u00D8\u00C5".toCharArray();
    private static final char[] LOWER = "abcdefghijklmnopqrstuvwxyz\u00E6\u00F8\u00E5".toCharArray();
    private static final int SIZE = UPPER.length; // 29
    private static final Map<Character, Integer> IDX = new HashMap<>(58);

    static {
        for (int i = 0; i < SIZE; i++) {
            IDX.put(UPPER[i], i);
            IDX.put(LOWER[i], ~i); // ~i encodes lowercase: ~0=-1, ~1=-2, ...
        }
    }

    static String cipher(String text, int shift) {
        shift = ((shift % SIZE) + SIZE) % SIZE;
        char[] out = text.toCharArray();
        for (int i = 0; i < out.length; i++) {
            Integer pos = IDX.get(out[i]);
            if (pos != null) {
                out[i] = pos >= 0 ? UPPER[(pos + shift) % SIZE]
                                  : LOWER[(~pos + shift) % SIZE];
            }
        }
        return new String(out);
    }

    public static void main(String[] args) throws IOException {
        if (args.length < 2) {
            System.err.println("usage: caesar <shift> <file> [-d|--decrypt] [-o|--output <file>]");
            System.exit(1);
        }

        int shift;
        try {
            shift = Integer.parseInt(args[0]);
        } catch (NumberFormatException e) {
            System.err.println("invalid shift: " + args[0]);
            System.exit(1);
            return;
        }

        String inputFile  = args[1];
        String outputFile = null;
        boolean decrypt   = false;

        for (int i = 2; i < args.length; i++) {
            switch (args[i]) {
                case "-d", "--decrypt" -> decrypt = true;
                case "-o", "--output"  -> {
                    if (i + 1 >= args.length) {
                        System.err.println("missing value for -o/--output");
                        System.exit(1);
                    }
                    outputFile = args[++i];
                }
                default -> {
                    System.err.println("unknown flag: " + args[i]);
                    System.exit(1);
                }
            }
        }

        String text   = Files.readString(Path.of(inputFile), StandardCharsets.UTF_8);
        String result = cipher(text, decrypt ? -shift : shift);

        if (outputFile != null) {
            Files.writeString(Path.of(outputFile), result, StandardCharsets.UTF_8);
        } else {
            PrintStream out = new PrintStream(System.out, true, StandardCharsets.UTF_8);
            out.print(result);
        }
    }
}
