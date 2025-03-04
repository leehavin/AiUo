using System.Reflection;
using AiUo.Configuration;
using AiUo.Reflection;

namespace AiUo.Extensions.StackExchangeRedis;

internal class ConsumerContainer
{
    private RedisSection _section;
    private List<Type> _types;

    private readonly List<object> _list = new();
    public bool HasConsumer => _list.Count > 0;
    public List<Assembly> ConsumerAssemblies { get; private set; } = new();

    public ConsumerContainer()
    {
        _section = ConfigUtil.GetSection<RedisSection>();
        var alllTypes = DIUtil.GetService<IAssemblyContainer>()
            .GetTypes(_section.ConsumerAssemblies, _section.AutoLoad, "加载配置文件Redis:ConsumerAssemblies中项失败。");
        _types = (from t in alllTypes
                .Where(x => x.GetCustomAttribute<RedisConsumerRegisterIgnoreAttribute>() == null)
            where t.IsSubclassOfGeneric(typeof(RedisSubscribeConsumer<>))
                  || t.IsSubclassOfGeneric(typeof(RedisQueueConsumer<>))
            select t).ToList();
    }
    public void Init()
    {
        var dict = new HashSet<Assembly>();
        foreach (var type in _types)
        {
            var obj = ReflectionUtil.CreateInstance(type);
            ((IRedisConsumer)obj).Register();
            _list.Add(obj);
            if (!dict.Contains(type.Assembly))
                dict.Add(type.Assembly);
        }
        ConsumerAssemblies = dict.ToList();
    }
}