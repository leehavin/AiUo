using AiUo;

//var testValue = Environment.GetEnvironmentVariable("ASPNETCORE_HOSTINGSTARTUPASSEMBLIES");
//Environment.SetEnvironmentVariable("ASPNETCORE_HOSTINGSTARTUPASSEMBLIES", $"{testValue};StartupLibraryDemo");

var builder = AspNetHost.CreateBuilder();
// Add services to the container.
builder.AddAspNetEx();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseAspNetEx();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
