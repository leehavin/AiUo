using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AiUo.AspNet;

namespace Demo.WebAPI.Apis.V2;

public class DemoController : AiUoControllerVersionBase
{
    [HttpGet]
    [AllowAnonymous]
    public string Version()
    {
        return "2.0";
    }
}