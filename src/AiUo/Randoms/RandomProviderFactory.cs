using System;
using AiUo.Configuration;
using AiUo.Reflection;

namespace AiUo.Randoms;

/// <summary>
/// RandomProvider工厂方法类
/// </summary>
public class RandomProviderFactory
{
    /// <summary>
    /// 创建RandomProvider
    /// </summary>
    /// <param name="providerName"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static RandomProvider Create(string providerName = null)
    {
        var section = ConfigUtil.GetSection<RandomSection>();
        if (section == null)
            throw new Exception($"配置Random不存在");
        if (!section.Providers.TryGetValue(providerName ?? section.DefaultProviderName, out var element))
            throw new Exception($"配置Random:Providers不存在Name。name:{providerName}");
        return Create(element);
    }
    public static RandomProvider CreateDefaultProvider()
    {
        var section = ConfigUtil.GetSection<RandomSection>();
        return section != null && section.Providers.TryGetValue(section.DefaultProviderName, out var element)
            ? Create(element)
            : new RandomProvider();
    }
    private static RandomProvider Create(RandomProviderElement element)
    {
        IRandomReader reader = string.IsNullOrEmpty(element.RandomType)
            ? new RNGReader()
            : ReflectionUtil.CreateInstance(element.RandomType) as IRandomReader;
        if (reader == null)
            throw new Exception($"配置Random:Providers:[name:{element.Name}]:RandomType错误。randomType: {element.RandomType}");
        var sampling = element.Options.Enabled
            ? new SamplingContainer(element.Options) : null;
        return new RandomProvider(reader, sampling);
    }
}