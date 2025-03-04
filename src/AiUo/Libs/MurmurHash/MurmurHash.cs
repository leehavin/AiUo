using System;
using System.Security.Cryptography;

#pragma warning disable CS1591
namespace Murmur;

public enum AlgorithmPreference
{
    Auto,
    X64,
    X86
}

public enum AlgorithmType
{
    Murmur32,
    Murmur128
}

public static class MurmurHash
{
    public static Murmur32 Create32(uint seed = 0, bool managed = true)
    {
        if (managed)
            return new Murmur32ManagedX86(seed);

        return new Murmur32UnmanagedX86(seed);
    }

    public static Murmur128 Create128(uint seed = 0, bool managed = true, AlgorithmPreference preference = AlgorithmPreference.Auto)
    {
        var algorithm = managed
            ? Pick(seed, preference, s => new Murmur128ManagedX86(s), s => new Murmur128ManagedX64(s))
            : Pick(seed, preference, s => new Murmur128UnmanagedX86(s), s => new Murmur128UnmanagedX64(s));

        return algorithm as Murmur128;
    }

    private static HashAlgorithm Pick<T32, T64>(uint seed, AlgorithmPreference preference, Func<uint, T32> factory32, Func<uint, T64> factory64)
        where T32 : HashAlgorithm
        where T64 : HashAlgorithm
    {
        switch (preference)
        {
            case AlgorithmPreference.X64: return factory64(seed);
            case AlgorithmPreference.X86: return factory32(seed);
            default:
            {
                if (Is64BitProcess())
                    return factory64(seed);

                return factory32(seed);
            }
        }
    }

    static bool Is64BitProcess()
    {
        return false;
    }
}