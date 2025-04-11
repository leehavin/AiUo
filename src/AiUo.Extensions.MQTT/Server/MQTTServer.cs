using AiUo.Configuration;
using AiUo.Logging;
using MQTTnet;
using MQTTnet.Server;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace AiUo.Extensions.MQTT.Server;

/// <summary>
/// MQTT服务器类，提供MQTT服务端功能
/// </summary>
public class MQTTServer : IDisposable
{
    private readonly MQTTServerOptions _options;
    private MqttServer _server;
    private bool _isStarted;

    /// <summary>
    /// 获取MQTT服务器实例
    /// </summary>
    public MqttServer Server => _server;

    /// <summary>
    /// 服务器是否已启动
    /// </summary>
    public bool IsStarted => _isStarted;

    /// <summary>
    /// 创建一个新的MQTT服务器实例
    /// </summary>
    /// <param name="options">服务器配置选项</param>
    public MQTTServer(MQTTServerOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    /// 启动MQTT服务器
    /// </summary>
    public async Task StartAsync()
    {
        if (_isStarted)
            return;

        try
        {
            var optionsBuilder = new MqttServerOptionsBuilder()
                .WithDefaultEndpoint()
                .WithDefaultEndpointPort(_options.Port);

            // 配置TLS/SSL
            if (_options.UseTls)
            {
                X509Certificate2 certificate = null;

                if (!string.IsNullOrEmpty(_options.CertificatePath))
                {
                    if (string.IsNullOrEmpty(_options.CertificatePassword))
                    {
                        certificate = new X509Certificate2(_options.CertificatePath);
                    }
                    else
                    {
                        certificate = new X509Certificate2(_options.CertificatePath, _options.CertificatePassword);
                    }
                }

                optionsBuilder.WithEncryptedEndpoint()
                    .WithEncryptedEndpointPort(_options.TlsPort)
                    .WithEncryptionSslProtocol(SslProtocols.Tls12);

                if (certificate != null)
                {
                    optionsBuilder.WithEncryptionCertificate(certificate.Export(X509ContentType.Pfx));
                }
            }

            // 配置认证
            if (_options.EnableAuthentication)
            {
                _server.ValidatingConnectionAsync += context =>
                {
                    if (_options.ValidateCredentials(context.UserName, context.Password))
                    {
                        context.ReasonCode = MQTTnet.Protocol.MqttConnectReasonCode.Success;
                    }
                    else
                    {
                        context.ReasonCode = MQTTnet.Protocol.MqttConnectReasonCode.BadUserNameOrPassword;
                    }

                    return Task.CompletedTask;
                }; 
            }

            // 配置订阅授权
            if (_options.EnableSubscriptionAuthorization)
            {
                _server.InterceptingSubscriptionAsync += context =>
                {
                    if (_options.AuthorizeSubscription(context.ClientId, context.TopicFilter.Topic))
                    {
                        context.ProcessSubscription = true;
                    }
                    else
                    {
                        context.ProcessSubscription = false;
                        context.CloseConnection = _options.CloseConnectionOnUnauthorizedSubscription;
                    }

                    return Task.CompletedTask;
                };
            }

            // 配置发布授权
            if (_options.EnablePublishAuthorization)
            {
                _server.InterceptingPublishAsync += context =>
                {
                    if (_options.AuthorizePublish(context.ClientId, context.ApplicationMessage.Topic))
                    {
                        context.ProcessPublish = true;
                    }
                    else
                    {
                        context.ProcessPublish = false;
                        context.CloseConnection = _options.CloseConnectionOnUnauthorizedPublish;
                    }

                    return Task.CompletedTask;
                };
            }

            // 创建服务器实例
            var factory = new MqttServerFactory();
            _server = factory.CreateMqttServer(optionsBuilder.Build());

            // 注册事件处理程序
            _server.ClientConnectedAsync += args =>
            {
                LogUtil.Info("[MQTT Server] 客户端已连接: {ClientId}", args.ClientId);
                return Task.CompletedTask;
            };

            _server.ClientDisconnectedAsync += args =>
            {
                LogUtil.Info("[MQTT Server] 客户端已断开连接: {ClientId}, 原因: {Reason}", args.ClientId, args.DisconnectType);
                return Task.CompletedTask;
            };

            _server.ClientSubscribedTopicAsync += args =>
            {
                LogUtil.Info("[MQTT Server] 客户端已订阅主题: {ClientId}, 主题: {Topic}, QoS: {QoS}",
                    args.ClientId, args.TopicFilter.Topic, args.TopicFilter.QualityOfServiceLevel);
                return Task.CompletedTask;
            };

            _server.ClientUnsubscribedTopicAsync += args =>
            {
                LogUtil.Info("[MQTT Server] 客户端已取消订阅主题: {ClientId}, 主题: {Topic}", args.ClientId, args.TopicFilter);
                return Task.CompletedTask;
            };

            _server.ApplicationMessageNotConsumedAsync += args =>
            {
                LogUtil.Warning("[MQTT Server] 消息未被消费: 主题: {Topic}", args.ApplicationMessage.Topic);
                return Task.CompletedTask;
            };

            // 启动服务器
            await _server.StartAsync();
            _isStarted = true;

            LogUtil.Info("[MQTT Server] 服务器已启动, 端口: {Port}, TLS端口: {TlsPort}", _options.Port, _options.UseTls ? _options.TlsPort.ToString() : "未启用");
        }
        catch (Exception ex)
        {
            LogUtil.Error(ex, "[MQTT Server] 启动服务器时发生错误");
            throw;
        }
    }

    /// <summary>
    /// 停止MQTT服务器
    /// </summary>
    public async Task StopAsync()
    {
        if (!_isStarted || _server == null)
            return;

        try
        {
            await _server.StopAsync();
            _isStarted = false;
            LogUtil.Info("[MQTT Server] 服务器已停止");
        }
        catch (Exception ex)
        {
            LogUtil.Error(ex, "[MQTT Server] 停止服务器时发生错误");
            throw;
        }
    }

    /// <summary>
    /// 发布消息到指定主题
    /// </summary>
    /// <param name="topic">主题</param>
    /// <param name="payload">消息内容</param>
    /// <param name="qosLevel">服务质量等级(0,1,2)</param>
    /// <param name="retain">是否保留消息</param>
    public async Task PublishAsync(string topic, byte[] payload, int qosLevel = 0, bool retain = false)
    {
        if (!_isStarted || _server == null)
            throw new InvalidOperationException("MQTT服务器未启动");

        var message = new MqttApplicationMessageBuilder()
            .WithTopic(topic)
            .WithPayload(payload)
            .WithQualityOfServiceLevel((MQTTnet.Protocol.MqttQualityOfServiceLevel)qosLevel)
            .WithRetainFlag(retain)
            .Build();

        await _server.InjectApplicationMessage(
            new InjectedMqttApplicationMessage(
                new MqttApplicationMessage()
                {
                    Topic = message.Topic,
                    Payload = message.Payload,
                    QualityOfServiceLevel = message.QualityOfServiceLevel,
                    Retain = message.Retain
                }));
    }

    /// <summary>
    /// 发布消息到指定主题
    /// </summary>
    /// <typeparam name="TMessage">消息类型</typeparam>
    /// <param name="message">消息对象</param>
    /// <param name="topic">主题</param>
    /// <param name="qosLevel">服务质量等级(0,1,2)</param>
    /// <param name="retain">是否保留消息</param>
    public async Task PublishAsync<TMessage>(TMessage message, string topic, int qosLevel = 0, bool retain = false)
        where TMessage : class, new()
    {
        var json = System.Text.Json.JsonSerializer.Serialize(message);
        var payload = System.Text.Encoding.UTF8.GetBytes(json);
        await PublishAsync(topic, payload, qosLevel, retain);
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        if (_isStarted && _server != null)
        {
            _server.StopAsync().GetAwaiter().GetResult();
            _isStarted = false;
        }

        _server = null;
    }
}