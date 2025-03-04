using System;
using System.Collections.Concurrent;

namespace AiUo.Collections;

/// <summary>
/// 线程安全对象池
/// </summary>
/// <typeparam name="T"></typeparam>
public class ObjectPool<T>
{
    private ConcurrentBag<T> _objects = new ConcurrentBag<T>();
    private Func<T> _objectGenerator;
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="objectGenerator">构建对象的构造器，当对象池中无对象时使用此构造器构造新的对象放入对象池</param>
    public ObjectPool(Func<T> objectGenerator = null) => _objectGenerator = objectGenerator;

    /// <summary>
    /// 从对象池中取对象，如没有创建一个加入对象池并返回
    /// </summary>
    /// <returns></returns>
    public T Get()
    {
        T ret = default(T);
        if (_objects.TryTake(out ret)) return ret;
        if (_objectGenerator == null)
            throw new Exception("对象池中对象不足，并且对象构造器为null。");
        return _objectGenerator();
    }

    /// <summary>
    /// 尝试从对象池中取对象，如果没有返回false
    /// </summary>
    /// <param name="item">从对象池取出的对象</param>
    /// <returns></returns>
    public bool TryTake(out T item) => _objects.TryTake(out item);

    /// <summary>
    /// 将对象放入对象池
    /// </summary>
    /// <param name="item"></param>
    public void Put(T item) => _objects.Add(item);
}