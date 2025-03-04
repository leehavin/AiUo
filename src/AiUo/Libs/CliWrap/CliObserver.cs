using CliWrap.EventStream;
using System;
using System.Threading.Tasks;

namespace AiUo.CliWrap;

public class CliObserver : IObserver<CommandEvent>
{
    public int ProcessId;
    private CliResult Result = new CliResult();
    private CliStatus Status = CliStatus.Start;
    private Action<string, CliStatus> _action;
    private object _async = new object();
    private Task Action(string msg, CliStatus status)
    {
        return Task.Run(() =>
        {
            _action(msg, status);
        });
    }
    public CliObserver(Action<string, CliStatus> action)
    {
        _action = action;
    }
    public CliStatus GetStatus()
    {
        return Status;
    }
    public CliResult GetResult()
    {
        return Result;
    }

    public void OnCompleted()
    {
        Action(null, CliStatus.Completed);
        Result.ExitTime = DateTime.Now;
        Status = CliStatus.Completed;
    }

    public void OnError(Exception error)
    {
        var ex = ExceptionUtil.GetInnerException(error);
        Action(ex.Message, CliStatus.Error);
        Result.ExitTime = DateTime.Now;
        Result.RunTime = Result.ExitTime - Result.StartTime;
        Status = CliStatus.Error;
    }

    public void OnNext(CommandEvent value)
    {
        switch (value)
        {
            case StartedCommandEvent started:
                ProcessId = started.ProcessId;
                Action(started.ProcessId.ToString(), CliStatus.Start);
                Result.StartTime = DateTime.Now;
                Status = CliStatus.Start;
                break;
            case StandardOutputCommandEvent stdOut:
                Action(stdOut.Text, CliStatus.Output);
                Status = CliStatus.Output;
                break;
            case StandardErrorCommandEvent stdErr:
                Action(stdErr.Text, CliStatus.Error);
                Status = CliStatus.Error;
                break;
            case ExitedCommandEvent exited:
                Action(exited.ExitCode.ToString(), CliStatus.Exited);
                Status = CliStatus.Exited;
                Result.ExitCode = exited.ExitCode;
                break;
        }
    }
}