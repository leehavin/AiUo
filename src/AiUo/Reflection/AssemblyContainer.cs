using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using AiUo.Logging;
using AiUo.Collections;

namespace AiUo.Reflection;

public interface IAssemblyContainer
{
    List<Assembly> GetOwnAssemblies();
    Assembly GetAssembly(string assemblyName, string errorMsg = null);
    List<Assembly> GetAssemblies(List<string> asms, bool? auto, string errorMsg = null);
    List<Type> GetTypes(List<string> asms, bool? auto, string errorMsg = null);
}

public class AssemblyContainer : IAssemblyContainer
{
    private ConcurrentDictionary<string, Assembly> _ownDict = new();
    // key: xxx 小写
    private ConcurrentDictionary<string, Assembly> _domainDict = new();
    public AssemblyContainer()
    {
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            var name = asm.GetName();
            var key = name.Name.ToLower();
            if (!_domainDict.TryAdd(key, asm))
                throw new Exception($"AssemblyContainer初始化时应用程序集名称相同。name: {name.Name}");
            var token = name.GetPublicKeyToken();
            if (token == null || token.Length == 0)
            {
                _ownDict.TryAdd(key, asm);
            }
        }
    }
    public List<Assembly> GetOwnAssemblies()
    {
        return _ownDict.Values.ToList();
    }

    public List<Type> GetTypes(List<string> asms, bool? auto, string errorMsg = null)
    {
        var ret = new List<Type>();
        var assemblies = GetAssemblies(asms, auto, errorMsg);
        assemblies.ForEach(asm =>
        {
            ret.AddRange(asm.GetExportedTypes());
        });
        return ret;
    }
    public List<Assembly> GetAssemblies(List<string> asms, bool? auto, string errorMsg = null)
    {
        var ret = new Dictionary<string, Assembly>();
        if (auto.HasValue && auto.Value)
            _ownDict.ForEach(x => ret.TryAdd(x.Key, x.Value));

        if (asms?.Count > 0)
        {
            foreach (var name in asms)
            {
                if (string.IsNullOrEmpty(name))
                    continue;
                var asm = GetAssembly(name, errorMsg);
                if (asm == null)
                    continue;
                var key = asm.GetName().Name.ToLower();
                ret.TryAdd(key, asm);
            }
        }
        return ret.Values.ToList();
    }

    public Assembly GetAssembly(string assemblyName, string errorMsg = null)
    {
        if (string.IsNullOrEmpty(assemblyName))
            throw new ArgumentNullException(nameof(assemblyName), $"获取Assembly时assemblyName不能为空");

        var key = Path.GetFileNameWithoutExtension(assemblyName).ToLower();
        if (_domainDict.TryGetValue(key, out var ret))
            return ret;

        var ignore = !StringUtil.LetterChars.Contains(assemblyName[0]);//非字母开头，可忽略
        var file = ignore ? assemblyName.Substring(1) : assemblyName;
        if (!file.EndsWith(".dll"))
            file += ".dll";

        if (!File.Exists(file))
            file = Path.Combine(AppContext.BaseDirectory, file);
        if (!File.Exists(file))
        {
            var errMsg = $"未能获取Assembly。name:{assemblyName} file:{file} {errorMsg}";
            if (!ignore)
                throw new Exception(errMsg);
            LogUtil.GetContextLogger()
                .SetLevel(Microsoft.Extensions.Logging.LogLevel.Warning)
                .AddField("AssemblyContainer.Warning", errMsg)
                .Save();
            return null;
        }
        ret = Assembly.LoadFrom(file);
        _domainDict.TryAdd(key, ret);
        return ret;
    }
}