using System;
using System.Collections.Generic;
using System.Data;
using AiUo.ConsoleTableExt;

namespace AiUo;

/// <summary>
/// 控制台输出辅助类
/// </summary>
public static class ConsoleUtil
{
    /// <summary>
    /// 输出到控制台，可设置前景色和背景色
    /// </summary>
    /// <param name="value"></param>
    /// <param name="foregroundColor"></param>
    /// <param name="backgroundColor"></param>
    public static void Write(string value, ConsoleColor foregroundColor = ConsoleColor.White, ConsoleColor backgroundColor = ConsoleColor.Black)
    {
        if (foregroundColor != ConsoleColor.White)
            Console.ForegroundColor = foregroundColor;
        if (backgroundColor != ConsoleColor.Black)
            Console.BackgroundColor = backgroundColor;
        Console.Write(value);
        Console.ResetColor();
    }
    /// <summary>
    /// 输出到控制台，可设置前景色和背景色
    /// </summary>
    /// <param name="value"></param>
    /// <param name="foregroundColor"></param>
    /// <param name="backgroundColor"></param>
    public static void WriteLine(string value, ConsoleColor foregroundColor = ConsoleColor.White, ConsoleColor backgroundColor = ConsoleColor.Black)
    {
        Write(value + Environment.NewLine, foregroundColor, backgroundColor);
    }

    #region WriteTable
    public static void WriteTable(List<List<object>> list, List<ConsoleTableFormat> formats = null)
        => ConsoleTableBuilder.From(list).WriteTable(formats);
    public static void WriteTable(List<object[]> list, List<ConsoleTableFormat> formats = null)
        => ConsoleTableBuilder.From(list).WriteTable(formats);
    public static void WriteTable<T>(List<T> list, List<ConsoleTableFormat> formats = null)
        where T : class
        => ConsoleTableBuilder.From(list).WriteTable(formats);
    public static void WriteTable(DataTable dt, List<ConsoleTableFormat> formats = null)
        => ConsoleTableBuilder.From(dt).WriteTable(formats);
    private static void WriteTable(this ConsoleTableBuilder builder, List<ConsoleTableFormat> formats = null)
    {
        if (formats != null)
        {
            var headers = new List<string>();
            var aligns = new Dictionary<int, TextAligntment>();
            var minLengths = new Dictionary<int, int>();
            for (int i = 0; i < formats.Count; i++)
            {
                var item = formats[i];
                headers.Add(item.Header);
                aligns.Add(i, item.TextAlign);
                minLengths.Add(i, item.MinLength);
            }
            builder = builder.WithColumn(headers)
                .WithMinLength(minLengths)
                .WithTextAlignment(aligns);
        }
        builder.WithFormat(ConsoleTableBuilderFormat.MarkDown)
            .ExportAndWriteLine(TableAligntment.Left);
    }
    #endregion
}