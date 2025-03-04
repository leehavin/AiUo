using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;

namespace AiUo.Configuration.Common;

internal class AppSettingsFileConfigBuilder
{
    public IConfiguration Build(List<string> files)
    {
        IConfigurationBuilder ret = new ConfigurationBuilder();
        ret.SetBasePath(AppContext.BaseDirectory);
        files.ForEach(file => ret.AddJsonFile(Path.GetFileName(file), true, true));
        ret.AddEnvironmentVariables();
        return ret.Build();
    }

    public List<string> GetConfigFiles(string envString)
    {
        var ret = new List<string>();
        if (TryGetFile("appsettings.json", out var file))
            ret.Add(file);
        if (!string.IsNullOrEmpty(envString))
        {
            if (TryGetFile($"appsettings.{envString}.json", out file))
            {
                ret.Add(file);
            }
            else
            {
                if (TryGetFile($"appsettings.{envString.ToLower()}.json", out file))
                    ret.Add(file);
            }
        }
        return ret;
    }
    private bool TryGetFile(string name, out string file)
    {
        file = Path.Combine(AppContext.BaseDirectory, name);
        if (File.Exists(file))
            return true;

        file = Path.Combine(Directory.GetCurrentDirectory(), name);
        if (File.Exists(file))
            return true;
        return false;
    }
}