using AiUo.AspNet;
using AiUo.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GrpcDemo1.Controllers;

public class DemoController : AiUoControllerBase
{
    [AllowAnonymous]
    [HttpGet]
    public string Get()
    {
        return ConfigUtil.Service.ServiceId;
    }
}