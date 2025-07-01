using AiUo.AspNet.Validations;
using AiUo.AspNet.Validations.FluentValidation.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers(options =>
{
    // 添加FluentValidation Action Filter
    options.Filters.Add<AiUo.AspNet.Validations.FluentValidation.Attributes.FluentValidationActionFilter>();
});
builder.Services.AddHttpContextAccessor();

// 添加基于特性的FluentValidation支持
builder.Services.AddFluentValidationWithAttributes(options =>
{
    options.DefaultErrorCode = "VALIDATION_ERROR";
    options.StopOnFirstFailure = false;
    options.MaxErrors = 100;
    
    // 启用性能监控
    options.EnablePerformanceMonitoring = true;
    options.PerformanceThresholdMs = 500; // 500ms阈值
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "FluentValidation Demo API", Version = "v1" });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "FluentValidation Demo API V1");
        c.RoutePrefix = string.Empty; // 设置Swagger UI为根路径
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Configure model validation extensions 
ModelValidationExtensions.ConfigureServiceProvider(app.Services);

app.Run();