using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.Configuration;
using AiUo.AspNet;
using AiUo.Collections;
using AiUo.Configuration;
using AiUo.Logging;
using AiUo.Reflection;

namespace AiUo.Configuration
{
    /// <summary>
    /// asp.net core 跨域访问配置
    /// </summary>
    public class CorsSection : ConfigSection
    {
        public override string SectionName => "Cors";

        /// <summary>
        /// 是否使用cors中间件
        /// </summary>
        public CorsUseElement UseCors { get; set; }
        public ICorsPoliciesProvider PoliciesProvider { get; private set; }

        /// <summary>
        /// 策略集合
        /// </summary>
        public Dictionary<string, CorsPolicyElement> Policies = new Dictionary<string, CorsPolicyElement>();

        public override void Bind(IConfiguration configuration)
        {
            base.Bind(configuration);
            
            // Policies
            Policies = configuration.GetSection("Policies")
                .Get<Dictionary<string, CorsPolicyElement>>() ?? new();
            Policies.ForEach(x =>
            {
                x.Value.Name = x.Key;
            });

            // DefaultPolicy
            if (string.IsNullOrEmpty(UseCors.DefaultPolicy))
            {
                UseCors.DefaultPolicy = Policies?.Count == 1
                    ? Policies.First().Key : "default";
            }

            // PolicyProvider
            if (!string.IsNullOrEmpty(UseCors.PoliciesProvider))
            {
                PoliciesProvider = ReflectionUtil.CreateInstance(UseCors.PoliciesProvider) as ICorsPoliciesProvider;
                if (PoliciesProvider == null)
                    throw new Exception($"配置文件Cors:UseCors:PolicyProvider必须继承ICorsPoliciesProvider. value:{UseCors.PoliciesProvider}");
            }
        }

        private List<CorsPolicyElement> GetPolicies()
        {
            if (PoliciesProvider != null)
            {
                var policies = PoliciesProvider.GetPolicies();
                // 附加到配置中
                policies.ForEach(x =>
                {
                    x.Name ??= "default";
                    if (Policies.ContainsKey(x.Name))
                    {
                        var oldOrigins = Policies[x.Name].Origins.Trim().TrimEnd(';');
                        var newOrigins = $"{oldOrigins};{x.Origins?.Trim()}";
                        var originsSet = newOrigins.Split(';', StringSplitOptions.RemoveEmptyEntries).ToHashSet();
                        Policies[x.Name].Origins = string.Join(';', originsSet);
                    }
                    else
                    {
                        Policies.Add(x.Name, x);
                    }
                    Policies[x.Name].OriginSet = null;
                });
            }
            return Policies.Values.ToList();
        }
        public void AddPolicies(CorsOptions opts, bool isUpdate = false)
        {
            var policies = GetPolicies();
            if (policies?.Count > 0)
            {
                var log = new LogBuilder("CORS").SetLevel(Microsoft.Extensions.Logging.LogLevel.Information);
                if (isUpdate)
                    log.AddMessage("更新跨域设置");
                else
                    log.AddMessage("启动 => 初始化跨域设置");
                foreach (var policy in policies)
                {
                    log.AddField($"Origins.{policy.Name}", $"[{policy.Name}] {policy.Origins}");
                    if (policy.Name == UseCors?.DefaultPolicy)
                        opts.AddDefaultPolicy(AspNetUtil.GetPolicyBuilder(policy));
                    else
                        opts.AddPolicy(policy.Name, AspNetUtil.GetPolicyBuilder(policy));
                }
                log.Save();
            }
        }
    }
}

namespace AiUo.AspNet
{
    public class CorsUseElement
    {
        public bool Enabled { get; set; }
        public bool EnabledReferer { get; set; }
        public string PoliciesProvider { get; set; }
        public string DefaultPolicy { get; set; }
    }

    /// <summary>
    /// CORS策略项
    /// </summary>
    public class CorsPolicyElement
    {
        public string Name { get; set; }
        /// <summary>
        /// 允许的来源,分号;分隔（下同）
        /// </summary>
        public string Origins { get; set; }
        /// <summary>
        /// 允许的HTTP方法
        /// </summary>
        public string Methods { get; set; }
        /// <summary>
        /// 允许的请求标头
        /// </summary>
        public string Headers { get; set; }
        /// <summary>
        /// Access-Control-Max-Age 时间(秒)
        /// </summary>
        public int MaxAge { get; set; } = 86400;

        private HashSet<string> _originSet = null;
        internal HashSet<string> OriginSet
        {
            get
            {
                if (_originSet == null)
                {
                    _originSet = ParseOriginSet();
                }
                return _originSet;
            }
            set { _originSet = value; }
        }
        private HashSet<string> ParseOriginSet()
        {
            var ret = new HashSet<string>();
            if (string.IsNullOrEmpty(Origins) || Origins == "*")
                return ret;
            Origins.Split(';', StringSplitOptions.RemoveEmptyEntries)
                .ForEach(x => ret.Add(x));
            return ret;
        }
    }
}
