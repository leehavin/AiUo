using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AiUo.AspNet;
using AiUo.Configuration;

namespace GrpcDemo2.Controllers;

public class DemoController : AiUoControllerBase
{
    [AllowAnonymous]
    [HttpGet]
    public string Get()
    {
        return ConfigUtil.Service.ServiceId;
    }
}