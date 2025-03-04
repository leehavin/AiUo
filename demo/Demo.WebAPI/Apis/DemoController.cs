using Demo.WebAPI.DAL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AiUo.AspNet;
using AiUo;
using AiUo.Logging;
using AiUo.DbCaching;
using AiUo.Randoms;
using AiUo.Security;
using AiUo.SnowflakeId;

namespace Demo.WebAPI.Apis
{
    /// <summary>
    /// 测试Demo API
    ///     无返回值: void
    ///     有具体返回值：ApiResult(T)
    /// </summary>
    //[ApiAccessFilter()]
    //[IgnoreActionFilter]
    //[ApiController]
    //[Route("api/[controller]/[action]")]
    //[EnableCors()]
    public class DemoController : AiUoControllerBase
    {
        public DemoController(TESTA a, TESTB b)
        {
            var ahash1 = DIUtil.GetService<TESTA>().GetHashCode();
            var ahash2 = a.GetHashCode();
            var bhash1 = b.GetHashCode();  
            var bHash2 = DIUtil.GetService<TESTB>().GetHashCode();
            if (ahash1 != ahash2 || bhash1 != bHash2)
                throw new Exception("DI哈希不同2");
        }
        [HttpGet]
        public string Version()
        {
            return HttpContextEx.GetJwtToken().UserId;
        }

        [HttpGet]
        [AllowAnonymous]
        //[RequestLogging("high")]
        public async Task<string> Test1()
        {
            LogUtil.Info("234234234");
            //await api.SetInstanceEnabled();

            //var sourceKey = "80150420F2885ECC2867209112D4E745lobby";
            //var sourceData = $"1704188983585{sourceKey}";
            //var sign = "I7XObb8dHk78D/Oe/Ec0pD42CqvgbVZdb9beJ8oXx7Y=";
            //var helper = new AccessSignFilterService();
            //// true
            ////08yC0b81GC2k8DN5 bothKey
            //var result = helper.VerifyBothKey(sourceKey, sourceData, sign);

            ////F89F733B671176B6072034C3CA87E57923A9890C0701E09099CD8739699B51D6
            //var accessKey = helper.GetAccessKeyDecrypt(sourceKey, "F89F733B671176B6072034C3CA87E57923A9890C0701E09099CD8739699B51D6");

            ////80150420F2885ECC2867209112D4E745|HgLHd+fXmCvkS7Uclt+wjUAJTorKNwjQpor6ux3iY+g=
            //sourceData = "{\"appId\":\"lobby\",\"operatorId\":\"own_lobby_bra\",\"langId\":\"pt\",\"countryId\":\"BRA\",\"currencyId\":\"BRL\"}";
            //result = helper.VerifyAccessKey("80150420F2885ECC2867209112D4E745", sourceData, "HgLHd+fXmCvkS7Uclt+wjUAJTorKNwjQpor6ux3iY+g=");

            return "aaaaaaaa";
        }
        [HttpGet]
        [AllowAnonymous]
        public async Task<string> Test2()
        {
            throw new CustomException("bbbbbbbbbb");
            //var i = Random.Shared.Next(10);
            //await DbUtil.GetRepository<Ss_providerEO>().UpdateAsync(it => new Ss_providerEO
            //{
            //    ProviderName = $"自有供应商{i}"
            //}, it => it.ProviderID == "own"); ;
            //await DbCachingUtil.PublishUpdate(new DbCacheChangeMessage
            //{
            //    Changed = new List<DbCacheItem>
            //    {
            //        new DbCacheItem
            //        {
            //            ConfigId = "default",
            //            TableName = "s_provider",
            //        }
            //    }
            //});
            //return "";
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<string> Test3()
        {
            var result = await DbCachingUtil.PublishCheck();
            return "";
        }



        [HttpGet]
        [AllowAnonymous]
        public string GetJwtToken()
        {
            var uip = AspNetUtil.GetRemoteIpString();
            return JwtUtil.CreateJwtToken(new JwtTokenData
            {
                UserId = RandomUtil.NextInt(10).ToString(),
                Expires = TimeSpan.FromMinutes(20),
                Role = "User",
                Meta = "abc",
                SplitDbKey = "112233",
                UserIp = "192.168.1.1"
            });
        }
        [HttpGet]
        
        public JwtTokenInfo ReadJwtToken()
        {
            return HttpContextEx.GetJwtToken();
        }


        [HttpGet]
        [AllowAnonymous]
        public long GetSnowflakeId()
        {
            return SnowflakeIdUtil.NextId();
        }

        [HttpPost]
        [AllowAnonymous]
        public DemoIpo2 PostIpo(DemoIpo ipo)
        {
            return new DemoIpo2 { Id = ipo.Name.Length };
        }
        /*
         #region Base
         /// <summary>
         /// 获取JwtToken
         /// </summary>
         /// <returns></returns>
         [HttpGet]
         [AllowAnonymous]
         public string GetJwtToken()
         {
             var uip = AspNetUtil.GetRemoteIpString();
             return JwtUtil.GenerateJwtToken(RandomUtil.NextInt(10), UserRole.User, uip);
         }

         [HttpGet]
         public string CheckJwtToken() => $"成功! UserId: {UserId} UserRole: {UserRole}";

         /// <summary>
         /// 返回结果
         /// </summary>
         /// <param name="type">返回类型 </param>
         [HttpGet]
         [AllowAnonymous]
         public IActionResult GetResult(int type = 0)
         {
             switch (type)
             {
                 case 0:
                     return Ok();
                 case 1:
                     return Ok(type);
                 case 2:
                     return StatusCode(404, type);
                 case 3:
                     return Content("文本内容", "text/plain", Encoding.UTF8);
                 case 4:
                     return NoContent();
                 case 5:
                     var filePath = "appsettings.json";
                     var provider = new FileExtensionContentTypeProvider();
                     if (!provider.TryGetContentType(filePath, out var contentType))
                         contentType = "application/octet-stream";
                     return File(System.IO.File.ReadAllBytes(filePath), contentType, Path.GetFileName(filePath));
                 case 6:
                     return Unauthorized(type);
                 case 7:
                     return NotFound();
                 case 8:
                     return BadRequest(type);
                 case 9:
                     throw new CustomException("customErrCode", "客户端Action=1", null, "其他数据");
                 case 10:
                     var i = 0;
                     var j = 1;
                     var k = j / i;
                     break;
                 case 11:
                     return (ObjectResult)new ApiResult<int>(123);
             }
             return Ok();
         }
         #endregion

         [HttpGet]
         [AllowAnonymous]
         public void Get01(int id, string name)
         {
             var a = IDGeneratorUtil.NextId();
             LogUtil.Debug($"id: {id} name: {name}");
         }
         [HttpPost]
         [AllowAnonymous]
         public void Get02([FromQuery] DemoIpo ipo, [FromForm] DemoIpo2 ipo2)
         {
             return;
         }

         [HttpPost]
         [AllowAnonymous]
         [ApiAccessFilter()]
         public void Test()
         {
             var i = 0;
             var a = 100 / i;
             //throw new CustomException("sdf", null);
         }

         [HttpPost]
         [AllowAnonymous]
         public DemoIpo2 PostIpo(DemoIpo ipo)
         {
             return new DemoIpo2 { Id = ipo.Name.Length };
         }

         [HttpPost]
         [AllowAnonymous]
         public DemoIpo2 PostIpo2(Demo.WebAPI.Apis2.DemoIpo ipo)
         {
             return new DemoIpo2 { Id = ipo.Name.Length };
         }

         [HttpGet]
         [AllowAnonymous]
         [RequestLogging]
         public string Version()
         {
             return "1.0";
         }

         [HttpGet]
         [AllowAnonymous]
         //[ResponseCacheEx("default")]
         [ResponseCacheKeys(10, "*")]
         public string Check(int id, string name)
         {
             return DateTime.Now.ToString("HH:mm:ss");
         }

         [HttpGet]
         [AllowAnonymous]
         public string Test1(string origin)
         {
             return "ok";
         }
         [HttpGet]
         [AllowAnonymous]
         [EnableCors()]
         public string Test2()
         {
             return "test2";
         }
         [HttpGet]
         [AllowAnonymous]
         [EnableCors("aaa")]
         public string Test3()
         {
             return "test3";
         }
         */
    }

    public class DemoIpo
    {
        public string Name { get; set; }
    }
    public class DemoIpo2
    {
        public int Id { get; set; }
    }
    public class UserInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class DemoProvider : IDbCachePreloadProvider
    {
        public List<DbCachePreloadItem> GetPreloadList()
        {
            var ret = new List<DbCachePreloadItem>();
            ret.Add(new DbCachePreloadItem(typeof(Ss_appEO)));
            return ret;
        }
    }
}

namespace Demo.WebAPI.Apis2
{
    public class DemoIpo
    {
        public string Name { get; set; }
    }
}
