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


app.MapGet("/demo", async () =>
{
    var userId = new Random().Next(100).ToString();
    HttpContextEx.SetSession("UserId", userId);
    await HttpContextEx.SignInUseCookie(userId);
    Console.WriteLine(userId);
})
.WithName("set");

app.MapGet("/s", () =>
{
    HttpContextEx.SetSession("UserId", "asdf");
});

app.MapGet("/g", () => 
{
    var uid = HttpContextEx.GetSessionOrDefault("UserId", "");
    Console.WriteLine($"Session:{uid}");
    var userId = HttpContextEx.User?.Identity?.Name;
    Console.WriteLine($"Identity: {userId}");
});

app.Run();
