using Microsoft.AspNetCore.Authorization;
using AiUo;
using AiUo.AspNet;

var builder = AspNetHost.CreateBuilder();

// Add services to the container.
builder.AddAspNetEx();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseAspNetEx();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/demo", [Authorize] async () =>
{
    var uid = HttpContextEx.GetSessionOrDefault("UserId", "");
    Console.WriteLine($"Session:{uid}");
    var userId = HttpContextEx.User?.Identity?.Name;
    Console.WriteLine($"Identity: {userId}");
})
.WithName("get");

app.Run();