using System.Collections.Generic;

namespace AiUo.Data.ORM;

/// <summary>
/// ORM辅助类
/// </summary>
public static class OrmUtil
{
    /// <summary>
    /// JSON序列化主键集合
    /// </summary>
    /// <param name="ids"></param>
    /// <returns></returns>
    public static string SerializeIDS(Dictionary<string, object> ids) 
        => SerializerUtil.SerializeJson(ids);
    /// <summary>
    /// 反序列化主键集合JSON
    /// </summary>
    /// <param name="jsonIds"></param>
    /// <returns></returns>
    public static Dictionary<string, object> DeserializeIDS(string jsonIds)
        => SerializerUtil.DeserializeJson<Dictionary<string, object>>(jsonIds);
}