using System;

namespace AiUo.Randoms;

public interface IRandomReader : IDisposable
{
    byte[] ReadBytes(int size);
    int[] ReadInts(int size);
}
public abstract class RandomReaderBase : IRandomReader
{
    public abstract void Dispose();

    public abstract byte[] ReadBytes(int size);

    public virtual int[] ReadInts(int size)
    {
        var ret = new int[size];
        var buffer = ReadBytes(size * 4);
        for (int i = 0; i < size; i++)
        {
            var value = BitConverter.ToInt32(buffer, i * 4);
            if (value >= 0)
                ret[i] = value;
            else
                ret[i] = value == int.MinValue ? int.MaxValue : -value;
        }
        return ret;
    }
}