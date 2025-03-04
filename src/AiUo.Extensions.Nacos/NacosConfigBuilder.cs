using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using AiUo.Configuration;
using AiUo.Hosting;
using AiUo.Logging;

namespace AiUo.Extensions.Nacos;

/// <summary>
/// API: https://nacos.io/zh-cn/docs/open-api.html
/// nacos高可用: https://www.cnblogs.com/crazymakercircle/p/15393171.html
/// nacos-sdk-csharp: https://github.com/nacos-group/nacos-sdk-csharp/tree/dev?tab=readme-ov-file#nacos-sdk-csharp-------%E4%B8%AD%E6%96%87
/// </summary>
public class NacosConfigBuilder : IExternalConfigBuilder
{
    public IConfiguration Build(IConfiguration fileConfig, IHostBuilder hostBuilder)
    {
        var nacosConfig = fileConfig.GetSection("Nacos");
        if (nacosConfig == null)
            return null;
        var section = new NacosSection();
        section.Bind(nacosConfig);
        if (!section.Enabled)
            return null;

        SetNacosConfig(section, nacosConfig);
        section = new NacosSection();
        section.Bind(nacosConfig);

        var builder = new ConfigurationBuilder();
        builder.AddEnvironmentVariables();
        builder.AddNacosV2Configuration(nacosConfig);
        builder.AddConfiguration(fileConfig, false);

        var ret = builder.Build(); //请求

        SetReturnConfig(section, ret);
        hostBuilder.ConfigureServices(services =>
        {
            services.AddSingleton(section);
        });
        LogUtil.Info($"配置管理 [加载nacos配置源] ServerAddresses: {string.Join('|', section.ServerAddresses)} Namespace: {section.Namespace} ServiceName:{section.ServiceName}");
        return ret;
    }

    private void SetNacosConfig(NacosSection section, IConfigurationSection nacosConfig)
    {
        // Secure
        ConfigUtil.Service.HostSecure = section.Secure;

        // HostIp
        var envHostIp = ConfigUtil.Service.HostIp;
        if (!string.IsNullOrEmpty(section.Ip))
        {
            if (!string.IsNullOrEmpty(envHostIp) && envHostIp != section.Ip)
                throw new Exception($"Nacos:Ip配置[{section.Ip}]与ConfigUtil.ServiceInfo.HostIp[{envHostIp}]不一致。");
            ConfigUtil.Service.HostIp = section.Ip;
        }
        else
        {
            nacosConfig["Ip"] = envHostIp;
        }

        // HostPort
        if (section.Port <= 0)
        {
            if (ConfigUtil.Service.HttpPort > 0 && ConfigUtil.Service.GrpcPort <= 0)
                ConfigUtil.Service.GrpcPort = ConfigUtil.Service.HttpPort + 10000;
            if (ConfigUtil.Service.HttpPort > 0 && ConfigUtil.Service.WebSocketPort <= 0)
                ConfigUtil.Service.WebSocketPort = ConfigUtil.Service.HttpPort + 20000;
        }
        switch (section.ApiType)
        {
            case HostApiType.Http:
                if (section.Port > 0)
                {
                    if (section.Port != ConfigUtil.Service.HttpPort)
                        throw new Exception($"Nacos配置RegisterApiType=REST，但port与HttpPort不同。httpPort:{ConfigUtil.Service.HttpPort} Nacos:Port:{section.Port}");
                }
                ConfigUtil.Service.HostPort = ConfigUtil.Service.HttpPort;
                break;
            case HostApiType.Grpc:
                if (section.Port > 0)
                {
                    if (ConfigUtil.Service.GrpcPort > 0 && ConfigUtil.Service.GrpcPort != section.Port)
                        throw new Exception($"Nacos配置RegisterApiType=Grpc，但port与GrpcPort不同。grpcPort:{ConfigUtil.Service.GrpcPort} Nacos:Port:{section.Port}");
                }
                ConfigUtil.Service.HostPort = ConfigUtil.Service.GrpcPort;
                break;
            case HostApiType.WebSocket:
                if (section.Port > 0)
                {
                    if (ConfigUtil.Service.WebSocketPort > 0 && ConfigUtil.Service.WebSocketPort != section.Port)
                        throw new Exception($"Nacos配置RegisterApiType=WebSocket，但port与WebSocketPort不同。webSocketPort:{ConfigUtil.Service.WebSocketPort} Nacos:Port:{section.Port}");
                }
                ConfigUtil.Service.HostPort = ConfigUtil.Service.GrpcPort;
                break;
        }
        if (section.RegisterEnabled && ConfigUtil.Service.HostPort <= 0)
            throw new Exception($"Nacos注册服务时Host:RegisterApiType={section.ApiType.ToString()}但HostPort==0");

        ConfigUtil.Service.HostApiType = section.ApiType;
        nacosConfig["Port"] = ConfigUtil.Service.HostPort.ToString();

        AddMetadata(nacosConfig, NacosHostMicroService.HOST_API_TYPE_KEY, ConfigUtil.Service.HostApiType.ToString());
        AddMetadata(nacosConfig, NacosHostMicroService.HOST_URL, ConfigUtil.Service.HostUrl);
    }

    private void SetReturnConfig(NacosSection section, IConfigurationRoot config)
    {
        //Project:ProjectId
        if (!string.IsNullOrEmpty(section.ServiceName))
        {
            var pid = config["Project:ProjectId"];
            if (!string.IsNullOrEmpty(pid) && pid != section.ServiceName)
                LogUtil.Warning($"配置Nacos:ServiceName与ProjectId不相同，同步为ServiceName。serviceName:{section.ServiceName} projectId:{pid}");
            config["Project:ProjectId"] = section.ServiceName;
        }

        // Host:RegisterApiType
        if (!string.IsNullOrEmpty(section.RegisterApiType))
        {
            var apiType = config["Host:RegisterApiType"];
            if (!string.IsNullOrEmpty(apiType) && apiType != section.RegisterApiType)
                LogUtil.Warning($"配置Nacos:RegisterApiType与Host:RegisterApiType不相同，使用nacos配置。nacos:{section.RegisterApiType} host:{apiType}");
            config["Host:RegisterApiType"] = section.RegisterApiType;
        }
    }

    private void AddMetadata(IConfigurationSection config, string key, string value)
    {
        config[$"Metadata:{key}"] = value;
    }
}