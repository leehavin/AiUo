using AiUo.Logging;
using Microsoft.Extensions.Hosting;
using System;

namespace AiUo;

public static class AiUoHostExtensions
{
    public static IHost UseAiUo(this IHost host, Func<IServiceProvider, IServiceProvider> func = null)
    {
        DIUtil.InitServiceProvider(host.Services, func);
        LogUtil.Init();
        return host;
    }
}