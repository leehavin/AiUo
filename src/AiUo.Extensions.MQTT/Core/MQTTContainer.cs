using AiUo.Collections;
using AiUo.Configuration;
using AiUo.Logging;
using AiUo.Reflection;
using MQTTnet;
using System.Collections.Concurrent;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;

namespace AiUo.Extensions.MQTT;

internal class MQTTContainer : IDisposable
{
    private readonly MQTTSection _section;
    private readonly bool _consumerEnabled;
    private readonly List<Type> _types;

    public List<Assembly> ConsumerAssemblies { get; private set; } = new();

    // 发布和消费客户端字典
    private readonly ConcurrentDictionary<string, Lazy<IMqttClient>> _publisherDict = new();
    private readonly ConcurrentDictionary<string, Lazy<IMqttClient>> _subscriberDict = new();

    // 消费者字典
    private readonly ConcurrentDictionary<string, IMQTTConsumer> _consumerDict = new();

    public MQTTContainer()
    {
        _section = ConfigUtil.GetSection<MQTTSection>();
        _consumerEnabled = _section.ConsumerEnabled;
        var allTypes = DIUtil.GetService<IAssemblyContainer>()
            .GetTypes(_section.ConsumerAssemblies
                , _section.AutoLoad
                , "加载配置文件MQTT:ConsumerAssemblies中项失败。");
        _types = (from t in allTypes
            where t.IsSubclassOfGeneric(typeof(MQTTSubscribeConsumer<>))
            select t).ToList();
    }

    #region Init
    public async Task InitAsync()
    {
        Dispose();
        // 初始化客户端
        InitClientDict();
        // 初始化消费者
        await InitConsumerDict();
    }

    private void InitClientDict()
    {
        foreach (var element in _section.ConnectionStrings.Values)
        {
            if (string.IsNullOrEmpty(element.Server))
                throw new Exception($"配置文件MQTT:ConnectionStrings:Server不能为空。Name:{element.Name}");

            // 创建发布客户端
            _publisherDict.TryAdd(element.Name, CreateMqttClient(element));

            // 创建订阅客户端
            if (_consumerEnabled)
            {
                _subscriberDict.TryAdd(element.Name, CreateMqttClient(element));
            }
        }
    }

    private async Task InitConsumerDict()
    {
        if (!_consumerEnabled)
            return;

        var dict = new HashSet<Assembly>();
        foreach (var type in _types)
        {
            var attr = type.GetCustomAttribute<MQTTConsumerIgnoreAttribute>();
            if (attr != null)
                continue;

            var consumerObj = (IMQTTConsumer)ReflectionUtil.CreateInstance(type);
            await consumerObj.Register();
            _consumerDict.TryAdd(consumerObj.GetType().FullName, consumerObj);

            if (!dict.Contains(type.Assembly))
                dict.Add(type.Assembly);
        }
        ConsumerAssemblies = dict.ToList();
    }
    #endregion

    #region Methods
    public IMqttClient GetPublishClient(string connectionStringName = null)
    {
        connectionStringName ??= _section.DefaultConnectionStringName;
        if (!_publisherDict.TryGetValue(connectionStringName, out var ret))
            throw new Exception($"配置文件MQTT:ConnectionStrings:Name不存在：name={connectionStringName}");
        return ret.Value;
    }

    public IMqttClient GetSubscribeClient(string connectionStringName = null)
    {
        connectionStringName ??= _section.DefaultConnectionStringName;
        if (!_subscriberDict.TryGetValue(connectionStringName, out var ret))
            throw new Exception($"配置文件MQTT:ConnectionStrings:Name不存在：name={connectionStringName}");
        return ret.Value;
    }

    private Lazy<IMqttClient> CreateMqttClient(MQTTConnectionStringElement config)
    {
        return new Lazy<IMqttClient>(() =>
        {
            var factory = new MqttClientFactory();
            var client = factory.CreateMqttClient();

            var clientId = string.IsNullOrEmpty(config.ClientId)
                ? $"{ConfigUtil.Project.ProjectId}-{Guid.NewGuid()}"
                : config.ClientId;

            // 创建MQTT客户端选项
            var clientOptionsBuilder = new MqttClientOptionsBuilder()
                .WithClientId(clientId)
                .WithTcpServer(config.Server, config.Port)
                .WithCleanStart(config.CleanSession);

            // 设置保活时间
            if (config.KeepAlivePeriod > 0)
            {
                clientOptionsBuilder.WithKeepAlive(TimeSpan.FromSeconds(config.KeepAlivePeriod));
            }

            // 添加用户名密码认证
            if (!string.IsNullOrEmpty(config.Username))
            {
                clientOptionsBuilder.WithCredentials(
                    config.Username,
                    config.Password != null ? System.Text.Encoding.UTF8.GetBytes(config.Password) : null
                );
            }

            // 添加TLS/SSL支持
            if (config.UseTls)
            {
                clientOptionsBuilder.WithTlsOptions(new MqttClientTlsOptions
                {
                    AllowUntrustedCertificates = config.AllowUntrustedCertificates,
                    IgnoreCertificateChainErrors = config.IgnoreCertificateChainErrors,
                    IgnoreCertificateRevocationErrors = config.IgnoreCertificateRevocationErrors,
                    CertificateValidationHandler = context =>
                    {
                        // 添加服务器证书指纹验证
                        if (!string.IsNullOrEmpty(config.ServerCertificateFingerprint) &&
                            context.Certificate != null && context.Chain != null)
                        {
                            // 获取证书指纹
                            var fingerprint = BitConverter.ToString(context.Certificate.GetCertHash()).Replace("-", "").ToLower();
                            return string.Equals(fingerprint, config.ServerCertificateFingerprint.ToLower());
                        }
                        return true;
                    },
                     
                    Certificates = !string.IsNullOrEmpty(config.ClientCertificatePath) ?
                        GetClientCertificates(config.ClientCertificatePath, config.ClientCertificatePassword) :
                        null
                });
            }

            var options = clientOptionsBuilder.Build();

            // 连接客户端
            client.ConnectAsync(options, CancellationToken.None).Wait();

            // 添加日志
            if (_section.DebugLogEnabled)
            {
                client.ConnectedAsync += e =>
                {
                    LogUtil.Info("[MQTT] 已连接到服务器 {Server}:{Port}", config.Server, config.Port);
                    return Task.CompletedTask;
                };

                client.DisconnectedAsync += e =>
                {
                    LogUtil.Info("[MQTT] 已断开连接，原因: {Reason}", e.ReasonString);
                    return Task.CompletedTask;
                };
            }

            return client;
        });
    }

    private List<X509Certificate> GetClientCertificates(string certificatePath, string password)
    {
        try
        {
            var certificate = new X509Certificate2(certificatePath, password);
            return new List<X509Certificate> { certificate };
        }
        catch (Exception ex)
        {
            LogUtil.Error(ex, "[MQTT] 加载客户端证书失败: {CertificatePath}", certificatePath);
            return null;
        }
    }

    public void ReleaseConsumers()
    {
        _consumerDict.Values.ForEach(consumer => consumer.Dispose());
        _consumerDict.Clear();
    }

    public void Dispose()
    {
        _publisherDict.Values.ForEach(client => client.Value.Dispose());
        _publisherDict.Clear();

        _subscriberDict.Values.ForEach(client => client.Value.Dispose());
        _subscriberDict.Clear();
    }
    #endregion
}