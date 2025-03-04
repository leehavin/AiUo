using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AiUo.Text;

public static class JsonUtil
{
    /// <summary>
    /// 使用jsonPath查询json，返回json字符串
    /// </summary>
    /// <param name="json"></param>
    /// <param name="jsonPath"></param>
    /// <returns></returns>
    public static string Select(string json, string jsonPath)
    {
        var root = JObject.Parse(json);
        var tokens = root.SelectTokens(jsonPath);
        return JsonConvert.SerializeObject(tokens);
    }
    /// <summary>
    /// 尝试根据jsonPath获取唯一值
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="json"></param>
    /// <param name="jsonPath"></param>
    /// <param name="ret"></param>
    /// <returns></returns>
    public static bool TrySelectSingle<T>(string json, string jsonPath, out T ret)
    {
        var root = JObject.Parse(json);
        var token = root.SelectToken(jsonPath);
        if (token == null)
        {
            ret = default(T);
            return false;
        }
        else
        {
            ret = token.ToObject<string>().To<T>();
            return true;
        }
    }
    public static T SelectObject<T>(string json, string jsonPath)
    {
        var value = Select(json, jsonPath);
        return JsonConvert.DeserializeObject<T>(value);
    }
}