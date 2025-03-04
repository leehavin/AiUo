using System;
using System.Collections.Generic;

namespace AiUo;

/// <summary>
/// 异常辅助类
/// </summary>
public static class ExceptionUtil
{
    /// <summary>
    /// 沿异常链(InnerException)查找首个TException类型异常
    /// </summary>
    /// <typeparam name="TException"></typeparam>
    /// <param name="ex"></param>
    /// <returns></returns>
    public static TException GetException<TException>(Exception ex)
        where TException : Exception
    {
        while (ex != null && !(ex is TException))
        {
            ex = ex.InnerException;
        }
        return ex as TException;
    }

    /// <summary>
    /// 获取CustomException
    /// </summary>
    /// <param name="ex"></param>
    /// <param name="exc"></param>
    /// <returns></returns>
    public static bool TryGetCustomException(Exception ex, out CustomException exc)
    {
        exc = GetException<CustomException>(ex);
        return exc != null;
    }

    /// <summary>
    /// 获得异常链(InnerException)最初的异常
    /// </summary>
    /// <param name="ex"></param>
    /// <returns></returns>
    public static Exception GetInnerException(Exception ex)
    {
        while (ex.InnerException != null)
        {
            ex = ex.InnerException;
        }
        return ex;
    }
    #region Data
    /// <summary>
    /// 异常添加用户自定义数据，key默认使用exp.Data的当前索引+1
    /// </summary>
    /// <param name="ex">异常</param>
    /// <param name="data">用户数据</param>
    public static void AddData(this Exception ex, object data)
        => ex.Data.Add(ex.Data.Count, data);

    /// <summary>
    /// 序列化异常用户数据Exception.Data=>json
    /// </summary>
    /// <param name="ex"></param>
    /// <returns></returns>
    public static string SerializeUserData(Exception ex)
    {
        return SerializerUtil.SerializeJsonNet(ex.Data);
    }

    /// <summary>
    /// 反序列化异常用户数据
    /// </summary>
    /// <param name="json"></param>
    /// <returns></returns>
    public static Dictionary<object, object> DeserializeUserData(string json)
    {
        return SerializerUtil.DeserializeJson<Dictionary<object, object>>(json);
    }
    #endregion // UserData

    /*
            #region ExceptionStackEntry
            /// <summary>
            /// 分析Exception.StackTrace获得类型化信息
            /// </summary>
            /// <param name="stackTrace"></param>
            /// <returns></returns>
            public static IEnumerable<ExceptionStackEntry> GetStackEntries(string stackTrace)
            {
                string[] lines = stackTrace.Split(new string[] { " 在 ", " at " }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string line in lines)
                {
                    string currLine = line.Trim();
                    if (string.IsNullOrEmpty(currLine)) continue;
                    string[] entries = currLine.Split(new string[] { " 位置 ", ":行号 ", " in ", ":line " }, StringSplitOptions.RemoveEmptyEntries);
                    if (entries.Length != 3) continue;

                    yield return new ExceptionStackEntry()
                    {
                        MethodName = entries[0].Trim(),
                        FileName = entries[1].Trim(),
                        LineNumber = int.Parse(entries[2].Trim())
                    };
                }
            }

            /// <summary>
            /// 获取异常的堆栈信息集合
            /// </summary>
            /// <param name="ex">抛出的异常</param>
            /// <returns></returns>
            public static IEnumerable<ExceptionStackEntry> GetStackEntries(Exception ex)
            {
                foreach (StackFrame frame in new StackTrace(ex, true).GetFrames())
                {
                    yield return GetStackEntry(frame);
                }
            }

            /// <summary>
            /// 获取异常的堆栈信息
            /// </summary>
            /// <param name="frame">堆栈中的一个函数调用</param>
            /// <returns></returns>
            internal static ExceptionStackEntry GetStackEntry(StackFrame frame)
            {
                System.Reflection.MethodBase method = frame.GetMethod();
                ExceptionStackEntry ret = new ExceptionStackEntry
                {
                    AssemblyName = method.Module.Name,
                    ClassName = (method.ReflectedType != null) ? method.ReflectedType.FullName : null,
                    MethodName = method.ToString(),
                    FileName = frame.GetFileName(),
                    LineNumber = frame.GetFileLineNumber(),
                    IsGlobalAssembly = method.Module.Assembly.GlobalAssemblyCache
                };
                return ret;
            }

            #endregion

            /// <summary>
            /// 获得第一个抛出的异常
            /// </summary>
            /// <param name="ex">异常</param>
            /// <returns></returns>
            public static Exception GetFirstException(Exception ex)
            {
                Exception ret = ex;
                while (ret.InnerException != null)
                {
                    ret = ret.InnerException;
                }
                return ret;
            }
        */
}