using AiUo.Configuration;
using AiUo.Net.SMS;

namespace AiUo.Net;

public static class SMSUtil
{
    public static ISMSProvider Create(string name = null)
    {
        var section = ConfigUtil.GetSection<SMSSection>();
        if (!section.Clients.TryGetValue(name ?? section.DefaultClientName, out var element))
            return null;
        /*
        switch (element.Provider)
        {
            case SMSProvider.Tencent:
                return new TencentSMSProvider(element as TencentSMSElement);
            case SMSProvider.Bluegoon:
                return new BluegoonSMSProvider(element as BluegoonSMSElement);
        }
        */
        return null;
    }
}