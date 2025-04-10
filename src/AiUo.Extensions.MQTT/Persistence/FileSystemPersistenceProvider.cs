using AiUo.Logging;
using System.Collections.Concurrent;
using System.Text.Json;

namespace AiUo.Extensions.MQTT.Persistence;

/// <summary>
/// 基于文件系统的MQTT消息持久化提供程序
/// </summary>
public class FileSystemPersistenceProvider : IMQTTPersistenceProvider
{
    private readonly string _storageDirectory;
    private readonly ConcurrentDictionary<string, MQTTPersistedMessage> _cache = new();
    private readonly SemaphoreSlim _lock = new(1, 1);
    private bool _isLoaded = false;

    /// <summary>
    /// 创建一个新的文件系统持久化提供程序
    /// </summary>
    /// <param name="storageDirectory">存储目录</param>
    public FileSystemPersistenceProvider(string storageDirectory)
    {
        _storageDirectory = storageDirectory ?? throw new ArgumentNullException(nameof(storageDirectory));
        Directory.CreateDirectory(_storageDirectory);
    }

    /// <summary>
    /// 加载缓存
    /// </summary>
    private async Task EnsureCacheLoadedAsync()
    {
        if (_isLoaded)
            return;

        await _lock.WaitAsync();
        try
        {
            if (_isLoaded)
                return;

            var files = Directory.GetFiles(_storageDirectory, "*.msg");
            foreach (var file in files)
            {
                try
                {
                    var json = await File.ReadAllTextAsync(file);
                    var message = JsonSerializer.Deserialize<MQTTPersistedMessage>(json);
                    if (message != null)
                    {
                        _cache[message.MessageId] = message;
                    }
                }
                catch (Exception ex)
                {
                    LogUtil.Error(ex, "[MQTT] 加载持久化消息失败: {FilePath}", file);
                }
            }

            _isLoaded = true;
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// 保存消息
    /// </summary>
    public async Task<bool> SaveMessageAsync(string topic, byte[] payload, int qosLevel, bool retain, string messageId)
    {
        await EnsureCacheLoadedAsync();

        var message = new MQTTPersistedMessage
        {
            MessageId = messageId,
            Topic = topic,
            Payload = payload,
            QosLevel = qosLevel,
            Retain = retain,
            CreatedAt = DateTime.UtcNow,
            IsAcknowledged = false
        };

        _cache[messageId] = message;

        try
        {
            var filePath = Path.Combine(_storageDirectory, $"{messageId}.msg");
            var json = JsonSerializer.Serialize(message);
            await File.WriteAllTextAsync(filePath, json);
            return true;
        }
        catch (Exception ex)
        {
            LogUtil.Error(ex, "[MQTT] 保存持久化消息失败: {MessageId}", messageId);
            return false;
        }
    }

    /// <summary>
    /// 标记消息为已确认
    /// </summary>
    public async Task<bool> MarkMessageAcknowledgedAsync(string messageId)
    {
        await EnsureCacheLoadedAsync();

        if (!_cache.TryGetValue(messageId, out var message))
            return false;

        message.IsAcknowledged = true;
        message.AcknowledgedAt = DateTime.UtcNow;

        try
        {
            var filePath = Path.Combine(_storageDirectory, $"{messageId}.msg");
            var json = JsonSerializer.Serialize(message);
            await File.WriteAllTextAsync(filePath, json);
            return true;
        }
        catch (Exception ex)
        {
            LogUtil.Error(ex, "[MQTT] 更新持久化消息状态失败: {MessageId}", messageId);
            return false;
        }
    }

    /// <summary>
    /// 获取未确认的消息
    /// </summary>
    public async Task<IEnumerable<MQTTPersistedMessage>> GetPendingMessagesAsync()
    {
        await EnsureCacheLoadedAsync();
        return _cache.Values.Where(m => !m.IsAcknowledged).ToList();
    }

    /// <summary>
    /// 清理已确认的消息
    /// </summary>
    public async Task<int> CleanAcknowledgedMessagesAsync(DateTime olderThan)
    {
        await EnsureCacheLoadedAsync();

        var messagesToRemove = _cache.Values
            .Where(m => m.IsAcknowledged && m.AcknowledgedAt.HasValue && m.AcknowledgedAt.Value < olderThan)
            .ToList();

        int count = 0;
        foreach (var message in messagesToRemove)
        {
            try
            {
                var filePath = Path.Combine(_storageDirectory, $"{message.MessageId}.msg");
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                _cache.TryRemove(message.MessageId, out _);
                count++;
            }
            catch (Exception ex)
            {
                LogUtil.Error(ex, "[MQTT] 清理持久化消息失败: {MessageId}", message.MessageId);
            }
        }

        return count;
    }
}