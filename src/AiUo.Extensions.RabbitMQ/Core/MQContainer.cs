using EasyNetQ;
using EasyNetQ.ConnectionString;
using EasyNetQ.DI;
using RabbitMQ.Client;
using System.Collections.Concurrent;
using System.Reflection;
using AiUo.Configuration;
using AiUo.Reflection;
using AiUo.Collections;

namespace AiUo.Extensions.RabbitMQ;

internal class MQContainer
{
    private RabbitMQSection _section;
    private bool _consumerEnabled;
    private List<Type> _types;

    public List<Assembly> ConsumerAssemblies { get; private set; } = new();

    // 发布和消费bus需要分开
    private readonly ConcurrentDictionary<string, Lazy<IBus>> _pubBusDict = new();
    private readonly ConcurrentDictionary<string, Lazy<IBus>> _subBusDict = new();

    private readonly ConcurrentDictionary<string, IMQConsumer> _consumerDict = new();

    public MQContainer()
    {
        _section = ConfigUtil.GetSection<RabbitMQSection>();
        _consumerEnabled = _section.ConsumerEnabled;
        var allTypes = DIUtil.GetService<IAssemblyContainer>()
            .GetTypes(_section.ConsumerAssemblies
                , _section.AutoLoad
                , "加载配置文件RRabbitMQ:ConsumerAssemblies中项失败。");
        _types = (from t in allTypes
            where t.IsSubclassOfGeneric(typeof(MQSubscribeConsumer<>))
                  || t.IsSubclassOfGeneric(typeof(MQRespondConsumer<,>))
                  || t.IsSubclassOfGeneric(typeof(MQReceiveConsumer))
            select t).ToList();
    }

    #region Init
    public async Task InitAsync()
    {
        Dispose();
        // Bus
        InitBusDict();
        // Consumer
        await InitConsumerDict();
    }
    private void InitBusDict()
    {
        // 解决 No ip address could be resolved
        ConnectionFactory.DefaultAddressFamily = System.Net.Sockets.AddressFamily.InterNetwork;
        var connParser = new ConnectionStringParser();
        foreach (var element in _section.ConnectionStrings.Values)
        {
            if (string.IsNullOrEmpty(element.ConnectionString))
                throw new Exception($"配置文件RabbitMQ:ConnectionStrings:ConnectionString不能为空。Name:{element.Name}");

            // conn
            var conn = connParser.Parse(element.ConnectionString);
            if (element.UseEnvironmentVirtualHost && (string.IsNullOrEmpty(conn.VirtualHost) || conn.VirtualHost == "/"))
                conn.VirtualHost = ConfigUtil.Environment.Name;
            conn.Product = ConfigUtil.Project.ProjectId;
            conn.Platform = ConfigUtil.Service.SID;

            // _busDict
            _pubBusDict.TryAdd(element.Name, CreateBus(conn, element.UseShortNaming));

            // _busDataDict
            if (_consumerEnabled)
            {
                _subBusDict.TryAdd(element.Name, CreateBus(conn, element.UseShortNaming));
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
            var attr = type.GetCustomAttribute<MQConsumerIgnoreAttribute>();
            if (attr != null)
                continue;
            //if (type.IsSubclassOfGeneric(typeof(MQSubscribeConsumer<>)))
            var consumerObj = (IMQConsumer)ReflectionUtil.CreateInstance(type);
            await consumerObj.Register();
            _consumerDict.TryAdd(consumerObj.GetType().FullName, consumerObj);

            if (!dict.Contains(type.Assembly))
                dict.Add(type.Assembly);
        }
        ConsumerAssemblies = dict.ToList();
    }
    #endregion

    #region Method
    public IBus GetPubBus(string connectionStringName = null)
    {
        connectionStringName ??= _section.DefaultConnectionStringName;
        if (!_pubBusDict.TryGetValue(connectionStringName, out var ret))
            throw new Exception($"配置文件RabbitMQ:ConnectionStrings:Name不存在：name={connectionStringName}");
        return ret.Value;
    }
    public IBus GetSubBus(string connectionStringName = null)
    {
        connectionStringName ??= _section.DefaultConnectionStringName;
        if (!_subBusDict.TryGetValue(connectionStringName, out var ret))
            throw new Exception($"配置文件RabbitMQ:ConnectionStrings:Name不存在：name={connectionStringName}");
        return ret.Value;
    }
    private Lazy<IBus> CreateBus(ConnectionConfiguration conn, bool useShortNaming)
    {
        return new Lazy<IBus>(() => RabbitHutch.CreateBus(conn, register =>
        {
            register.EnableSystemTextJson();
            //register.EnableMicrosoftLogging();
            if (!ConfigUtil.Environment.IsProduction && _section.DebugLogEnabled)
                register.EnableConsoleLogger();
            register.Register(typeof(EasyNetQ.Logging.ILogger<>), typeof(MQLogger<>));
            register.Register<IConventions>(c => new MQConventions(c.Resolve<ITypeNameSerializer>(), useShortNaming));
            register.TryRegister<IPubSub, MyPubSub>();
        }));
    }
    public void ReleaseConsumers()
    {
        _consumerDict.Values.ForEach(consumer => consumer.Dispose());
        _consumerDict.Clear();

    }
    public void Dispose()
    {
        _pubBusDict.Values.ForEach(data => data.Value.Dispose());
        _pubBusDict.Clear();

        _subBusDict.Values.ForEach(data => data.Value.Dispose());
        _subBusDict.Clear();
    }
    #endregion
}