using AiUo.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;

namespace AiUo.Hosting.Common;

internal class FileConfigBuilder : IExternalConfigBuilder
{
    public IConfiguration Build(IConfiguration fileConfig, IHostBuilder hostBuilder)
    {
        ConfigHost(fileConfig);
        return fileConfig;
    }
    private void ConfigHost(IConfiguration fileConfig)
    {
        var config = fileConfig.GetSection("Host");
        if (config == null)
            return;

        if (ConfigUtil.Service.HttpPort > 0 && ConfigUtil.Service.GrpcPort <= 0)
            ConfigUtil.Service.GrpcPort = ConfigUtil.Service.HttpPort + 10000;
        if (ConfigUtil.Service.HttpPort > 0 && ConfigUtil.Service.WebSocketPort <= 0)
            ConfigUtil.Service.WebSocketPort = ConfigUtil.Service.HttpPort + 20000;

        var section = new HostSection();
        section.Bind(config);
        switch (section.ApiType)
        {
            case HostApiType.Http:
                ConfigUtil.Service.HostPort = ConfigUtil.Service.HttpPort;
                break;
            case HostApiType.Grpc:
                ConfigUtil.Service.HostPort = ConfigUtil.Service.GrpcPort;
                break;
            case HostApiType.WebSocket:
                ConfigUtil.Service.HostPort = ConfigUtil.Service.WebSocketPort;
                break;
        }
        ConfigUtil.Service.HostApiType = section.ApiType;

        //
        if (section.RegisterEnabled && ConfigUtil.Service.HostPort <= 0)
            throw new Exception($"配置文件注册服务时Host:RegisterApiType={section.ApiType.ToString()}但HostPort==0");
    }
}