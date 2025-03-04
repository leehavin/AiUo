using System;
using System.Threading;

namespace AiUo.Extensions.SshNet.Workers;

public class Reboot : WorkerBase
{
    public int TryCount { get; private set; }
    private int _interval = 5000;
    public Reboot(int timeoutMinutes = 5, SshClientEx client = null) : base(client)
    {
        TryCount = timeoutMinutes * 60 * 1000 / _interval;
    }
    public override void Execute()
    {
        try
        {
            ExecuteCmd("sudo shutdown -r now");
        }
        catch { }
        OutputText("正在重启服务器", true);
        var tryCount = 0;
        do
        {
            try
            {
                Client.GetSshClient(false).Connect();
            }
            catch (Exception)
            {
                Thread.Sleep(_interval);
                tryCount++;
                OutputText($"服务器重启中...{TimeSpan.FromSeconds(tryCount * _interval/1000).TotalSeconds}秒", true);
            }
        }
        while (!Client.GetSshClient(false).IsConnected && tryCount < TryCount);
        if (!Client.GetSshClient(false).IsConnected)
            throw new Exception($"连接服务器超时。host: {Client.ConnectionInfo.Host} type: {GetType().FullName}");
        OutputText("服务器重启成功", true);
    }
}