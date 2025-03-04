using System;
using System.Collections.Concurrent;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace AiUo;

/// <summary>
/// 提供AiUo辅助方法
/// </summary>
public static partial class AiUoUtil
{
    /// <summary>
    /// 尝试执行指定次数
    /// </summary>
    /// <param name="func"></param>
    /// <param name="maxTries"></param>
    /// <param name="interval"></param>
    /// <returns></returns>
    public static async Task RetryExecuteAsync(Func<Task> func, int maxTries, int interval)
    {
        for (int i = 0; i < maxTries; i++)
        {
            try
            {
                await func();
                return;
            }
            catch
            {
                if (i == maxTries - 1)
                    throw;
                await Task.Delay(interval);
            }
        }
    }
    public static async Task<T> RetryExecuteAsync<T>(Func<Task<T>> func, int maxTries, int interval)
    {
        for (int i = 0; i < maxTries; i++)
        {
            try
            {
                return await func();
            }
            catch
            {
                if (i == maxTries - 1)
                    throw;
                await Task.Delay(interval);
            }
        }
        throw new NotImplementedException();
    }

    #region Other
    private static ConcurrentDictionary<Type, bool> _nullableTypeCache = new ConcurrentDictionary<Type, bool>();
    /// <summary>
    /// 判断是否为可空类型
    /// </summary>
    /// <param name="type">类型信息</param>
    /// <returns></returns>
    public static bool IsNullableType(this Type type)
    {
        if (type == null)
            throw new ArgumentException("Type类型不能为NULL。");
        if (_nullableTypeCache.TryGetValue(type, out bool ret))
            return ret;
        ret = type.IsGenericType && type.GetGenericTypeDefinition().Equals(typeof(Nullable<>));
        _nullableTypeCache.TryAdd(type, ret);
        return ret;
    }


    /// <summary>
    /// 计算分页的页数
    /// </summary>
    /// <param name="totalRecord">总记录数</param>
    /// <param name="pageSize">页大小</param>
    /// <returns></returns>
    public static int GetPageCount(int totalRecord, int pageSize)
        => (totalRecord % pageSize == 0) ? totalRecord / pageSize : totalRecord / pageSize + 1;
    #endregion

    /// <summary>
    /// 获取绝对路径。支持web相对路径
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string GetAbsolutePath(string path)
    {
        if (string.IsNullOrEmpty(path))
            throw new ArgumentNullException("path参数不能为空。");
        string ret = path;
        // 绝对路径
        if (Path.IsPathRooted(path))
            return path;
        if (path.StartsWith("~/"))
            return Path.Combine(AppContext.BaseDirectory, path.TrimStart("~/").Replace('/', '\\'));
        if (path.StartsWith("/"))
            return Path.Combine(AppContext.BaseDirectory, path.Replace('/', '\\'));
        return Path.Combine(AppContext.BaseDirectory, path.Replace('/', '\\'));
        //return Path.Combine(Environment.CurrentDirectory, path);
    }

    /// <summary>
    /// 是否是Windows系统
    /// </summary>
    /// <returns></returns>
    public static bool IsWindowsOS
        => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
    public static bool IsLinuxOS
        => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);


    #region GetTaskResult
    public static T GetTaskResult<T>(this Task<T> task, bool continueOnCapturedContext = false)
        => task.ConfigureAwait(continueOnCapturedContext).GetAwaiter().GetResult();
    public static void GetTaskResult(this Task task, bool continueOnCapturedContext = false)
        => task.ConfigureAwait(continueOnCapturedContext).GetAwaiter().GetResult();
    public static T GetTaskResult<T>(this ValueTask<T> task, bool continueOnCapturedContext = false)
        => task.ConfigureAwait(continueOnCapturedContext).GetAwaiter().GetResult();
    public static void GetTaskResult(this ValueTask task, bool continueOnCapturedContext = false)
        => task.ConfigureAwait(continueOnCapturedContext).GetAwaiter().GetResult();
    #endregion
}