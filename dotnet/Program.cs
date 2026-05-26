namespace Caesar;

public static class Cipher
{
    private static readonly char[] Upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZÆØÅ".ToCharArray();
    private static readonly char[] Lower = "abcdefghijklmnopqrstuvwxyzæøå".ToCharArray();
    private static readonly Dictionary<char, int> Idx = new(58);

    static Cipher()
    {
        for (int i = 0; i < Upper.Length; i++) Idx[Upper[i]] = i;
        for (int i = 0; i < Lower.Length; i++) Idx[Lower[i]] = ~i;
    }

    public static string Apply(string text, int shift)
    {
        int size = Upper.Length;
        shift = ((shift % size) + size) % size;
        return string.Create(text.Length, (text, shift), static (span, state) =>
        {
            var (src, n) = state;
            int size = Upper.Length;
            for (int i = 0; i < src.Length; i++)
            {
                char ch = src[i];
                span[i] = Idx.TryGetValue(ch, out int pos)
                    ? pos >= 0 ? Upper[(pos + n) % size]
                                : Lower[(~pos + n) % size]
                    : ch;
            }
        });
    }
}

class Program
{
    static int Main(string[] args)
    {
        if (args.Length < 2)
        {
            Console.Error.WriteLine("usage: caesar <shift> <file> [-d|--decrypt] [-o|--output <file>]");
            return 1;
        }

        if (!int.TryParse(args[0], out int shift))
        {
            Console.Error.WriteLine($"invalid shift: {args[0]}");
            return 1;
        }

        string inputFile = args[1];
        string? outputFile = null;
        bool decrypt = false;

        for (int i = 2; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "-d" or "--decrypt":
                    decrypt = true;
                    break;
                case "-o" or "--output":
                    if (i + 1 >= args.Length)
                    {
                        Console.Error.WriteLine("missing value for -o/--output");
                        return 1;
                    }
                    outputFile = args[++i];
                    break;
                default:
                    Console.Error.WriteLine($"unknown flag: {args[i]}");
                    return 1;
            }
        }

        string text;
        try { text = File.ReadAllText(inputFile, System.Text.Encoding.UTF8); }
        catch (Exception e) { Console.Error.WriteLine($"could not read file: {e.Message}"); return 1; }

        string result = Cipher.Apply(text, decrypt ? -shift : shift);

        if (outputFile is not null)
        {
            try { File.WriteAllText(outputFile, result, System.Text.Encoding.UTF8); }
            catch (Exception e) { Console.Error.WriteLine($"could not write file: {e.Message}"); return 1; }
        }
        else
        {
            Console.Write(result);
        }

        return 0;
    }
}

