using Microsoft.AspNetCore.Http;

namespace AiUo.AspNet;

public class IgnoreActionFilterAttribute : Attribute
{
    internal const string ITEM_NAME = "IgnoreActionFilter";
    internal static bool CheckIgnore(HttpContext context)
        => context.Items.ContainsKey(ITEM_NAME);
}