namespace AiUo.Extensions.RabbitMQ;

/// <summary>
/// 发布订阅模式
/// https://www.rabbitmq.com/tutorials
/// </summary>
public enum MQSubscribeMode
{
    /// <summary>
    /// 单Queue负载均衡模式:
    ///     1) 消息被路由到唯一的Queue，并被Queue上绑定的多个Consumer负载轮询消费
    ///     2) 服务启动时根据 Consumer类名(subscriptionId) 创建全局唯一的Queue
    /// </summary>
    OneQueue,
    /// <summary>
    /// SAC模式: 
    ///     1) 消息被路由到唯一的Queue，并被Queue上绑定的多个Consumer中唯一有效的一个消费
    ///     2) 服务启动时根据 Consumer类名(subscriptionId) 创建全局唯一的Queue
    /// </summary>
    SAC,
    /// <summary>
    /// 广播模式:
    ///     1) 消息被同时路由到多个Queue，并被Queue上绑定的单个Consumer消费
    ///     2) 服务启动时根据 Consumer类名+服务唯一标识(subscriptionId) 为每一个服务创建一个Queue
    ///     建议使用redis实现
    /// </summary>
    Broadcast,
}