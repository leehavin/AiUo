using Microsoft.Extensions.Configuration;
using SqlSugar;
using AiUo.Collections;
using AiUo.Data.SqlSugar;

namespace AiUo.Configuration;

public class SqlSugarSection : ConfigSection
{
    public override string SectionName => "SqlSugar";
    public bool Enabled { get; set; } = true;
    /// <summary>
    /// 连接配置提供者
    /// </summary>
    public string DbConfigProvider { get; set; }
    /// <summary>
    /// 数据分库分表提供者
    /// </summary>
    public string DbSplitProvider { get; set; }

    public string DefaultConnectionStringName { get; set; }

    public Dictionary<string, ConnectionElement> ConnectionStrings = new();

    public Dictionary<string, string> NamespaceMappings = new();

    public override void Bind(IConfiguration configuration)
    {
        base.Bind(configuration);
        ConnectionStrings = configuration.GetSection("ConnectionStrings")
            .Get<Dictionary<string, ConnectionElement>>() ?? new();
        if (ConnectionStrings.Count == 0)
            return;

        // NamespaceMappings
        ConnectionStrings.ForEach(x =>
        {
            x.Value.ConfigId = x.Key;
            x.Value.LanguageType = LanguageType.Chinese;
            x.Value.IsAutoCloseConnection = true;
            if (!string.IsNullOrEmpty(x.Value.MappingNamespaces))
            {
                var ns = x.Value.MappingNamespaces.Split(new char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                ns.ForEach(n => NamespaceMappings.TryAdd(n, x.Key));
            }
        });

        // DefaultConnectionStringName
        if (string.IsNullOrEmpty(DefaultConnectionStringName))
        {
            DefaultConnectionStringName = ConnectionStrings.Count == 1
                ? ConnectionStrings.First().Key
                : throw new Exception("配置SqlSugar:ConnectionStrings中存在多个但没有配置DefaultConnectionStringName");
        }
        else
        {
            if (!ConnectionStrings.ContainsKey(DefaultConnectionStringName))
                throw new Exception($"配置SqlSugar:DefaultConnectionStringName在ConnectionStrings中不存在。DefaultConnectionStringName:{DefaultConnectionStringName}");
        }
    }
}