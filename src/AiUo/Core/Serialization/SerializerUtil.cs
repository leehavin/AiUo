using System;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using AiUo.Core.Serialization;
using AiUo.Serialization;

namespace AiUo;

/// <summary>
/// 序列化辅助类,提供Xml和Json序列化
/// </summary>
public static class SerializerUtil
{
    #region Xml Serializer

    /// <summary>
    /// Xml序列化到bytes
    /// </summary>
    /// <param name="source">序列化对象</param>
    /// <param name="type">序列化对象类型</param>
    /// <returns></returns>
    public static byte[] SerializeXmlToBytes(object source, Type type)
    {
        byte[] ret = null;
        XmlSerializer ser = new XmlSerializer(type);
        using (MemoryStream ms = new MemoryStream())
        {
            ser.Serialize(ms, source);
            ret = ms.ToArray();
        }
        return ret;
    }

    /// <summary>
    /// Xml序列化到bytes
    /// </summary>
    /// <typeparam name="T">序列化对象类型</typeparam>
    /// <param name="source">序列化对象</param>
    /// <returns></returns>
    public static byte[] SerializeXmlToBytes<T>(T source)
        => SerializeXmlToBytes(source, typeof(T));

    /// <summary>
    /// Xml序列化到string
    /// </summary>
    /// <param name="source">序列化对象</param>
    /// <param name="type">序列化对象类型</param>
    /// <param name="encoding">字符集</param>
    /// <returns></returns>
    public static string SerializeXml(object source, Type type, Encoding encoding = null)
        => (encoding ?? Encoding.UTF8).GetString(SerializeXmlToBytes(source, type));

    /// <summary>
    /// Xml序列化string
    /// </summary>
    /// <typeparam name="T">序列化对象类型</typeparam>
    /// <param name="source">序列化对象</param>
    /// <param name="encoding">字符集</param>
    /// <returns></returns>
    public static string SerializeXml<T>(T source, Encoding encoding = null)
        => (encoding ?? Encoding.UTF8).GetString(SerializeXmlToBytes(source, typeof(T)));

    /// <summary>
    /// Xml反序列化从bytes
    /// </summary>
    /// <param name="input">反序列化对象的bytes</param>
    /// <param name="type">反序列化对象类型</param>
    /// <returns></returns>
    public static object DeserializeXmlFromBytes(byte[] input, Type type)
    {
        object ret = null;
        XmlSerializer ser = new XmlSerializer(type);
        using (MemoryStream ms = new MemoryStream(input))
        {
            ret = ser.Deserialize(ms);
        }
        return ret;
    }

    /// <summary>
    /// Xml反序列化从bytes
    /// </summary>
    /// <typeparam name="T">反序列化对象类型</typeparam>
    /// <param name="input">序列化对象</param>
    /// <returns></returns>
    public static T DeserializeXmlFromBytes<T>(byte[] input) where T : class
        => DeserializeXmlFromBytes(input, typeof(T)) as T;

    /// <summary>
    /// Xml反序列化从string
    /// </summary>
    /// <param name="input">可反序列化的字符串</param>
    /// <param name="type">反序列化对象类型</param>
    /// <param name="encoding">字符集</param>
    /// <returns></returns>
    public static object DeserializeXml(string input, Type type, Encoding encoding = null)
        => DeserializeXmlFromBytes((encoding ?? Encoding.UTF8).GetBytes(input), type);

    /// <summary>
    /// Xml反序列化从Stream
    /// </summary>
    /// <param name="input"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public static object DeserializeXml(Stream input, Type type)
        => new XmlSerializer(type).Deserialize(input);

    /// <summary>
    /// Xml反序列化从Stream
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="input"></param>
    /// <returns></returns>
    public static T DeserializeXml<T>(Stream input)
        => (T)DeserializeXml(input, typeof(T));

    /// <summary>
    /// Xml反序列化
    /// </summary>
    /// <typeparam name="T">反序列化对象类型</typeparam>
    /// <param name="input">可反序列化的字符串</param>
    /// <param name="encoding">字符集</param>
    /// <returns></returns>
    public static T DeserializeXml<T>(string input, Encoding encoding = null)
        => (T)DeserializeXmlFromBytes((encoding ?? Encoding.UTF8).GetBytes(input), typeof(T));
    #endregion //Xml Serializer

    #region JSON Serializer 使用 System.Text.Json

    /// <summary>
    /// 设置System.Text.Json的Options
    /// </summary>
    /// <param name="options"></param>
    public static JsonSerializerOptions ConfigJsonOptions(JsonSerializerOptions options)
    {
        // 属性名称不区分大小写
        options.PropertyNameCaseInsensitive = true;
        // 属性名称CamelCase
        options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        // 循环引用
        options.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        // 数字可使用字符串
        //options.NumberHandling = JsonNumberHandling.AllowReadingFromString;

        // 序列化所有语言集(如中文)而不进行转义! Content-Type: application/json; charset=utf-8
        options.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
        // 排除只读属性
        //IgnoreReadOnlyProperties = true; 
        // 排除所有null属性
        //options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        // 允许在JSON中添加注释
        //options.ReadCommentHandling = JsonCommentHandling.Skip;
        // 允许尾随逗号
        //options.AllowTrailingCommas = true;
        // 缩进
        //options.WriteIndented = ConfigUtil.IsDebugEnvironment;
        // 枚举转换成名称
        //options.Converters.Add(new JsonStringEnumConverter());
        // 支持Exception序列化
        options.Converters.Add(new JsonExceptionConverter());
        options.Converters.Add(new JsonCustomExceptionConverter());
        // 支持int[,]
        options.Converters.Add(new TwoDIntArrayConverter());
        return options;
    }

    private static JsonSerializerOptions _jsonOptions;
    /// <summary>
    /// 获取带有默认配置的Options
    /// </summary>
    /// <returns></returns>
    public static JsonSerializerOptions GetJsonOptions()
    {
        return ConfigJsonOptions(new JsonSerializerOptions());
    }
    /// <summary>
    /// 提供与System.Text.Json.JsonSerializer一起使用的选项。
    /// </summary>
    public static JsonSerializerOptions DefaultJsonOptions
    {
        get
        {
            if (_jsonOptions == null)
            {
                _jsonOptions = ConfigJsonOptions(new JsonSerializerOptions());
            }
            return _jsonOptions;
        }
    }
    /// <summary>
    /// 对象json序列化
    /// </summary>
    /// <param name="source"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static string ToJson(this object source, JsonSerializerOptions options = null)
        => SerializeJson(source, options);
    /// <summary>
    /// 序列化JSON对象，对象需要 DataContractAttribute,DataMember,IgnoreDataMember
    /// </summary>
    /// <param name="source">对象</param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static string SerializeJson(object source, JsonSerializerOptions options = null)
        => JsonSerializer.Serialize(source, options ?? DefaultJsonOptions);
    public static async Task<string> SerializeJsonAsync(object source, JsonSerializerOptions options = null)
    {
        using (var stream = new MemoryStream())
        {
            await JsonSerializer.SerializeAsync(stream, source, options ?? DefaultJsonOptions);
            return Encoding.UTF8.GetString(stream.ToArray());
        }
    }

    public static byte[] SerializeJsonToBytes(object source, JsonSerializerOptions options = null)
        => Encoding.UTF8.GetBytes(SerializeJson(source, options));

    public static void SerializeJsonToFile(object source, string file, Encoding encoding = null)
        => File.WriteAllText(file, SerializeJson(source), encoding ?? Encoding.UTF8);

    /// <summary>
    /// 反序列化JSON对象
    /// </summary>
    /// <param name="json">JSON字符串</param>
    /// <param name="type">JSON类型</param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static object DeserializeJson(string json, Type type, JsonSerializerOptions options = null)
        => JsonSerializer.Deserialize(json, type, options ?? DefaultJsonOptions);
    /// <summary>
    /// 反序列化json对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="json"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static T ParseJson<T>(this string json, JsonSerializerOptions options = null)
        => DeserializeJson<T>(json, options);
    /// <summary>
    /// 反序列化JSON对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="json">JSON字符串</param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static T DeserializeJson<T>(string json, JsonSerializerOptions options = null)
        => JsonSerializer.Deserialize<T>(json, options ?? DefaultJsonOptions);
    public static async Task<T> DeserializeJsonAsync<T>(string json, JsonSerializerOptions options = null)
    {
        using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(json)))
        {
            return await JsonSerializer.DeserializeAsync<T>(stream, options ?? DefaultJsonOptions);
        }
    }
    public static object DeserializeJson(byte[] bytes, Type type, JsonSerializerOptions options = null)
        => DeserializeJson(Encoding.UTF8.GetString(bytes), type, options);
    public static object DeserializeJsonFromFile(string file, Type type)
        => JsonSerializer.Deserialize(File.ReadAllText(file), type, DefaultJsonOptions);
    public static T DeserializeJsonFromFile<T>(string file, Encoding encoding = null)
        => JsonSerializer.Deserialize<T>(File.ReadAllText(file, encoding ?? Encoding.UTF8), DefaultJsonOptions);

    /// <summary>
    /// 反序列化JSON对象
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="type"></param>
    /// <param name="options"></param>
    /// <param name="encoding"></param>
    /// <returns></returns>
    public static object DeserializeJson(Stream stream, Type type, JsonSerializerOptions options = null, Encoding encoding = null)
    {
        using (var reader = new StreamReader(stream, encoding))
        {
            var json = reader.ReadToEnd();
            return DeserializeJson(json, type, options);
        }
    }
    #endregion

    #region Json Newtonsoft.Json
    public static Newtonsoft.Json.JsonSerializerSettings ConfigJsonNetSettings(Newtonsoft.Json.JsonSerializerSettings settings)
    {
        //settings.Formatting = ConfigUtil.IsDebugEnvironment
        //    ? Newtonsoft.Json.Formatting.Indented
        //    : Newtonsoft.Json.Formatting.None;
        var resolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver();
        resolver.NamingStrategy.ProcessDictionaryKeys = false;
        settings.ContractResolver = resolver;
        settings.DefaultValueHandling = Newtonsoft.Json.DefaultValueHandling.Include;
        settings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Include;
        return settings;
    }
    private static Newtonsoft.Json.JsonSerializerSettings _defaultJsonNetSettings;
    public static Newtonsoft.Json.JsonSerializerSettings DefaultJsonNetSettings
    {
        get
        {
            if (_defaultJsonNetSettings == null)
            {
                _defaultJsonNetSettings = ConfigJsonNetSettings(new Newtonsoft.Json.JsonSerializerSettings());
            }
            return _defaultJsonNetSettings;
        }
    }
    public static Newtonsoft.Json.JsonSerializerSettings GetJsonNetSettings()
    {
        return ConfigJsonNetSettings(new Newtonsoft.Json.JsonSerializerSettings());
    }
    /// <summary>
    /// Newtonsoft.Json序列化
    /// </summary>
    /// <param name="src"></param>
    /// <param name="settings"></param>
    /// <returns></returns>
    public static string SerializeJsonNet(object src, Newtonsoft.Json.JsonSerializerSettings settings = null)
        => Newtonsoft.Json.JsonConvert.SerializeObject(src, settings ?? DefaultJsonNetSettings);
    public static byte[] SerializeJsonNetToBytes(object src, Newtonsoft.Json.JsonSerializerSettings settings = null)
        => Encoding.UTF8.GetBytes(SerializeJsonNet(src, settings));
    public static void SerializeJsonNetToFile(object src, string toFile, Newtonsoft.Json.JsonSerializerSettings settings = null)
        => File.WriteAllText(toFile, SerializeJsonNet(src, settings));

    public static object DeserializeJsonNet(string json, Type returnType, Newtonsoft.Json.JsonSerializerSettings settings = null)
        => Newtonsoft.Json.JsonConvert.DeserializeObject(json, returnType, settings ?? DefaultJsonNetSettings);

    /// <summary>
    /// Newtonsoft.Json反序列化
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="json"></param>
    /// <param name="settings"></param>
    /// <returns></returns>
    public static T DeserializeJsonNet<T>(string json, Newtonsoft.Json.JsonSerializerSettings settings = null)
        => Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json, settings ?? DefaultJsonNetSettings);

    public static object DeserializeJsonNet(byte[] bytes, Type returnType, Newtonsoft.Json.JsonSerializerSettings settings = null)
        => DeserializeJsonNet(Encoding.UTF8.GetString(bytes), returnType, settings);


    public static T DeserializeJsonNetFromFile<T>(string file, Newtonsoft.Json.JsonSerializerSettings settings = null)
        => Newtonsoft.Json.JsonConvert.DeserializeObject<T>(File.ReadAllText(file), settings ?? DefaultJsonNetSettings);

    #endregion

    /// <summary>
    /// 解析json结构meta数据
    /// </summary>
    /// <param name="jsonMeta"></param>
    /// <returns></returns>
    public static JsonMetaData ParseJsonMeta(string jsonMeta)
        => new JsonMetaData(jsonMeta);
}