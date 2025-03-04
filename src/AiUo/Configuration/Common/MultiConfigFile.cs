using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Linq;

namespace AiUo.Configuration;

/// <summary>
/// 配置文件中包含多个配置节点，可分别保存删除更新
/// </summary>
public class MultiConfigFile
{
    public string ConfigFile { get; private set; }
    public JObject ConfigObject { get; private set; }
    public MultiConfigFile(string configFile)
    {
        ConfigFile = configFile;
        if (File.Exists(ConfigFile))
        {
            using (var reader = new JsonTextReader(new StreamReader(ConfigFile)))
            {
                ConfigObject = (JObject)JToken.ReadFrom(reader);
            }
        }
        else
        {
            ConfigObject = new JObject();
        }
    }
    public bool ExistsConfigFile => File.Exists(ConfigFile);
    public bool ExistsSection(string sectionName)
        => ConfigObject[sectionName] != null;
    public bool TryGetSection<T>(string sectionName, out T ret)
        where T : class
    {
        ret = GetSection<T>(sectionName);
        return ret != null;
    }
    /// <summary>
    /// 获取配置
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="name"></param>
    /// <returns></returns>
    public T GetSection<T>(string name)
        where T : class
    {
        var token = ConfigObject[name];
        return token == null ? null : token.ToObject<T>();
    }
    /// <summary>
    /// 设置配置
    /// </summary>
    /// <param name="name"></param>
    /// <param name="obj"></param>
    /// <param name="notWriteNull">true：null时不写 </param>
    public MultiConfigFile SetSection(string name, object obj, bool notWriteNull = false)
    {
        var config = JToken.FromObject(obj);
        if (config.Count() == 0 && notWriteNull)
            return this;
        ConfigObject[name] = config;
        return this;
    }
    /// <summary>
    /// 保存文件
    /// </summary>
    public void Save()
    {
        using (var writer = new JsonTextWriter(new StreamWriter(ConfigFile)))
        {
            writer.Formatting = Formatting.Indented;
            ConfigObject.WriteTo(writer);
        }
    }
    public override string ToString()
    {
        return ExistsConfigFile ? File.ReadAllText(ConfigFile) : string.Empty;
    }
}