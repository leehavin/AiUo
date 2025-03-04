using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AiUo.Hosting.Services;

/// <summary>
/// host生命周期服务
/// </summary>
public interface IAiUoHostLifetimeService
{
    List<Func<Task>> StartingTasks { get; }
    List<Func<Task>> StartedTasks { get; }
    List<Func<Task>> StoppingTasks { get; }
    List<Func<Task>> StoppedTasks { get; }

    void RegisterStarting(Func<Task> func);
    void RegisterStarted(Func<Task> func);
    void RegisterStopping(Func<Task> func);
    void RegisterStopped(Func<Task> func);
}
public class DefaultAiUoHostLifetimeService : IAiUoHostLifetimeService
{
    public List<Func<Task>> StartingTasks { get; } = new();
    public List<Func<Task>> StartedTasks { get; } = new();
    public List<Func<Task>> StoppingTasks { get; } = new();
    public List<Func<Task>> StoppedTasks { get; } = new();

    public void RegisterStarting(Func<Task> func)
    {
        StartingTasks.Add(func);
    }
    public void RegisterStarted(Func<Task> func)
    {
        StartedTasks.Add(func);
    }
    public void RegisterStopping(Func<Task> func)
    {
        StoppingTasks.Add(func);
    }
    public void RegisterStopped(Func<Task> func)
    {
        StoppedTasks.Add(func);
    }
}