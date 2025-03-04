using AiUo.Configuration;
using AiUo.Reflection;
using AutoMapper;
using System.Reflection;

namespace AiUo.Extensions.AutoMapper;

/// <summary>
/// AutoMapper配置类
/// 需要在Application_Start中配置AutoMapperUtil.Register(Assembly.GetAssembly(typeof(OSC.Models.IAssembly)));
/// 其中Assembly为所有继承IMapFrom和IMapTo的程序集
/// </summary>
public static class AutoMapperUtil
{
    public static MapperConfigurationExpression Expression { get; private set; }
    public static MapperConfiguration Configuration { get; private set; }
    public static IMapper Mapper { get; private set; }

    #region Register
    /// <summary>
    /// 使用配置文件中的配置注册AutoMapper
    /// </summary>
    public static bool Register()
    {
        var section = ConfigUtil.GetSection<AutoMapperSection>();
        var asms = DIUtil.GetService<IAssemblyContainer>().GetAssemblies(section?.Assemblies
            , section?.AutoLoad, "配置文件AutoMapper:Assemblies中不存在。");
        if (asms.Count == 0)
            return false;
        Register(asms);
        return true;
    }

    /// <summary>
    /// 注册AutoMapper，需要在应用程序启动时注册
    /// </summary>
    /// <param name="asm"></param>
    public static void Register(Assembly asm)
    {
        Register(new List<Assembly>() { asm });
    }
    /// <summary>
    /// 注册AutoMapper，需要在应用程序启动时注册
    /// </summary>
    public static void Register(List<Assembly> asms)
    {
        List<Type> types = new List<Type>();
        foreach (var asm in asms)
            types.AddRange(asm.GetExportedTypes());
        Expression = new MapperConfigurationExpression();
        GetMapperConfig(types).Invoke(Expression);
        Configuration = new MapperConfiguration(Expression);
        Configuration.AssertConfigurationIsValid();
        Mapper = Configuration.CreateMapper();
    }
    /// <summary>
    /// 获取映射配置
    /// </summary>
    /// <param name="types"></param>
    /// <returns></returns>
    private static Action<IMapperConfigurationExpression> GetMapperConfig(List<Type> types)
    {
        Action<IMapperConfigurationExpression> config = cfg =>
        {
            foreach (var type in types)
            {
                if (!type.IsClass) continue;
                foreach (var currInterface in type.GetInterfaces())
                {
                    if (!string.IsNullOrEmpty(currInterface.FullName))
                        RegisterInterface(currInterface, type, cfg);
                }
            }
        };
        return config;
    }
    private static void RegisterInterface(Type currInterface, Type type, IMapperConfigurationExpression cfg)
    {
        if (currInterface.FullName.StartsWith("AiUo.Extensions.AutoMapper.IMapTo`"))
        {
            RegisterMapTo(currInterface, type, cfg);
        }
        if (currInterface.FullName.StartsWith("AiUo.Extensions.AutoMapper.IMapFrom`"))
        {
            RegisterMapFrom(currInterface, type, cfg);
        }
    }
    private static void RegisterMapTo(Type currInterface, Type type, IMapperConfigurationExpression cfg)
    {
        foreach (var destType in currInterface.GetGenericArguments())
        {
            Action<object, object> afterFunc;
            afterFunc = (src, dest) =>
            {
                var method = type.GetMethod("MapTo", new Type[] { destType });
                method.Invoke(src, new object[] { dest });
            };
            cfg.CreateMap(type, destType, MemberList.None).AfterMap(afterFunc);
        }
    }
    private static void RegisterMapFrom(Type currInterface, Type type, IMapperConfigurationExpression cfg)
    {
        foreach (var srcType in currInterface.GetGenericArguments())
        {
            Action<object, object> afterFunc;
            afterFunc = (src, dest) =>
            {
                var method = type.GetMethod("MapFrom", new Type[] { srcType });
                method.Invoke(dest, new object[] { src });
            };
            cfg.CreateMap(srcType, type, MemberList.None).AfterMap(afterFunc);
        }
    }
    #endregion

    /// <summary>
    /// 自动映射成目标对象
    /// </summary>
    /// <typeparam name="TDestination"></typeparam>
    /// <param name="source"></param>
    /// <returns></returns>
    public static TDestination Map<TDestination>(this object source)
        => Mapper.Map<TDestination>(source);

    /// <summary>
    /// 自动映射成目标对象
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TDestination"></typeparam>
    /// <param name="source"></param>
    /// <returns></returns>
    public static TDestination Map<TSource, TDestination>(this TSource source)
        => Mapper.Map<TSource, TDestination>(source);
}