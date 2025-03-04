using System;
using System.Collections.Generic;
using System.Linq;
using AiUo.Net;

namespace AiUo.Extensions.SshNet.Workers;

public abstract class WorkerBase
{
    private SshClientEx _client;
    private bool _showDetail = false;
    public SshClientEx Client
    {
        get
        {
            if (_client == null)
                throw new Exception($"SshNet.Worker必须设置SshClientEx。type: {GetType().FullName}");
            return _client;
        }
    }
    public WorkerBase(SshClientEx client = null)
    {
        _client = client;
    }
    public WorkerBase SetClient(SshClientEx client)
    {
        if (client != null) 
        {
            _client = client;
        }
        return this;
    }
    public WorkerBase ShowDetail(bool enabled = true)
    {
        _showDetail = enabled;
        return this;
    }
    public abstract void Execute();
    public EventHandler<WorkerOutputArgs> Output;

    protected void OutputText(string text, bool isMain = true, ConsoleColor textColor = default, ConsoleColor bgColor = default)
    {
        if (isMain || _showDetail)
            ConsoleEx.WriteLine(text, textColor, bgColor);
        Output?.Invoke(this, new WorkerOutputArgs { Text = text, IsMain = isMain });
    }
    protected ResponseResult<string> ExecuteCmd(string cmd)
    {
        OutputText($"执行命令：{cmd}", false);
        var result = Client.ExecuteCmd(cmd);
        OutputText(result.Message, false);
        return result;
    }
}
public class WorkerOutputArgs : EventArgs
{
    public string Text { get; set; }
    public bool IsMain { get; set; }
}

public class WorkerEngine : IDisposable
{
    public SshClientEx Client { get; }
    public List<WorkerBase> Workers { get; }
    public EventHandler<WorkerOutputArgs> Output;
    public EventHandler<int> Progress;
    public WorkerEngine(List<WorkerBase> workers, SshClientEx client, bool showDetail = true)
    {
        Workers = workers.Select(item =>
        {
            item.SetClient(client);
            item.ShowDetail(showDetail);
            item.Output += OnOutput;
            return item;
        }).ToList();
        Client = client;
    }
    private void OnOutput(object sender, WorkerOutputArgs args)
    {
        Output?.Invoke(sender, args);
    }
    public void Execute()
    {
        for (int i = 0; i < Workers.Count; i++)
        {
            Workers[i].Execute();
            Progress?.Invoke(this, i + 1);
        }
    }

    public void Dispose()
    {
        Client?.Disconnect();
    }
}