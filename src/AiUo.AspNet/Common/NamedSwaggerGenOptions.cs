using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text;
using AiUo.Configuration;

namespace AiUo.AspNet.Common;

public class NamedSwaggerGenOptions : IConfigureNamedOptions<SwaggerGenOptions>
{
    private readonly IApiVersionDescriptionProvider provider;
    public NamedSwaggerGenOptions(IApiVersionDescriptionProvider provider)
    {
        this.provider = provider;
    }

    public void Configure(string name, SwaggerGenOptions options)
    {
        Configure(options);
    }

    public void Configure(SwaggerGenOptions options)
    {
        // 为发现的每个 API 版本添加 swagger 文档
        foreach (var description in provider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(description.GroupName, CreateVersionInfo(description));
        }
    }

    private OpenApiInfo CreateVersionInfo(ApiVersionDescription description)
    {
        var sbDesc = new StringBuilder();
        sbDesc.Append("<pre><b>");
        sbDesc.Append(GetDesc("ServiceId", ConfigUtil.Service.ServiceId));
        sbDesc.Append(GetDesc("Env", $"{ConfigUtil.Environment.Name}({ConfigUtil.Environment.Type})"));
        sbDesc.Append(GetDesc("IsDebug", ConfigUtil.Environment.IsDebug));
        sbDesc.Append(GetDesc("IsStaging", ConfigUtil.Environment.IsStaging));
        sbDesc.Append(GetDesc("PathBase", ConfigUtil.GetSection<AspNetSection>()?.PathBase));
        sbDesc.Append("</b></pre>");
        var info = new OpenApiInfo()
        {
            Title = $"{ConfigUtil.Project.ProjectId} API {description.GroupName}",
            Version = description.ApiVersion.ToString(),
            Description = sbDesc.ToString()
        };

        if (description.IsDeprecated)
        {
            info.Description += " 此 API 版本已弃用.";
        }

        return info;
        string GetDesc(string key, object value)
        {
            return $"<font size=\"3\" color=\"#FF9912\">{key}: </font>{value}&#9;";
        }
    }
}