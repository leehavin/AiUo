using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using AiUo.AspNet;

namespace CorsAPI;

[AllowAnonymous]
public class DemoController : AiUoControllerBase
{
    [HttpGet]
    [EnableCors()]
    public string test()
    {
        return "cors OK";
    }

    [HttpGet]
    public async Task add()
    {
        //await DbCachingUtil.PublishUpdate(new List<DbCacheItem> { new DbCacheItem() 
        //{
        //    ConfigId="default",
        //    TableName="s_operator"
        //} });
    }
}