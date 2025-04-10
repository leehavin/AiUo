using AiUo.Extensions.MQTT.Filters;
using AiUo.Extensions.MQTT.Transformers;
using AiUo.Logging;
using System.Collections.Generic;

namespace AiUo.Extensions.MQTT.Pipeline;

/// <summary>
/// MQTT消息处理管道
/// </summary>
/// <typeparam name="TMessage">消息类型</typeparam>
public class MQTTMessagePipeline<TMessage>
    where TMessage : class
{
    private readonly List<IMQTTMessageFilter<TMessage>> _filters = new();
    private readonly List<object> _transformers = new();
    private readonly Func<TMessage, Task> _handler;

    /// <summary>
    /// 创建一个新的MQTT消息处理管道
    /// </summary>
    /// <param name="handler">消息处理函数</param>
    public MQTTMessagePipeline(Func<TMessage, Task> handler)
    {
        _handler = handler ?? throw new ArgumentNullException(nameof(handler));
    }

    /// <summary>
    /// 添加消息过滤器
    /// </summary>
    /// <param name="filter">消息过滤器</param>
    /// <returns>消息处理管道</returns>
    public MQTTMessagePipeline<TMessage> AddFilter(IMQTTMessageFilter<TMessage> filter)
    {
        _filters.Add(filter);
        return this;
    }

    /// <summary>
    /// 添加消息转换器
    /// </summary>
    /// <typeparam name="TOutput">输出消息类型</typeparam>
    /// <param name="transformer">消息转换器</param>
    /// <returns>转换后的消息处理管道</returns>
    public MQTTMessagePipeline<TOutput> AddTransformer<TOutput>(IMQTTMessageTransformer<TMessage, TOutput> transformer)
        where TOutput : class
    {
        var pipeline = new MQTTMessagePipeline<TOutput>(async output =>
        {
            // 这里不需要做任何事情，因为转换后的消息会被新管道处理
            await Task.CompletedTask;
        });

        _transformers.Add(transformer);
        _transformers.Add(pipeline);

        return pipeline;
    }

    /// <summary>
    /// 处理消息
    /// </summary>
    /// <param name="message">消息</param>
    /// <returns>处理任务</returns>
    public async Task ProcessAsync(TMessage message)
    {
        try
        {
            // 应用所有过滤器
            foreach (var filter in _filters)
            {
                if (!filter.Filter(message))
                {
                    // 消息被过滤，不继续处理
                    return;
                }
            }

            // 处理消息
            await _handler(message);

            // 应用所有转换器
            for (int i = 0; i < _transformers.Count; i += 2)
            {
                if (i + 1 < _transformers.Count)
                {
                    var transformer = _transformers[i];
                    var nextPipeline = _transformers[i + 1];

                    // 使用反射调用转换器和下一个管道
                    var transformMethod = transformer.GetType().GetMethod("Transform");
                    var transformed = transformMethod.Invoke(transformer, new[] { message });

                    var processMethod = nextPipeline.GetType().GetMethod("ProcessAsync");
                    var task = (Task)processMethod.Invoke(nextPipeline, new[] { transformed });
                    await task;
                }
            }
        }
        catch (Exception ex)
        {
            LogUtil.Error(ex, "[MQTT] 消息管道处理失败");
        }
    }
}