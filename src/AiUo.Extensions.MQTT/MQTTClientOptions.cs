using System;

namespace AiUo.Extensions.MQTT
{
    /// <summary>
    /// MQTT客户端选项配置
    /// </summary>
    public class MQTTClientOptions
    {
        /// <summary>
        /// 自动重连延迟（毫秒）
        /// </summary>
        public int AutoReconnectDelayMs { get; set; } = 5000;

        /// <summary>
        /// 最大重连尝试次数
        /// </summary>
        public int MaxReconnectAttempts { get; set; } = 10;

        /// <summary>
        /// 是否启用消息队列
        /// </summary>
        public bool EnableMessageQueueing { get; set; } = true;

        /// <summary>
        /// 最大队列大小
        /// </summary>
        public int MaxQueueSize { get; set; } = 1000;

        /// <summary>
        /// 是否启用遗嘱消息
        /// </summary>
        public bool EnableLastWill { get; set; } = false;

        /// <summary>
        /// 遗嘱消息主题
        /// </summary>
        public string LastWillTopic { get; set; } = "clients/status";

        /// <summary>
        /// 遗嘱消息内容
        /// </summary>
        public string LastWillMessage { get; set; } = "Offline";

        /// <summary>
        /// 遗嘱消息QoS级别
        /// </summary>
        public int LastWillQoS { get; set; } = 1;

        /// <summary>
        /// 遗嘱消息是否保留
        /// </summary>
        public bool LastWillRetain { get; set; } = true;

        /// <summary>
        /// 是否禁用密钥环，解决"Unknown element with name 'doc' found in keyring"警告
        /// </summary>
        public bool DisableKeyring { get; set; } = true;

        /// <summary>
        /// 是否使用随机ClientId后缀，避免客户端ID冲突
        /// </summary>
        public bool UseRandomClientIdSuffix { get; set; } = true;
    }
}