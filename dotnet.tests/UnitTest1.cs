using Caesar;

public class EncryptTests
{
    [Theory]
    [InlineData("ABC",           "BCD",           1)]
    [InlineData("abc",           "bcd",           1)]
    [InlineData("XYZ",           "YZÆ",           1)]
    [InlineData("ÆØÅ",           "ØÅA",           1)]
    [InlineData("Hello, World!", "Mjqqt, Øtwqi!", 5)]
    [InlineData("Hei æøå!",      "Mjn cde!",      5)]
    [InlineData("Hello Æøå!",    "Khoor Abc!",    3)]
    [InlineData("ABC",           "ABC",           0)]
    [InlineData("ABC",           "ABC",           29)]
    [InlineData("123 !?",        "123 !?",        7)]
    public void Encrypt(string input, string want, int shift) =>
        Assert.Equal(want, Cipher.Apply(input, shift));
}

public class DecryptTests
{
    [Theory]
    [InlineData("BCD",           "ABC",           1)]
    [InlineData("bcd",           "abc",           1)]
    [InlineData("YZÆ",           "XYZ",           1)]
    [InlineData("ØÅA",           "ÆØÅ",           1)]
    [InlineData("Mjqqt, Øtwqi!", "Hello, World!", 5)]
    [InlineData("ABC",           "ABC",           0)]
    [InlineData("ABC",           "ABC",           29)]
    public void Decrypt(string input, string want, int shift) =>
        Assert.Equal(want, Cipher.Apply(input, -shift));
}

public class RoundtripTests
{
    [Theory]
    [InlineData("Hello, World!",                 5)]
    [InlineData("Hei på deg, æøå!",              13)]
    [InlineData("ABCDEFGHIJKLMNOPQRSTUVWXYZÆØÅ", 7)]
    [InlineData("abcdefghijklmnopqrstuvwxyzæøå", 7)]
    [InlineData("The quick brown fox!",          29)]
    public void Roundtrip(string text, int shift) =>
        Assert.Equal(text, Cipher.Apply(Cipher.Apply(text, shift), -shift));
}

