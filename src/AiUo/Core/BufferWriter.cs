using System;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace AiUo;

public class BufferWriter<T> : Disposable, IBufferWriter<T>
{
    #region Properties & Constructors
    private const int DEFAULT_SIZE_HINT = 256;
    private T[] _buffer;

    /// <summary>
    /// 获取已写入的字节数
    /// </summary>
    public int WrittenCount { get; private set; }

    /// <summary>
    /// 获取容量
    /// </summary>
    public int Capacity => _buffer.Length;


    public BufferWriter(int initialCapacity = 1024)
    {
        if (initialCapacity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(initialCapacity));
        }
        _buffer = ArrayPool<T>.Shared.Rent(initialCapacity);
    }
    #endregion

    #region IBufferWriter
    /// <summary>
    /// 设置向前推进
    /// </summary>
    /// <param name="count"></param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public void Advance(int count)
    {
        if (count < 0 || WrittenCount + count > Capacity)
        {
            throw new ArgumentOutOfRangeException(nameof(count));
        }
        WrittenCount += count;
    }

    /// <summary>
    /// 返回用于写入数据的Memory
    /// </summary>
    /// <param name="sizeHint">意图大小</param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    /// <returns></returns>
    public Memory<T> GetMemory(int sizeHint = 0)
    {
        this.CheckAndResizeBuffer(sizeHint);
        return _buffer.AsMemory(WrittenCount);
    }

    /// <summary>
    /// 返回用于写入数据的Span
    /// </summary>
    /// <param name="sizeHint">意图大小</param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    /// <returns></returns>
    public Span<T> GetSpan(int sizeHint = 0)
    {
        this.CheckAndResizeBuffer(sizeHint);
        return _buffer.AsSpan(this.WrittenCount);
    }

    protected override void Dispose(bool disposing)
    {
        ArrayPool<T>.Shared.Return(_buffer);
    }
    #endregion

    #region Utils
    /// <summary>
    /// 写入数据
    /// </summary>
    /// <param name="value"></param>
    public void Write(T value)
    {
        this.GetSpan(1)[0] = value;
        this.WrittenCount += 1;
    }

    /// <summary>
    /// 写入数据
    /// </summary>
    /// <param name="value">值</param> 
    public void Write(ReadOnlySpan<T> value)
    {
        if (value.IsEmpty == false)
        {
            value.CopyTo(this.GetSpan(value.Length));
            this.WrittenCount += value.Length;
        }
    }

    /// <summary>
    /// 获取已数入的数据
    /// </summary>
    /// <returns></returns>
    public ArraySegment<T> GetWrittenSegment()
    {
        return new ArraySegment<T>(_buffer, 0, this.WrittenCount);
    }

    /// <summary>
    /// 获取已数入的数据
    /// </summary>
    public ReadOnlySpan<T> GetWrittenSpan()
    {
        return _buffer.AsSpan(0, this.WrittenCount);
    }

    /// <summary>
    /// 获取已数入的数据
    /// </summary>
    public ReadOnlyMemory<T> GetWrittenMemory()
    {
        return _buffer.AsMemory(0, this.WrittenCount);
    }

    #endregion

    /// <summary>
    /// 检测和扩容
    /// </summary>
    /// <param name="sizeHint"></param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void CheckAndResizeBuffer(int sizeHint)
    {
        if (sizeHint < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(sizeHint));
        }

        if (sizeHint == 0)
        {
            sizeHint = DEFAULT_SIZE_HINT;
        }

        var freeCapacity = Capacity - WrittenCount;
        if (sizeHint > freeCapacity)
        {
            var growBy = Math.Max(sizeHint, Capacity);
            var newSize = checked(Capacity + growBy);

            var newOwer = ArrayPool<T>.Shared.Rent(newSize);
            GetWrittenSpan().CopyTo(newOwer);
            ArrayPool<T>.Shared.Return(_buffer);
            _buffer = newOwer;
        }
    }
}