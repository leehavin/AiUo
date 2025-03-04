using System;
using AiUo.Logging;

namespace AiUo;

/// <summary>
/// 表示支持Dispose的抽象基础类
/// </summary>
public abstract class Disposable : IDisposable
{
    /// <summary>
    /// 获取对象是否已释放
    /// </summary>
    public bool IsDisposed { get; private set; }

    /// <summary>
    /// 关闭和释放所有相关资源
    /// </summary>
    public void Dispose()
    {
        if (!IsDisposed )
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        IsDisposed = true;
    }
    /// <summary>
    /// 关闭和释放所有相关资源
    /// </summary>
    public virtual void Close() => Dispose();

    /// <summary>
    /// 析构函数
    /// </summary>
    ~Disposable()
    {
        LogUtil.Warning("类型通过析构函数释放资源。typeName:{typeName}", GetType().FullName);
        Dispose(false);
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    /// <param name="disposing">true:主动释放 false:析构函数释放</param>
    protected abstract void Dispose(bool disposing);
}