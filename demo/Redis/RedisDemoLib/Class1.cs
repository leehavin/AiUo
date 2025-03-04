using AiUo.Extensions.StackExchangeRedis;

namespace RedisDemoLib;

[RedisPublishMessage()]
public class PubMsg1
{
    public int Id { get; set; }
    public string Name { get; set; } 
}

[RedisPublishMessage()]
public class PubMsg2
{
    public int Id { get; set; }
    public string Name { get; set; }
}
[RedisPublishMessage()]
public class PubMsg3
{
    public int Id { get; set; }
    public string Name { get; set; }
}