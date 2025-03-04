using System;

namespace AiUo;

public static partial class AiUoUtil
{
    #region ThrowIf
    public static void ThrowIfFunc(Func<bool> func, string errorMessage)
    {
        if (func())
            throw new Exception(errorMessage);
    }
    public static void ThrowIfFuncEx(Func<bool> func, string errorCode, string errorMessage)
    {
        if (func())
            throw new CustomException(errorCode, errorMessage);
    }
    public static void ThrowIfNull(string errorMessage, params object[] args)
    {
        if (args == null)
            throw new ArgumentNullException(nameof(args));
        for (int i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            if (arg == null)
                throw new ArgumentNullException($"{errorMessage}。index: {i}");
        }
    }
    public static void ThrowIfNullEx(string errorCode, string errorMessage, params object[] args)
    {
        if (string.IsNullOrEmpty(errorCode))
            throw new Exception("errorCode不能为空");
        if (args == null)
            throw new ArgumentNullException(nameof(args));
        for (int i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            if (arg == null)
                throw new CustomException(errorCode, $"{errorMessage}。index: {i}");
        }
    }
    /// <summary>
    /// 判断字符串不能为null或者空，否则抛出ArgumentNullException
    /// </summary>
    /// <param name="errorMessage"></param>
    /// <param name="args"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public static void ThrowIfNullOrEmpty(string errorMessage, params string[] args)
    {
        if (args == null)
            throw new ArgumentNullException(nameof(args));
        for (int i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            if (string.IsNullOrEmpty(arg))
                throw new ArgumentNullException($"{errorMessage}。index: {i}");
        }
    }
    public static void ThrowIfNullOrEmptyEx(string errorCode, string errorMessage, params string[] args)
    {
        if (string.IsNullOrEmpty(errorCode))
            throw new Exception("errorCode不能为空");
        if (args == null)
            throw new ArgumentNullException(nameof(args));
        for (int i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            if (string.IsNullOrEmpty(arg))
                throw new CustomException(errorCode, $"{errorMessage}。index: {i}");
        }
    }
    #endregion
}