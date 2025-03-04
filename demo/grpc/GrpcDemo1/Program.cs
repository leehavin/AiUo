
using AiUo;

var builder = AspNetHost.CreateBuilder();
builder.AddAspNetEx();

var app = builder.Build();
app.UseAspNetEx();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
