using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AiUo;

public static class ThreadUtil
{
    /// <summary>
    /// 仅需要任何一个Task完成即可，其他Task将取消
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="functions"></param>
    /// <returns></returns>
    public static async Task<T> NeedOnlyOne<T>(params Func<CancellationToken, Task<T>>[] functions)
    {
        var cts = new CancellationTokenSource();
        var tasks = (from function in functions
            select function(cts.Token)).ToArray();
        var completed = await Task.WhenAny(tasks).ConfigureAwait(false);
        cts.Cancel();
        foreach (var task in tasks)
        {
            var ignored = task.ContinueWith(
                t => { }, TaskContinuationOptions.OnlyOnFaulted);
        }

        return await completed;
    }
}