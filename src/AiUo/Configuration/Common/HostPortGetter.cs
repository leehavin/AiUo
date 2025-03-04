using System;
using System.Collections.Generic;
using System.Linq;

namespace AiUo.Configuration.Common;

internal class HostPortGetter
{
    #region Properties
    private List<string> _httpEnvs = new List<string>()
    {
        "ENV_HTTP_PORT",
        "HTTP_PORTS",
        "DOTNET_HTTP_PORTS",
        "ASPNETCORE_HTTP_PORTS"
    };
    private EnvironmentInfo _info;
    public HostPortGetter(EnvironmentInfo info)
    {
        _info = info;
    }
    #endregion

    public int GetHttpPort()
    {
        if (_info.UrlsEndPoint?.Port > 0)
            return _info.UrlsEndPoint.Port;
        var port = GetEnvPort(_httpEnvs);
        if (port > 0)
            return port;
        return 0;
    }
    public int GetGrpcPort()
    {
        return GetEnvPort("ENV_GRPC_PORT");
    }
    public int GetWebSocketPort()
    {
        return GetEnvPort("ENV_WS_PORT");
    }
    private int GetEnvPort(string env)
        => Environment.GetEnvironmentVariable(env).ToInt32(0);

    private int GetEnvPort(List<string> envs)
    {
        var ret = 0;
        foreach (var env in envs)
        {
            var ports = Environment.GetEnvironmentVariable(env);
            if (!string.IsNullOrEmpty(ports))
            {
                ret = ports.Split(';').FirstOrDefault("0").ToInt32();
                if (ret > 0)
                    return ret;
            }
        }
        return ret;
    }
}