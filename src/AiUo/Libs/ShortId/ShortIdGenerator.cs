using System;
using System.Security.Cryptography;

namespace AiUo.ShortId;

/// <summary>
/// 类UUID短唯一ID生成器，可指定长度和使用的字符，不确保唯一!
/// </summary>
public class ShortIdGenerator
{
    private static readonly RandomNumberGenerator _random = RandomNumberGenerator.Create();
    private ShortIdOptions _options;
    private const string UPPER_LETTERS = "ABCDEFGHIJKLMNPQRSTUVWXY";
    private const string LOWER_LETTERS = "abcdefghjklmnopqrstuvwxyz";
    private const string NUMBERS = "0123456789";
    private const string UUID_SPECIALS = "_-";
    private string _alphabet;

    public ShortIdGenerator(ShortIdOptions options = null)
    {
        _options = options ?? new ShortIdOptions();
    }
    public string Generate(int size = 21)
    {
        var alphabet = GetAlphabet();
        if (alphabet == null)
            throw new ArgumentNullException("字母表不能为空");
        if (alphabet.Length <= 0 || alphabet.Length >= 256)
            throw new ArgumentOutOfRangeException("字母表必须包含 1 到 255 个符号");
        if (size <= 0)
            throw new ArgumentOutOfRangeException("size必须大于零");

        var mask = (2 << 31 - Clz32((alphabet.Length - 1) | 1)) - 1;
        var step = (int)Math.Ceiling(1.6 * mask * size / alphabet.Length);
        var idBuilder = new char[size];
        var bytes = new byte[step];
        int cnt = 0;
        while (true)
        {
            _random.GetBytes(bytes);
            for (var i = 0; i < step; i++)
            {
                var alphabetIndex = bytes[i] & mask;
                if (alphabetIndex >= alphabet.Length) continue;
                idBuilder[cnt] = alphabet[alphabetIndex];
                if (++cnt == size)
                {
                    return new string(idBuilder);
                }
            }
        }
    }

    private string GetAlphabet()
    {
        if (string.IsNullOrEmpty(_alphabet))
        {
            if (_options.UseUuidSpecials)
                _alphabet += UUID_SPECIALS;
            if (_options.UseNumbers)
                _alphabet += NUMBERS;
            if (_options.UseLowerLetters)
                _alphabet += LOWER_LETTERS;
            if (_options.UseUpperLetters)
                _alphabet += UPPER_LETTERS;
            _alphabet += _options.CustomAlphabet;
        }
        return _alphabet;
    }
    internal static int Clz32(int x)
    {
        const int numIntBits = sizeof(int) * 8;
        x |= x >> 1;
        x |= x >> 2;
        x |= x >> 4;
        x |= x >> 8;
        x |= x >> 16;
        x -= x >> 1 & 0x55555555;
        x = (x >> 2 & 0x33333333) + (x & 0x33333333);
        x = (x >> 4) + x & 0x0f0f0f0f;
        x += x >> 8;
        x += x >> 16;
        return numIntBits - (x & 0x0000003f);
    }
}