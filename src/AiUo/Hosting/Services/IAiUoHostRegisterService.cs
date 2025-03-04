using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AiUo.Logging;

namespace AiUo.Hosting.Services;

/// <summary>
/// host注册服务
/// </summary>
public interface IAiUoHostRegisterService
{
    bool RegisterEnabled { get; }
    void AddProvider(IAiUoHostRegisterProvider provider);
    Task Register();
    Task Deregistering();
    Task Deregistered();
    Task Heartbeat();
    Task Health();
}
internal class DefaultHostRegisterService : IAiUoHostRegisterService
{
    private List<IAiUoHostRegisterProvider> _providers = new();

    public bool RegisterEnabled => _providers.Count > 0;

    public void AddProvider(IAiUoHostRegisterProvider provider)
    {
        _providers.Add(provider);
    }
    public async Task Register()
    {
        foreach (var provider in _providers)
        {
            await provider.Register();
        }
    }
    public async Task Deregistering()
    {
        foreach (var provider in _providers)
        {
            await provider.Deregistering();
        }
    }
    public async Task Deregistered()
    {
        foreach (var provider in _providers)
        {
            await provider.Deregistered();
        }
    }
    public async Task Heartbeat()
    {
        foreach (var provider in _providers)
        {
            try
            {
                await provider.Heartbeat();
            }
            catch (Exception ex)
            {
                LogUtil.Error(ex, $"{provider.GetType().FullName}.Heartbeat()异常");
            }
        }
    }
    public async Task Health()
    {
        foreach (var provider in _providers)
        {
            try
            {
                await provider.Health();
            }
            catch (Exception ex)
            {
                LogUtil.Error(ex, $"{provider.GetType().FullName}.Health()异常");
            }
        }
    }
}
public interface IAiUoHostRegisterProvider
{
    Task Register();
    Task Deregistering();
    Task Deregistered();
    Task Heartbeat();
    Task Health();
}