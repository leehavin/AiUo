using System.Collections.Generic;

namespace AiUo.Configuration.Common;

internal class EnvironmentTypeParser
{
    private static Dictionary<string, EnvironmentType> _envMapDic = new() {
        // dev
        { "dev", EnvironmentType.DEV},
        { "development",EnvironmentType.DEV },
        // sit
        { "sit",EnvironmentType.SIT },
        { "test",EnvironmentType.SIT },
        // fat
        { "fat",EnvironmentType.FAT },
        { "qa",EnvironmentType.FAT },
        // uat
        { "uat",EnvironmentType.UAT },
        { "staging",EnvironmentType.UAT },
        // pre
        { "pre",EnvironmentType.PRE },
        // pro
        { "pro",EnvironmentType.PRO },
        { "prod",EnvironmentType.PRO },
        { "production",EnvironmentType.PRO },
    };
    public EnvironmentType Parse(string envString)
    {
        return !string.IsNullOrEmpty(envString) && _envMapDic.TryGetValue(envString.ToLower(), out var v)
            ? v : EnvironmentType.PRO;
    }
}