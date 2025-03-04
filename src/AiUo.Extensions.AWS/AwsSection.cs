using AiUo.Configuration;
using Amazon.Extensions.NETCore.Setup;
using Microsoft.Extensions.Configuration;
using System.Text.RegularExpressions;

namespace AiUo.Extensions.AWS;

public class AwsSection : AWSOptions, IConfigSection
{
    public string SectionName => "AWS";

    public bool Enabled { get; set; }
    public string AccessKey { get; set; }
    public string SecretKey { get; set; }
    public string VpcName { get; set; }

    public LoadBalancingElement LoadBalancing { get; set; }

    public void Bind(IConfiguration configuration)
    {
        configuration?.Bind(this);
        if (LoadBalancing != null)
        {
            if (LoadBalancing.RegisterTargetGroup && string.IsNullOrEmpty(LoadBalancing.TargetGroupName))
            {
                LoadBalancing.TargetGroupName = Regex.Replace(ConfigUtil.Project.ProjectId, "[^a-zA-Z0-9]", "-"); 
            }
        }
    }
}