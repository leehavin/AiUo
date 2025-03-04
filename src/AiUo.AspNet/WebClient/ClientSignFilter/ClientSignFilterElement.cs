using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiUo.AspNet
{
    public class ClientSignFilterElement
    {
        public string Name { get; set; }

        public bool Enabled { get; set; }

        public string HeaderName { get; set; }

        /// <summary>KeySeed是否加密保存</summary>
        public bool IsProtection { get; set; }

        /// <summary>下发给客户端</summary>
        public string BothKeySeed { get; set; }

        public string AccessKeySeed { get; set; }

        public string KeyIndexes { get; set; }
    }
}
