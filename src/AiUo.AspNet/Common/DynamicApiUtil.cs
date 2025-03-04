using Microsoft.AspNetCore.Mvc.ApplicationParts;
using System.Runtime.Loader;

namespace AiUo.AspNet.Common;

public static class DynamicApiUtil
{
    private static Dictionary<string, PluginItem> _plugins = new();
    public static bool HasChanged { get; set; }
    public static CancellationTokenSource TokenSource { get; private set; } = new();

    public static void Add(string path, ApplicationPartManager mgr = null)
    {
        path = Path.Combine(AppContext.BaseDirectory, path);
        if (_plugins.ContainsKey(path))
            return;
        mgr = mgr ?? DIUtil.GetRequiredService<ApplicationPartManager>();
        var context = new CollectibleAssemblyLoadContext();
        var plugin = new PluginItem
        {
            Context = context,
        };
        var files = new DirectoryInfo(path).GetFiles();
        foreach (var file in files)
        {
            if (file.Extension.ToLower() != ".dll")
                continue;
            using (var fs = new FileStream(file.FullName, FileMode.Open))
            {
                var assembly = context.LoadFromStream(fs);
                var part = new AssemblyPart(assembly);
                plugin.Parts.Add(part);
                mgr.ApplicationParts.Add(part);
            }
        }
        _plugins.Add(path, plugin);
        HasChanged = true;
        TokenSource.Cancel();
    }
    public static void Remove(string path, ApplicationPartManager mgr = null)
    {
        path = Path.Combine(AppContext.BaseDirectory, path);
        if (!_plugins.ContainsKey(path))
            return;
        mgr = mgr ?? DIUtil.GetRequiredService<ApplicationPartManager>();
        var plugin = _plugins[path];
        for (int i = 0; i < plugin.Parts.Count; i++)
        {
            mgr.ApplicationParts.Remove(plugin.Parts[i]);
            plugin.Parts[i] = null;
        }
        HasChanged = true;
        TokenSource.Cancel();
        plugin.Context.Unload();
        _plugins.Remove(path);
    }
}
internal class PluginItem
{
    public CollectibleAssemblyLoadContext Context { get; set; }
    public List<AssemblyPart> Parts { get; set; } = new List<AssemblyPart>();
}
internal class CollectibleAssemblyLoadContext : AssemblyLoadContext
{
    /// <summary>
    /// 将IsCollectible属性设置为true
    /// </summary>
    public CollectibleAssemblyLoadContext() : base(isCollectible: true)
    {
    }
}