namespace AiUo;

public static class MathUtil
{
    /// <summary>
    /// 获取浮点数小数后位数
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static int GetDecimalPlaces(decimal value)
    {
        var tmps = value.ToString().Split('.');
        return tmps.Length == 1 ? 0 : tmps[1].Length;
        //if (tmps.Length == 1)
        //    return 0;
        //var idx = tmps[1].Length - 1;
        //while (tmps[1][idx] == '0' && idx-- > 0) ;
        //return idx + 1;
    }

    /// <summary>
    /// 计算分页总数
    /// </summary>
    /// <param name="totalCount"></param>
    /// <param name="pageSize"></param>
    /// <returns></returns>
    public static int GetPageCount(int totalCount, int pageSize)
    {
        return (totalCount + pageSize - 1) / pageSize;
    }
}