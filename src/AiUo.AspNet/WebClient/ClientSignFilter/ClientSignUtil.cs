using System;
using System.Collections.Concurrent;
using AiUo.Configuration;
using AiUo.Security;

namespace AiUo.AspNet.ClientSignFilter
{
    public static class ClientSignUtil
    {
        private static ConcurrentDictionary<string, ClientSignFilterService> _svcDict = new ConcurrentDictionary<string, ClientSignFilterService>();

        static ClientSignUtil()
        {
            ConfigUtil.RegisterChangedCallback((Action)(() => ClientSignUtil._svcDict.Clear()));
        }

        public static bool TryGetService(string name, out ClientSignFilterService service)
        {
            service = (ClientSignFilterService)null;
            ClientSignFilterSection section = ConfigUtil.GetSection<ClientSignFilterSection>();
            if (section == null)
                return false;
            if (name == null)
                name = section.DefaultFilterName;
            if (ClientSignUtil._svcDict.TryGetValue(name, out service))
                return true;
            ClientSignFilterElement element;
            if (!section.Filters.TryGetValue(name, out element))
                return false;
            service = new ClientSignFilterService(element);
            ClientSignUtil._svcDict.TryAdd(name, service);
            return true;
        }

        public static ClientSignFilterService GetService(string name = null)
        {
            ClientSignFilterService service;
            if (!ClientSignUtil.TryGetService(name, out service))
                throw new Exception("配置ClientSignFilter中Filters不存在。name: " + name);
            return service;
        }

        /// <summary>通过BothKey验证请求数据</summary>
        /// <param name="data"></param>
        /// <param name="sign"></param>
        /// <param name="sourceBothKey"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool VerifyByBothKey(
          string data,
          string sign,
          string sourceBothKey,
          string name = null)
        {
            return ClientSignUtil.GetService(name).VerifyByBothKey(data, sign, sourceBothKey);
        }

        /// <summary>获取加密以后的AccessKey(使用BothKey加密)</summary>
        /// <param name="sourceBothKey"></param>
        /// <param name="sourceAccessKey"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetEncryptedAccessKey(
          string sourceBothKey,
          string sourceAccessKey = null,
          string name = null)
        {
            return ClientSignUtil.GetService(name).GetEncryptedAccessKey(sourceBothKey, sourceAccessKey);
        }
    }
}