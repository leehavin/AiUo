using AiUo.CliWrap;

namespace AiUo.Linux;

public static class FirewallUtil
{
    public static void Stop() => CliUtil.ExecuteReturn("systemctl stop firewalld.service");
    public static void Start() => CliUtil.ExecuteReturn("systemctl start firewalld.service");
    public static void Enable() => CliUtil.ExecuteReturn("systemctl enable firewalld.service");
    public static void Disable() => CliUtil.ExecuteReturn("systemctl disable firewalld.service");
    public static void AddTcpPort(int port) => CliUtil.ExecuteReturn($"firewall-cmd --add-port={port}/tcp --permanent");
    public static void Reload()=> CliUtil.ExecuteReturn("firewall-cmd --reload");
}