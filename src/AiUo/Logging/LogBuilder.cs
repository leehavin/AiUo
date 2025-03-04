using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using AiUo.Reflection;

namespace AiUo.Logging;

public class LogBuilder<T> : LogBuilder
{
    public LogBuilder(LogLevel level = LogLevel.Debug)
        : base(level, typeof(T).Name)
    { }
}
/// <summary>
/// 结构化日志构建器
/// </summary>
public class LogBuilder : ILogBuilder
{
    public string CategoryName { get; set; }
    public LogLevel Level { get; set; } = LogLevel.Debug;
    public LogLevel CustomeExceptionLevel { get; set; } = LogLevel.Information;
    public bool IsContext { get; set; }
    public StringBuilder Message { get; set; } = new();
    public ConcurrentQueue<(string key, object value)> Fields = new();
    private List<Exception> _excList = new();
    public Exception Exception { get; set; }
    public CustomException CustomException { get; set; }

    public LogBuilder(string categoryName) : this(LogLevel.Debug, categoryName) { }
    public LogBuilder(LogLevel level = LogLevel.Debug, string categoryName = null)
    {
        Level = level;
        CategoryName = categoryName;
    }

    #region Methods
    public ILogBuilder SetCategoryName(string categoryName)
    {
        CategoryName = categoryName; return this;
    }
    public ILogBuilder SetLevel(LogLevel level)
    {
        Level = level;
        return this;
    }
    public ILogBuilder AddMessage(string msg)
    {
        if (!string.IsNullOrEmpty(msg))
            Message.AppendLine(msg);
        return this;
    }
    /// <summary>
    /// 添加Field
    /// </summary>
    /// <param name="field"></param>
    /// <param name="value">对象将被json序列化</param>
    /// <returns></returns>
    public ILogBuilder AddField(string field, object value)
    {
        Fields.Enqueue((field, value));
        return this;
    }
    public ILogBuilder AddException(Exception ex)
    {
        if (ex == null) return this;
        var lv = LogLevel.Error;
        if (ExceptionUtil.TryGetCustomException(ex, out var exc))
        {
            lv = CustomeExceptionLevel;
            CustomException = exc;
        }
        Level = Level > lv ? Level : lv;
        Exception = ex;
        _excList.Add(ex);
        return this;
    }
    #endregion

    public void Save(bool saveWhenContext)
    {
        if (IsContext && !saveWhenContext)
            return;

        var msgTmpl = string.Empty;
        var args = new List<object>(Fields.Count + 1);
        msgTmpl += "{Message}";
        args.Add(Message.ToString().TrimEnd(Environment.NewLine));
        foreach (var field in Fields)
        {
            if (string.IsNullOrEmpty(field.key))
                continue;
            // 兼容Console输出
            //msgTmpl += $" {{{field.key}}}";
            msgTmpl += $" {{{field.key.Replace('.', '_')}}}";
            var obj = field.value;
            if (obj == null)
            {
                args.Add(null);
            }
            else
            {
                var type = obj.GetType();
                if (ReflectionUtil.IsSimpleType(type))
                    args.Add(obj);
                else
                    args.Add(SerializerUtil.SerializeJson(obj));
            }
        }
        for (int i = 0; i < _excList.Count - 1; i++)
        {
            var ex = _excList[i];
            if (ex == null) continue;
            msgTmpl += $" {{Exception{i}}}";
            args.Add(SerializerUtil.SerializeJsonNet(ex));
        }
        var category = IsContext && string.IsNullOrEmpty(CategoryName)
            ? "ASPNET_CONTEXT" : CategoryName ?? "DEFAULT_LOGBUILDER";
        LogUtil.CreateLogger(category).Log(Level, Exception, msgTmpl, args.ToArray());
    }

    public void Save()
        => Save(false);
}