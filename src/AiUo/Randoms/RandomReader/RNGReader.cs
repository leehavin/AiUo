using System.Security.Cryptography;

namespace AiUo.Randoms;

public class RNGReader : RandomReaderBase
{
    public readonly RandomNumberGenerator RNG = RandomNumberGenerator.Create();
    public override byte[] ReadBytes(int size)
    {
        var buffer = new byte[size];
        RNG.GetBytes(buffer);
        return buffer;
    }
    public override void Dispose()
    {
        RNG.Dispose();
    }
}