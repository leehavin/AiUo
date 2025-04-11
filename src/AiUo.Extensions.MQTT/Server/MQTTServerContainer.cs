using AiUo.Configuration;
using AiUo.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AiUo.Extensions.MQTT.Server;

/// <summary>
/// MQTT服务器容器，用于管理MQTT服务器实例
/// </summary>
public class MQTTServerContainer : IDisposable
{
    private readonly MQTTSection _section;
    private MQTTServer _server;
    private bool _isInitialized;

    /// <summary>
    /// 获取MQTT服务器实例
    /// </summary>
    public MQTTServer Server => _server;

    /// <summary>
    /// 服务器是否已初始化
    /// </summary>
    public bool IsInitialized => _isInitialized;

    /// <summary>
    /// 创建一个新的MQTT服务器容器实例
    /// </summary>
    public MQTTServerContainer()
    {
        _section = ConfigUtil.GetSection<MQTTSection>();
    }

    /// <summary>
    /// 初始化MQTT服务器
    /// </summary>
    public async Task InitAsync()
    {
        if (_isInitialized || _server != null)
            return;

        if (_section?.Server == null || !_section.Server.Enabled)
            return;

        try
        {
            // 创建服务器选项
            var options = new MQTTServerOptions
            {
                Port = _section.Server.Port,
                UseTls = _section.Server.UseTls,
                TlsPort = _section.Server.TlsPort,
                CertificatePath = _section.Server.CertificatePath,
                CertificatePassword = _section.Server.CertificatePassword,
                EnableAuthentication = _section.Server.EnableAuthentication,
                EnableSubscriptionAuthorization = _section.Server.EnableSubscriptionAuthorization,
                EnablePublishAuthorization = _section.Server.EnablePublishAuthorization,
                CloseConnectionOnUnauthorizedSubscription = _section.Server.CloseConnectionOnUnauthorizedSubscription,
                CloseConnectionOnUnauthorizedPublish = _section.Server.CloseConnectionOnUnauthorizedPublish,
                MaximumConnections = _section.Server.MaximumConnections
            };

            // 解析凭据
            if (_section.Server.EnableAuthentication && _section.Server.Credentials != null)
            {
                foreach (var credential in _section.Server.Credentials)
                {
                    var parts = credential.Split(':');
                    if (parts.Length == 2)
                    {
                        options.Credentials[parts[0]] = parts[1];
                    }
                }
            }

            // 解析主题访问控制
            if ((_section.Server.EnableSubscriptionAuthorization || _section.Server.EnablePublishAuthorization) && 
                _section.Server.TopicAccessControl != null)
            {
                foreach (var control in _section.Server.TopicAccessControl)
                {
                    var parts = control.Split(':');
                    if (parts.Length == 2)
                    {
                        var clientId = parts[0];
                        var topics = parts[1].Split(',');
                        options.TopicAccessControl[clientId] = new List<string>(topics);
                    }
                }
            }

            // 创建并启动服务器
            _server = new MQTTServer(options);
            await _server.StartAsync();
            _isInitialized = true;

            LogUtil.Info("[MQTT Server Container] MQTT服务器已初始化并启动");
        }
        catch (Exception ex)
        {
            LogUtil.Error(ex, "[MQTT Server Container] 初始化MQTT服务器时发生错误");
            throw;
        }
    }

    /// <summary>
    /// 停止MQTT服务器
    /// </summary>
    public async Task StopAsync()
    {
        if (!_isInitialized || _server == null)
            return;

        try
        {
            await _server.StopAsync();
            _isInitialized = false;
            LogUtil.Info("[MQTT Server Container] MQTT服务器已停止");
        }
        catch (Exception ex)
        {
            LogUtil.Error(ex, "[MQTT Server Container] 停止MQTT服务器时发生错误");
            throw;
        }
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        if (_server != null)
        {
            _server.Dispose();
            _server = null;
            _isInitialized = false;
        }
    }
}