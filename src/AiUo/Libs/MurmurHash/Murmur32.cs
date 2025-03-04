using System;
using System.Security.Cryptography;

#pragma warning disable CS1591
namespace Murmur;
#pragma warning disable CS3003 // 类型不符合 CLS
public abstract class Murmur32 : HashAlgorithm
{
    protected const uint C1 = 0xcc9e2d51;
    protected const uint C2 = 0x1b873593;

    private readonly uint _Seed;

    protected Murmur32(uint seed)
    {
        _Seed = seed;
        Reset();
    }

    public override int HashSize { get { return 32; } }
    public uint Seed { get { return _Seed; } }

    protected uint H1 { get; set; }

    protected int Length { get; set; }

    private void Reset()
    {
        H1 = Seed;
        Length = 0;
    }

    public override void Initialize()
    {
        Reset();
    }

    protected override byte[] HashFinal()
    {
        H1 = (H1 ^ (uint)Length).FMix();

        return BitConverter.GetBytes(H1);
    }
}