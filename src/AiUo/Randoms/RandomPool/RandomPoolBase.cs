using System;
using System.Collections.Concurrent;
using System.Threading;

namespace AiUo.Randoms;

public abstract class RandomPoolBase : IRandomReader
{
    public IRandomReader Reader { get; protected set; }
    public RandomPoolOptions Options;

    protected ConcurrentQueue<byte> _byteQueue = new ConcurrentQueue<byte>();
    protected ConcurrentQueue<int> _intQueue = new ConcurrentQueue<int>();
    protected int _byteBufferNum = 1;
    protected int _byteTotal { get => Options.ByteBufferSize * _byteBufferNum; }
    protected int _intBufferNum = 1;
    protected int _intTotal { get => Options.IntBufferSize * _intBufferNum; }

    public int IntBufferSize => Options.IntBufferSize;
    public int ByteBufferSize => Options.ByteBufferSize;
    private System.Timers.Timer _timer;

    public RandomPoolBase(RandomPoolOptions options = null)
    {
        Options = options ?? new RandomPoolOptions();
        _timer = new System.Timers.Timer(Options.CheckInterval);
        _timer.Elapsed += (sender, e) => {
            DoEnqueueIntBuffer();
            DoEnqueueByteBuffer();
        };
        _timer.Start();
    }
    private void DoEnqueueIntBuffer()
    {
        if (_intQueue.Count > _intTotal) return;
        try
        {
            if (Reader != null)
            {
                var buffer = Reader.ReadInts(IntBufferSize);
                foreach (var item in buffer)
                    _intQueue.Enqueue(item);
            }
            else
            {
                EnqueueIntBuffer(IntBufferSize);
            }
        }
        catch
        {
            if (Options.UseRNGWhenException)
            {
                Reader = new RNGReader();
                DoEnqueueIntBuffer();
                Console.WriteLine($"RandomPool提供的读取器错误，切换到RNG默认读取器。{Reader.GetType().FullName}");
            }
            else
                throw new RandomException($"RandomPool读取随机数出现异常。{Reader.GetType().FullName}"); ;
        }
    }
    protected virtual void EnqueueIntBuffer(int size)
    {
        throw new Exception("RandomPool没有定义Reader或者子类没有重写EnqueueIntBuffer");
    }
    protected void DoEnqueueByteBuffer()
    {
        if (_byteQueue.Count > _byteTotal) return;
        try
        {
            if (Reader != null)
            {
                var buffer = Reader.ReadBytes(ByteBufferSize);
                foreach (var item in buffer)
                    _byteQueue.Enqueue(item);
            }
            else
            {
                EnqueueByteBuffer(ByteBufferSize);
            }
        }
        catch
        {
            if (Options.UseRNGWhenException)
            {
                Reader = new RNGReader();
                DoEnqueueByteBuffer();
                Console.WriteLine($"RandomPool提供的读取器错误，切换到RNG默认读取器。{Reader.GetType().FullName}");
            }
            else
                throw new RandomException($"RandomPool读取随机数出现异常。{Reader.GetType().FullName}"); ;
        }
    }

    protected virtual void EnqueueByteBuffer(int size)
    {
        throw new Exception("RandomPool没有定义Reader或者子类没有重写EnqueueByteBuffer");
    }

    public byte[] ReadBytes(int size)
    {
        byte[] ret = new byte[size];
        int timeTotal = 0;
        bool addBuffer = false;
        for (int i = 0; i < size; i++)
        {
            while (!_byteQueue.TryDequeue(out ret[i]))
            {
                if (!addBuffer)
                {
                    addBuffer = true;
                    _byteBufferNum++;
                }
                if (timeTotal > Options.Timeout)
                    throw new Exception($"RandomPool获取随机数超时！timeout:{Options.Timeout} type:{GetType().Name}");
                Thread.Sleep(200);
                timeTotal += 200;
            }
        }
        return ret;
    }

    public int[] ReadInts(int size)
    {
        int[] ret = new int[size];
        int timeTotal = 0;
        bool addBuffer = false;
        for (int i = 0; i < size; i++)
        {
            while (!_intQueue.TryDequeue(out ret[i]))
            {
                if (!addBuffer)
                {
                    addBuffer = true;
                    _intBufferNum++;
                }
                if (timeTotal > Options.Timeout)
                    throw new Exception($"RandomPool获取随机数超时！timeout:{Options.Timeout} type:{GetType().Name}");
                Thread.Sleep(200);
                timeTotal += 200;
            }
        }
        return ret;
    }
    public virtual void Dispose()
    {
        _timer.Dispose();
    }
}