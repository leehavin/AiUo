using AiUo.Collections;
using AiUo.Configuration;
using AiUo.Extensions.DotNetty.Configuration;
using AiUo.Extensions.DotNetty.Core.DotNetty;
using AiUo.Logging;
using AiUo.Reflection; 
using System.Collections.Concurrent;
using System.Reflection;

namespace AiUo.Extensions.DotNetty;

/// <summary>
/// MQTT容器，管理MQTT客户端和消费者
/// </summary>
internal class MQTTContainer : IDisposable
{
    private MQTTSection _section;
    private bool _consumerEnabled;
    private List<Type> _types;

    /// <summary>
    /// 消费者程序集列表
    /// </summary>
    public List<Assembly> ConsumerAssemblies { get; private set; } = new();

    // 客户端连接池
    private readonly ConcurrentDictionary<string, Lazy<MQTTClient>> _clientDict = new();
    // 消费者字典
    private readonly ConcurrentDictionary<string, IMQTTConsumer> _consumerDict = new();
    // 会话管理器
    private readonly MQTTSessionManager _sessionManager = new();
    // 连接管理器
    private MQTTConnectionManager _connectionManager;

    /// <summary>
    /// 创建MQTT容器
    /// </summary>
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
        
        // 创建连接管理器，配置连接超时和重连参数
        _connectionManager = new MQTTConnectionManager(
            _sessionManager,
            connectionTimeout: TimeSpan.FromMinutes(_section.ConnectionTimeout > 0 ? _section.ConnectionTimeout : 5),
            connectionMonitorInterval: TimeSpan.FromMinutes(_section.ConnectionMonitorInterval > 0 ? _section.ConnectionMonitorInterval : 1),
            maxReconnectAttempts: _section.MaxReconnectAttempts > 0 ? _section.MaxReconnectAttempts : 5,
            reconnectDelay: TimeSpan.FromSeconds(_section.ReconnectDelay > 0 ? _section.ReconnectDelay : 10),
            autoReconnect: _section.AutoReconnect);
        
        // 注册连接状态变更事件
        _connectionManager.ConnectionStateChanged += (sender, args) =>
        {
            LogUtil.Info("MQTT连接状态变更: ConnectionId={ConnectionId}, ClientId={ClientId}, State={State}", 
                args.ConnectionId, args.ClientId, args.IsConnected ? "已连接" : "已断开");
        };
        
        // 注册连接错误事件
        _connectionManager.ConnectionError += (sender, args) =>
        {
            LogUtil.Error(args.Exception, "MQTT连接错误: ConnectionId={ConnectionId}, ClientId={ClientId}", 
                args.ConnectionId, args.ClientId);
        };
    }

    #region Init
    /// <summary>
    /// 初始化MQTT容器
    /// </summary>
    public async Task InitAsync()
    {
        // 释放现有资源
        await DisposeAsync();
        
        // 初始化客户端
        InitClientDict();
        
        // 初始化消费者
        await InitConsumerDict();
        
        LogUtil.Info("MQTT容器初始化完成，已加载{ClientCount}个客户端，{ConsumerCount}个消费者", 
            _clientDict.Count, _consumerDict.Count);
    }

    /// <summary>
    /// 初始化客户端字典
    /// </summary>
    private void InitClientDict()
    {
        foreach (var element in _section.ConnectionStrings.Values)
        {
            if (string.IsNullOrEmpty(element.Server))
                throw new Exception($"配置文件MQTT:ConnectionStrings:Server不能为空。Name:{element.Name}");

            _clientDict.TryAdd(element.Name, CreateClient(element));
            
            // 注册到连接管理器，提供更多连接信息
            var client = _clientDict[element.Name].Value;
            var clientId = element.ClientId ?? Guid.NewGuid().ToString();
            _connectionManager.RegisterConnection(
                element.Name, 
                client, 
                clientId,
                element.Server, 
                element.Port, 
                "MQTT 3.1.1", 
                element.KeepAlivePeriod, 
                element.UseTls);
                
            // 为不同QoS级别的消息配置重传管理器
            if (element.EnableRetransmission)
            {
                var retransmissionManager = new MQTTRetransmissionManager(
                    retransmissionInterval: TimeSpan.FromSeconds(element.RetransmissionInterval > 0 ? element.RetransmissionInterval : 10),
                    maxRetransmissionCount: element.MaxRetransmissionCount > 0 ? element.MaxRetransmissionCount : 5,
                    useExponentialBackoff: element.UseExponentialBackoff);
                    
                // 将重传管理器与连接关联
                _connectionManager.SetRetransmissionManager(element.Name, retransmissionManager);
            }
        }
    }

    /// <summary>
    /// 初始化消费者字典
    /// </summary>
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

            try
            {
                var consumerObj = (IMQTTConsumer)ReflectionUtil.CreateInstance(type);
                await consumerObj.Register();
                _consumerDict.TryAdd(consumerObj.GetType().FullName, consumerObj);

                if (!dict.Contains(type.Assembly))
                    dict.Add(type.Assembly);
                    
                LogUtil.Debug("已注册MQTT消费者: {ConsumerType}", type.FullName);
            }
            catch (Exception ex)
            {
                LogUtil.Error(ex, "注册MQTT消费者失败: {ConsumerType}", type.FullName);
            }
        }
        ConsumerAssemblies = dict.ToList();
    }
    #endregion

    #region Client
    /// <summary>
    /// 创建MQTT客户端
    /// </summary>
    private Lazy<MQTTClient> CreateClient(MQTTConnectionStringElement element)
    {
        return new Lazy<MQTTClient>(() =>
        {
            try
            {
                // 创建MQTT客户端
                var client = new MQTTClient(
                    element.Server,
                    element.Port,
                    element.ClientId ?? Guid.NewGuid().ToString(),
                    element.Username,
                    element.Password,
                    element.CleanSession,
                    element.KeepAlivePeriod,
                    element.UseTls);
                
                // 配置连接重试
                int retryCount = 0;
                int maxRetries = element.MaxConnectRetries > 0 ? element.MaxConnectRetries : 3;
                TimeSpan retryDelay = TimeSpan.FromSeconds(element.ConnectRetryDelay > 0 ? element.ConnectRetryDelay : 5);
                bool connected = false;
                
                while (!connected && retryCount < maxRetries)
                {
                    try
                    {
                        // 尝试连接
                        client.StartAsync().Wait();
                        connected = true;
                        LogUtil.Info("MQTT客户端连接成功: {Server}:{Port}, ClientId={ClientId}", 
                            element.Server, element.Port, client.ClientId);
                    }
                    catch (Exception ex)
                    {
                        retryCount++;
                        if (retryCount >= maxRetries)
                        {
                            LogUtil.Error(ex, "MQTT客户端连接失败，已达到最大重试次数: {Server}:{Port}, ClientId={ClientId}, RetryCount={RetryCount}", 
                                element.Server, element.Port, client.ClientId, retryCount);
                            throw;
                        }
                        
                        LogUtil.Warning("MQTT客户端连接失败，将在{Delay}秒后重试: {Server}:{Port}, ClientId={ClientId}, RetryCount={RetryCount}/{MaxRetries}", 
                            retryDelay.TotalSeconds, element.Server, element.Port, client.ClientId, retryCount, maxRetries);
                        Thread.Sleep(retryDelay);
                        
                        // 使用指数退避策略增加重试延迟
                        if (element.UseExponentialBackoff)
                        {
                            retryDelay = TimeSpan.FromMilliseconds(retryDelay.TotalMilliseconds * 2);
                        }
                    }
                }
                
                return client;
            }
            catch (Exception ex)
            {
                LogUtil.Error(ex, "MQTT客户端创建失败: {Server}:{Port}", element.Server, element.Port);
                throw;
            }
        });
    }

    public IManagedMqttClient GetClient(string connectionStringName = null)
    {
        connectionStringName ??= _section?.DefaultConnectionStringName;
        if (!_clientDict.TryGetValue(connectionStringName, out var client))
            throw new Exception($"配置MQTT:ConnectionStrings中没有此Name: {connectionStringName}");
        return new MQTTClientAdapter(client.Value);
    }
    #endregion

    #region Consumer
    /// <summary>
    /// 释放消费者资源
    /// </summary>
    public void ReleaseConsumers()
    {
        foreach (var consumer in _consumerDict.Values)
        {
            try
            {
                consumer.Unregister();
            }
            catch (Exception ex)
            {
                LogUtil.Error(ex, "注销MQTT消费者失败: {ConsumerType}", consumer.GetType().FullName);
            }
        }
        _consumerDict.Clear();
    }
    #endregion

    /// <summary>
    /// 异步释放资源
    /// </summary>
    public async Task DisposeAsync()
    {
        try
        {
            // 释放消费者
            ReleaseConsumers();
            
            // 释放连接管理器
            if (_connectionManager != null)
            {
                _connectionManager.Dispose();
                _connectionManager = null;
            }
            
            // 释放客户端
            foreach (var client in _clientDict.Values)
            {
                if (client.IsValueCreated)
                {
                    try
                    {
                        await client.Value.StopAsync();
                        client.Value.Dispose();
                    }
                    catch (Exception ex)
                    {
                        LogUtil.Error(ex, "关闭MQTT客户端失败");
                    }
                }
            }
            _clientDict.Clear();
            
            // 清理会话
            _sessionManager.ClearSessions();
            
            LogUtil.Info("MQTT容器资源已释放");
        }
        catch (Exception ex)
        {
            LogUtil.Error(ex, "释放MQTT容器资源时发生错误");
        }
    }
    
    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        DisposeAsync().Wait();
    }
}