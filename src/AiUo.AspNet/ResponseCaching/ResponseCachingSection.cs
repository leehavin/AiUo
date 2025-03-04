using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace AiUo.Configuration;

/// <summary>
/// http输出缓存设置。暂时不成熟，未实现
/// </summary>
public class ResponseCachingSection : ConfigSection
{
    public override string SectionName => "ResponseCaching";
    /// <summary>
    /// 是否启用缓存策略
    /// </summary>
    public bool ProfileEnabled { get; set; }
    public Dictionary<string, CacheProfile> Profiles { get; set; }
    /// <summary>
    /// 是否启动静态缓存
    /// </summary>
    public bool StaticEnabled { get; set; }
    public ResponseStaticCaching Static { get; set; }
    public override void Bind(IConfiguration configuration)
    {
        base.Bind(configuration);
        Profiles ??= new();
        Static ??= new();
    }
}
public class ResponseStaticCaching
{
    public List<string> Files { get; set; } = new List<string>() { ".png", ".gif", ".jpg", ".jpeg", ".svg", ".webp" };
    public int MaxAge { get; set; } = 600;
    private HashSet<string> _fileDict;
    public HashSet<string> FileDict
    {
        get 
        {
            if (_fileDict == null)
                _fileDict = Files.ToHashSet();
            return _fileDict;
        }
    }
}