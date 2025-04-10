using AiUo.Logging;
using System;
using System.Threading.Tasks;

namespace AiUo.Extensions.MQTT.Exceptions;

/// <summary>
/// MQTT异常处理工具类
/// </summary>
public static class MQTTExceptionHandler
{
    /// <summary>
    /// 尝试执行MQTT操作，并处理可能出现的异常
    /// </summary>
    /// <param name="action">要执行的操作</param>
    /// <param name="operationName">操作名称，用于日志记录</param>
    /// <param name="retryCount">重试次数</param>
    /// <param name="retryDelayMs">重试延迟（毫秒）</param>
    /// <returns>操作是否成功</returns>
    public static bool TryExecute(Action action, string operationName, int retryCount = 3, int retryDelayMs = 1000)
    {
        int attempts = 0;
        while (attempts <= retryCount)
        {
            try
            {
                attempts++;
                action();
                return true;
            }
            catch (MQTTConnectionException ex)
            {
                LogUtil.Error(ex, "[MQTT] 连接异常: {OperationName}, 服务器: {Server}:{Port}, 尝试: {Attempt}/{MaxAttempts}", 
                    operationName, ex.Server, ex.Port, attempts, retryCount + 1);
                
                if (attempts > retryCount)
                    throw;
            }
            catch (MQTTPublishException ex)
            {
                LogUtil.Error(ex, "[MQTT] 发布异常: {OperationName}, 主题: {Topic}, 消息ID: {MessageId}, 尝试: {Attempt}/{MaxAttempts}", 
                    operationName, ex.Topic, ex.MessageId, attempts, retryCount + 1);
                
                if (attempts > retryCount)
                    throw;
            }
            catch (MQTTSubscribeException ex)
            {
                LogUtil.Error(ex, "[MQTT] 订阅异常: {OperationName}, 主题: {Topic}, QoS: {QosLevel}, 尝试: {Attempt}/{MaxAttempts}", 
                    operationName, ex.Topic, ex.QosLevel, attempts, retryCount + 1);
                
                if (attempts > retryCount)
                    throw;
            }
            catch (MQTTException ex)
            {
                LogUtil.Error(ex, "[MQTT] 操作异常: {OperationName}, 尝试: {Attempt}/{MaxAttempts}", 
                    operationName, attempts, retryCount + 1);
                
                if (attempts > retryCount)
                    throw;
            }
            catch (Exception ex)
            {
                LogUtil.Error(ex, "[MQTT] 未预期的异常: {OperationName}, 尝试: {Attempt}/{MaxAttempts}", 
                    operationName, attempts, retryCount + 1);
                
                if (attempts > retryCount)
                    throw;
            }

            // 延迟后重试
            if (attempts <= retryCount)
            {
                Task.Delay(retryDelayMs).Wait();
            }
        }

        return false;
    }

    /// <summary>
    /// 尝试执行MQTT异步操作，并处理可能出现的异常
    /// </summary>
    /// <param name="asyncAction">要执行的异步操作</param>
    /// <param name="operationName">操作名称，用于日志记录</param>
    /// <param name="retryCount">重试次数</param>
    /// <param name="retryDelayMs">重试延迟（毫秒）</param>
    /// <returns>操作是否成功</returns>
    public static async Task<bool> TryExecuteAsync(Func<Task> asyncAction, string operationName, int retryCount = 3, int retryDelayMs = 1000)
    {
        int attempts = 0;
        while (attempts <= retryCount)
        {
            try
            {
                attempts++;
                await asyncAction();
                return true;
            }
            catch (MQTTConnectionException ex)
            {
                LogUtil.Error(ex, "[MQTT] 连接异常: {OperationName}, 服务器: {Server}:{Port}, 尝试: {Attempt}/{MaxAttempts}", 
                    operationName, ex.Server, ex.Port, attempts, retryCount + 1);
                
                if (attempts > retryCount)
                    throw;
            }
            catch (MQTTPublishException ex)
            {
                LogUtil.Error(ex, "[MQTT] 发布异常: {OperationName}, 主题: {Topic}, 消息ID: {MessageId}, 尝试: {Attempt}/{MaxAttempts}", 
                    operationName, ex.Topic, ex.MessageId, attempts, retryCount + 1);
                
                if (attempts > retryCount)
                    throw;
            }
            catch (MQTTSubscribeException ex)
            {
                LogUtil.Error(ex, "[MQTT] 订阅异常: {OperationName}, 主题: {Topic}, QoS: {QosLevel}, 尝试: {Attempt}/{MaxAttempts}", 
                    operationName, ex.Topic, ex.QosLevel, attempts, retryCount + 1);
                
                if (attempts > retryCount)
                    throw;
            }
            catch (MQTTException ex)
            {
                LogUtil.Error(ex, "[MQTT] 操作异常: {OperationName}, 尝试: {Attempt}/{MaxAttempts}", 
                    operationName, attempts, retryCount + 1);
                
                if (attempts > retryCount)
                    throw;
            }
            catch (Exception ex)
            {
                LogUtil.Error(ex, "[MQTT] 未预期的异常: {OperationName}, 尝试: {Attempt}/{MaxAttempts}", 
                    operationName, attempts, retryCount + 1);
                
                if (attempts > retryCount)
                    throw;
            }

            // 延迟后重试
            if (attempts <= retryCount)
            {
                await Task.Delay(retryDelayMs);
            }
        }

        return false;
    }

    /// <summary>
    /// 尝试执行MQTT操作并返回结果，同时处理可能出现的异常
    /// </summary>
    /// <typeparam name="T">返回值类型</typeparam>
    /// <param name="func">要执行的操作</param>
    /// <param name="operationName">操作名称，用于日志记录</param>
    /// <param name="defaultValue">默认返回值，当所有重试都失败时返回</param>
    /// <param name="retryCount">重试次数</param>
    /// <param name="retryDelayMs">重试延迟（毫秒）</param>
    /// <returns>操作结果，如果失败则返回默认值</returns>
    public static T TryExecute<T>(Func<T> func, string operationName, T defaultValue = default, int retryCount = 3, int retryDelayMs = 1000)
    {
        int attempts = 0;
        while (attempts <= retryCount)
        {
            try
            {
                attempts++;
                return func();
            }
            catch (MQTTConnectionException ex)
            {
                LogUtil.Error(ex, "[MQTT] 连接异常: {OperationName}, 服务器: {Server}:{Port}, 尝试: {Attempt}/{MaxAttempts}", 
                    operationName, ex.Server, ex.Port, attempts, retryCount + 1);
                
                if (attempts > retryCount)
                    throw;
            }
            catch (MQTTPublishException ex)
            {
                LogUtil.Error(ex, "[MQTT] 发布异常: {OperationName}, 主题: {Topic}, 消息ID: {MessageId}, 尝试: {Attempt}/{MaxAttempts}", 
                    operationName, ex.Topic, ex.MessageId, attempts, retryCount + 1);
                
                if (attempts > retryCount)
                    throw;
            }
            catch (MQTTSubscribeException ex)
            {
                LogUtil.Error(ex, "[MQTT] 订阅异常: {OperationName}, 主题: {Topic}, QoS: {QosLevel}, 尝试: {Attempt}/{MaxAttempts}", 
                    operationName, ex.Topic, ex.QosLevel, attempts, retryCount + 1);
                
                if (attempts > retryCount)
                    throw;
            }
            catch (MQTTException ex)
            {
                LogUtil.Error(ex, "[MQTT] 操作异常: {OperationName}, 尝试: {Attempt}/{MaxAttempts}", 
                    operationName, attempts, retryCount + 1);
                
                if (attempts > retryCount)
                    throw;
            }
            catch (Exception ex)
            {
                LogUtil.Error(ex, "[MQTT] 未预期的异常: {OperationName}, 尝试: {Attempt}/{MaxAttempts}", 
                    operationName, attempts, retryCount + 1);
                
                if (attempts > retryCount)
                    throw;
            }

            // 延迟后重试
            if (attempts <= retryCount)
            {
                Task.Delay(retryDelayMs).Wait();
            }
        }

        return defaultValue;
    }

    /// <summary>
    /// 尝试执行MQTT异步操作并返回结果，同时处理可能出现的异常
    /// </summary>
    /// <typeparam name="T">返回值类型</typeparam>
    /// <param name="asyncFunc">要执行的异步操作</param>
    /// <param name="operationName">操作名称，用于日志记录</param>
    /// <param name="defaultValue">默认返回值，当所有重试都失败时返回</param>
    /// <param name="retryCount">重试次数</param>
    /// <param name="retryDelayMs">重试延迟（毫秒）</param>
    /// <returns>操作结果，如果失败则返回默认值</returns>
    public static async Task<T> TryExecuteAsync<T>(Func<Task<T>> asyncFunc, string operationName, T defaultValue = default, int retryCount = 3, int retryDelayMs = 1000)
    {
        int attempts = 0;
        while (attempts <= retryCount)
        {
            try
            {
                attempts++;
                return await asyncFunc();
            }
            catch (MQTTConnectionException ex)
            {
                LogUtil.Error(ex, "[MQTT] 连接异常: {OperationName}, 服务器: {Server}:{Port}, 尝试: {Attempt}/{MaxAttempts}", 
                    operationName, ex.Server, ex.Port, attempts, retryCount + 1);
                
                if (attempts > retryCount)
                    throw;
            }
            catch (MQTTPublishException ex)
            {
                LogUtil.Error(ex, "[MQTT] 发布异常: {OperationName}, 主题: {Topic}, 消息ID: {MessageId}, 尝试: {Attempt}/{MaxAttempts}", 
                    operationName, ex.Topic, ex.MessageId, attempts, retryCount + 1);
                
                if (attempts > retryCount)
                    throw;
            }
            catch (MQTTSubscribeException ex)
            {
                LogUtil.Error(ex, "[MQTT] 订阅异常: {OperationName}, 主题: {Topic}, QoS: {QosLevel}, 尝试: {Attempt}/{MaxAttempts}", 
                    operationName, ex.Topic, ex.QosLevel, attempts, retryCount + 1);
                
                if (attempts > retryCount)
                    throw;
            }
            catch (MQTTException ex)
            {
                LogUtil.Error(ex, "[MQTT] 操作异常: {OperationName}, 尝试: {Attempt}/{MaxAttempts}", 
                    operationName, attempts, retryCount + 1);
                
                if (attempts > retryCount)
                    throw;
            }
            catch (Exception ex)
            {
                LogUtil.Error(ex, "[MQTT] 未预期的异常: {OperationName}, 尝试: {Attempt}/{MaxAttempts}", 
                    operationName, attempts, retryCount + 1);
                
                if (attempts > retryCount)
                    throw;
            }

            // 延迟后重试
            if (attempts <= retryCount)
            {
                await Task.Delay(retryDelayMs);
            }
        }

        return defaultValue;
    }
}