using System.Security.Cryptography;

#pragma warning disable CS1591
namespace Murmur;

public abstract class Murmur128 : HashAlgorithm
{
    private readonly uint _Seed;
    protected Murmur128(uint seed)
    {
        _Seed = seed;
    }

#pragma warning disable CS3003 // 类型不符合 CLS
    public uint Seed { get { return _Seed; } }
#pragma warning restore CS3003 // 类型不符合 CLS

    public override int HashSize { get { return 128; } }
}