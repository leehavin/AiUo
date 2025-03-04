using AiUo.ChinaArea;
using AiUo.ChineseCalendar;
using AiUo.Common;
using AiUo.Cronos;
using AiUo.Demos;
using AiUo.IO;
using AiUo.Logging;
using AiUo.Net;
using AiUo.Randoms;
using AiUo.Reflection;
using AiUo.Security;
using AiUo.ShortId;
using AiUo.Text;
using AiUo.Text.Pinyin;
using System.ComponentModel;

namespace AiUo.Demos.Core;

internal class EasyDemo : DemoBase
{
    public override async Task Execute()
    {
        //计算两点位置的距离，返回两点的距离，单位：米
        var val = GpsUtil.GetDistance(116.38370990753174, 39.92319856900366, 116.3831090927124, 39.91954523865314);
        Console.WriteLine($"{val}米");

        // ISO 639-1小写双字母语言代码zh-cn 简体中文和zh-tw繁体中文
        Console.WriteLine(LanguageUtil.GetName("en"));

        // 变量格式如: {{key}}
        var str = new StringTemplateReplacer("字符串模板{{key1}}替换").Set("key1", "aaa")
            .ToString();
        Console.WriteLine(str);

        // 枚举
        var info = EnumUtil.GetInfo(typeof(TestEnum));
        Console.WriteLine(info.GetItem(1).Description);

        // 系列化
        var obj = new List<int>() {11,22,33 };
        var json = SerializerUtil.SerializeJson(obj);
        Console.WriteLine(json);

        // 字符串辅助(很多，慢慢看)
        Console.WriteLine(StringUtil.IndexOf("192.12.0.0", '.', 1));

        // 很多
        Console.WriteLine(AiUoUtil.IsWindowsOS);

        // 很多
        Console.WriteLine(IOUtil.IsDir("C:\\a.txt"));

        // 地区信息
        Console.WriteLine(ChinaAreaUtil.GetByAlias("京").MergerName);

        //中国阴历日期处理类
        Console.WriteLine(new ChineseCalendarHelper().ConstellationText);

        //CliUtil.ExecuteWinCmd("dir", out string output);
        //Console.WriteLine(output);

        var list = new List<MyInfo>() {
            new MyInfo(1,"a"),
            new MyInfo(2,"b")
        };
        ConsoleUtil.WriteTable(list);

        //Cron
        Console.WriteLine(CronUtil.GetNextLocal("0 0/2 * * * ?"));

        Console.WriteLine(ShortIdUtil.Generate(new ShortIdOptions
        { 
            CustomAlphabet = "@#$%^"
        }, 18));

        LogUtil.Info("aaa");

        // 配置文件
        /*
        var client = HttpClientExFactory.Create("test");
        var ret = client.CreateAgent()
            .AddUrl("apis/weather/")
            .AddParameter("id", "101110101")
            .GetStringAsync()
            .Result;
        Console.WriteLine(ret.ResultString);
        */

        //var ftp = new FtpClient("10.0.0.100","admin","123");
        Console.WriteLine(NetUtil.GetIpMode("192.168.1.1").ToString());

        var url = new UriBuilderEx("http://www.baidu.com");
        url.AppendQueryString("id","1");
        Console.WriteLine(url.ToString());

        //WebSocketClientEx

        //配置
        RandomUtil.DefaultProvider = RandomProviderFactory.CreateDefaultProvider();
        Console.WriteLine(RandomUtil.NextInt(100));

        var o = new MyInfo(1,"aa");
        Console.WriteLine(ReflectionUtil.GetPropertyValue<string>(o, "Name"));

        Console.WriteLine(SecurityUtil.MD5Hash("abc"));

        Console.WriteLine(JwtUtil.CreateJwtToken(123));

        var filter = new DirtyStringFilter();
        Console.WriteLine(filter.HasDirty("哈哈你大爷操二hi"));

        Console.WriteLine(PinyinUtil.GetFirstPinyin("中国"));

        //注册允许字符检测
        var f = new LimitedCharFilter(true);
        f.AddNumberChars();
        Console.WriteLine(f.IsValid("123a4"));

        //注册验证码
        var s = new RandomString(CharsScope.Numbers, 4);
        Console.WriteLine(s.Next());

    }
}
enum TestEnum
{
    [Description("111")]
    A,
    [Description("222")]
    B
}
class MyInfo
{
    public int Id { get; set; }
    public string Name { get; set; }
    public MyInfo(int id, string name)
    {
        Id = id;
        Name = name;
    }
}