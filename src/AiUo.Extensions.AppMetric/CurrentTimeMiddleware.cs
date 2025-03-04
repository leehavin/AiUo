using App.Metrics;
using App.Metrics.Core.Options;
using App.Metrics.Extensions.Middleware;
using App.Metrics.Extensions.Middleware.DependencyInjection.Options;
using App.Metrics.Timer.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Logging;

namespace AiUo.Extensions.AppMetric;

/// <summary>
/// 统计耗时 并记录接口访问日志 中间件
/// </summary>
public class CurrentTimeMiddleware : AppMetricsMiddleware<AspNetMetricsOptions>
{
    private const string TimerItemsKey = "__App.Metrics.RequestTimer__";
    private readonly App.Metrics.Timer.Abstractions.ITimer _requestTimer;

    public readonly IMetrics metrics;

    public TimerOptions RequestTransactionDuration = new TimerOptions
    {
        Context = nameof(CurrentTimeMiddleware),
        Name = "Transactions",
        MeasurementUnit = Unit.Requests
    };
    public CurrentTimeMiddleware(
        RequestDelegate next,
        AspNetMetricsOptions aspNetOptions,
        ILoggerFactory loggerFactory,
        IMetrics metrics)
        : base(next, aspNetOptions, loggerFactory, metrics)
    {
        this.metrics = metrics;

        //_requestTimer = Metrics.Provider
        //    .Timer
        //    .Instance(RequestTransactionDuration);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="context"></param>
    /// <param name="next"></param>
    /// <returns></returns>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            var endpoint = context.GetEndpoint()?.Metadata.GetMetadata<ControllerActionDescriptor>();
            if (PerformMetric(context) && endpoint != null)
            {

                //context.Items[TimerItemsKey] = _requestTimer.NewContext();

                //await Next(context);

                //var timer = context.Items[TimerItemsKey];

                //using (timer as IDisposable)
                //{
                //}

                //context.Items.Remove(TimerItemsKey);

                TimerOptions SampleTimer = new TimerOptions
                {
                    DurationUnit = TimeUnit.Milliseconds,
                };
                SampleTimer.Name = endpoint.ControllerName + "Controller." + endpoint.ActionName;
                using (metrics.Measure.Timer.Time(SampleTimer))
                {
                    await Next(context);
                }

            }
            else
            {
                await Next(context);
            }
        }
        catch
        {
            await Next(context);
        }

    }


}