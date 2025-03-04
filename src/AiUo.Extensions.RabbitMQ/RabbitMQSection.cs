using AiUo.Collections;
using AiUo.Extensions.RabbitMQ;
using Microsoft.Extensions.Configuration;

namespace AiUo.Configuration
{
    public class RabbitMQSection : ConfigSection
    {
        public override string SectionName => "RabbitMQ";
        public bool Enabled { get; set; } = true;
        /// <summary>
        /// 是否开启消息消费日志(很大)
        /// </summary>
        public bool MessageLogEnabled { get; set; }
        /// <summary>
        /// 是否开启调试日志(很大)
        /// </summary>
        public bool DebugLogEnabled { get; set; }
        /// <summary>
        /// 是否开启Consumer消费
        /// </summary>
        public bool ConsumerEnabled { get; set; } = true;
        public string DefaultConnectionStringName { get; set; }
        public Dictionary<string, MQConnectionStringElement> ConnectionStrings = new();

        public bool AutoLoad { get; set; }
        /// <summary>
        /// ReceiveConsumer、RespondConsumer、SubscribeConsumer所在的程序集,用于消费注册
        /// </summary>
        public List<string> ConsumerAssemblies { get; set; } = new List<string>();
        public override void Bind(IConfiguration configuration)
        {
            base.Bind(configuration);
            ConnectionStrings = configuration.GetSection("ConnectionStrings")
                .Get<Dictionary<string, MQConnectionStringElement>>() ?? new();
            ConnectionStrings.ForEach(x =>
            {
                x.Value.Name = x.Key;
            });

            if (string.IsNullOrEmpty(DefaultConnectionStringName) && ConnectionStrings.Count == 1)
                DefaultConnectionStringName = ConnectionStrings.First().Key;
            // Assemblies
            ConsumerAssemblies = configuration?.GetSection("ConsumerAssemblies").Get<List<string>>() 
                ?? new List<string>();
        }
    }
}

namespace AiUo.Extensions.RabbitMQ
{
    public class MQConnectionStringElement
    {
        public string Name { get; internal set; }
        /// <summary>
        /// 是否使用简短的名字定义ExchangeName和QueueName(MQConventions)
        /// </summary>
        public bool UseShortNaming { get; set; }
        /// <summary>
        /// 是否使用ConfigUtil.EnvironmentString作为VirtualHost
        /// </summary>
        public bool UseEnvironmentVirtualHost { get; set; }
        /// <summary>
        /// 是否启用高可用(仅MQ群集使用)
        /// </summary>
        public bool UseQuorum { get; set; }
        public string ConnectionString { get; set; }
    }
}
