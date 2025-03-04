using Microsoft.AspNetCore.Builder;

namespace AiUo.AspNet;

public interface IAiUoHostingStartup
{
    void ConfigureServices(WebApplicationBuilder builder);
    void Configure(WebApplication app);
}