using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using AiUo.AspNet;

namespace StartupLibraryDemo;

public class HostingStartup : IAiUoHostingStartup
{

    public void Configure(WebApplication app)
    {
        Console.WriteLine("BBBBBBBBB");
    }

    public void ConfigureServices(WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton(new HttpClient());
    }
}
/*
[assembly: HostingStartup(typeof(StartupLibraryDemo.HostingStartup))]
namespace StartupLibraryDemo
{
public class HostingStartup : IHostingStartup
{
    public void Configure(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            Console.WriteLine("BBBBBBBBB");
        });
    }
}

public class DemoFilter : IStartupFilter
{
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
    {
        return app =>
        {
            app.UseMiddleware<DemoMiddleware>();
            next(app);
        };
    }
}
public class DemoMiddleware
{
    private readonly RequestDelegate _next;
    private string cr = Environment.NewLine;
    public DemoMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public Task Invoke(HttpContext context)
    {
        Console.WriteLine("AAAAAAAAAAAAAAA");
        return _next(context);
    }
}

}
*/