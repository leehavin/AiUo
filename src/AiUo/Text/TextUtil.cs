using System.Text;

namespace AiUo.Text;

/// <summary>
/// Text辅助类
/// </summary>
public static class TextUtil
{
    /// <summary>
    /// 添加
    /// </summary>
    /// <param name="sb"></param>
    /// <param name="format"></param>
    /// <param name="args"></param>
    public static StringBuilder AppendFormatLine(this StringBuilder sb, string format, params object[] args)
    {
        var ret = sb.AppendFormat(format, args);
        ret.AppendLine();
        return ret;
    }
}