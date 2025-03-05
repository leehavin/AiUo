using MagicOnion;

namespace AiUo.Extensions.MagicOnion;

/// <summary>
/// MagicOnion服务接口基类
/// </summary>
/// <typeparam name="TService">服务接口类型</typeparam>
public interface IMagicOnionService<TService> : IService<TService>
    where TService : IMagicOnionService<TService>
{
}