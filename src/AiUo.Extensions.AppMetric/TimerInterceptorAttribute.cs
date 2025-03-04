using App.Metrics;
using App.Metrics.Core.Options;
using App.Metrics.ReservoirSampling.Uniform;
using AspectCore.DependencyInjection;
using AspectCore.DynamicProxy;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AiUo.Extensions.AppMetric;

public class TimerInterceptorAttribute : AbstractInterceptorAttribute
{


    [FromServiceContext]
    public ILogger<TimerInterceptorAttribute> logger { get; set; }

    [FromServiceContext]
    public IMetrics metrics { get; set; }


    //private readonly ILogger<TimerInterceptorAttribute> logger;
    //private readonly IMetrics metrics;

    //public TimerInterceptorAttribute(ILogger<TimerInterceptorAttribute> logger, IMetrics metrics)
    //{
    //    this.logger = logger;
    //    this.metrics = metrics;
    //}
    /// <summary>
    ///     实现抽象方法
    /// </summary>
    /// <param name="context"></param>
    /// <param name="next"></param>
    public override async Task Invoke(AspectContext context, AspectDelegate next)
    {

        try
        {

            if (metrics == null)
            {
                metrics = context.ServiceProvider.GetService<IMetrics>();
            }

            if (logger == null)
            {
                logger = context.ServiceProvider.GetService<ILogger<TimerInterceptorAttribute>>();
            }

            TimerOptions SampleTimer = new TimerOptions
            {
                Reservoir = () => new DefaultAlgorithmRReservoir(),
                DurationUnit = TimeUnit.Milliseconds,
                MeasurementUnit = Unit.Results
            };
            //Console.WriteLine("执行之前");
            //await next(context);//执行被拦截的方法
            var className = context.ServiceMethod.ReflectedType?.FullName;
            //var a1 = context.Implementation.GetType().FullName;
            var methodName = context.ServiceMethod.Name;
            SampleTimer.Name = className + "." + methodName;
            //System.Console.WriteLine($"{className}, {methodName}");
            using (metrics.Measure.Timer.Time(SampleTimer))
            {
                //  await Task.Delay(1000 * new Random().Next(1, 30));
                await next(context); //执行被拦截的方法
            }

        }
        catch (Exception ex)
        {
            logger.LogError("aop error", ex);
            await next(context);
        }
    }

}

public sealed class DisableGlobalInterceptor : Attribute { }