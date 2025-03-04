namespace AiUo.ShortId;

/// <summary>
/// 类UUID短唯一ID生成器辅助类
/// 可指定长度和使用的字符，不确保唯一!
/// </summary>
public static class ShortIdUtil
{
    /// <summary>
    /// 生成类UUID短唯一ID
    /// </summary>
    /// <param name="size">长度</param>
    /// <returns></returns>
    public static string Generate(int size = 21)
        => GetGenerator(null).Generate(size);

    /// <summary>
    /// 生成类UUID短唯一ID
    /// </summary>
    /// <param name="options"></param>
    /// <param name="size">长度</param>
    /// <returns></returns>
    public static string Generate(ShortIdOptions options, int size)
        => GetGenerator(options).Generate(size);

    private static ShortIdGenerator GetGenerator(ShortIdOptions options)
    {
        return new ShortIdGenerator(options);
    }
}