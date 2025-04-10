namespace AiUo.Extensions.MQTT.Transformers;

/// <summary>
/// MQTT消息转换器接口
/// </summary>
/// <typeparam name="TInput">输入消息类型</typeparam>
/// <typeparam name="TOutput">输出消息类型</typeparam>
public interface IMQTTMessageTransformer<TInput, TOutput>
    where TInput : class
    where TOutput : class
{
    /// <summary>
    /// 转换消息
    /// </summary>
    /// <param name="input">输入消息</param>
    /// <returns>转换后的消息</returns>
    TOutput Transform(TInput input);
}