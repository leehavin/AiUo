using StackExchange.Redis;

namespace AiUo.Extensions.StackExchangeRedis.Clients;

/// <summary>
/// Redis string结构Bit操作,可以被继承
/// </summary>
public class RedisBitClient : RedisClientBase
{
    public override RedisType RedisType => RedisType.String;
    public RedisBitClient() { }

    /// <summary>
    /// [SETBIT]设置或清除键中存储的字符串值中偏移处的位
    /// </summary>
    /// <param name="offset"></param>
    /// <param name="bit"></param>
    /// <param name="flags"></param>
    /// <returns></returns>
    public async Task SetBitAsync(long offset, bool bit, CommandFlags flags = CommandFlags.None)
        => await Database.StringSetBitAsync(RedisKey, offset, bit, flags);

    /// <summary>
    /// [GETBIT]返回 key 存储的字符串值中偏移处的位值
    /// </summary>
    /// <param name="offset"></param>
    /// <param name="flags"></param>
    /// <returns></returns>
    public async Task<bool> GetBitAsync(long offset, CommandFlags flags = CommandFlags.None)
        => await Database.StringGetBitAsync(RedisKey, offset, flags);

    /// <summary>
    /// [BITCOUNT]计算字符串中设置位的数量（总体计数）
    /// </summary>
    /// <param name="start">字节为单位，支持负数（从末尾索引字节），-1最后一个字节，-2倒数第二个字节</param>
    /// <param name="end">同start</param>
    /// <param name="type">指定位索引还是字节索引</param>
    /// <param name="flags"></param>
    /// <returns></returns>
    public async Task<long> BitCountAsync(long start = 0, long end = -1, StringIndexType type = StringIndexType.Byte, CommandFlags flags = CommandFlags.None)
        => await Database.StringBitCountAsync(RedisKey, start, end, type, flags);

    /// <summary>
    /// [BITPOS]返回字符串中从左向右第一个设置为 1 或 0 的位的位置
    /// </summary>
    /// <param name="bit"></param>
    /// <param name="start">字节为单位，支持负数（从末尾索引字节），-1最后一个字节，-2倒数第二个字节</param>
    /// <param name="end">同start</param>
    /// <param name="type"></param>
    /// <param name="flags"></param>
    /// <returns>如果我们查找设置位（位参数为 1）并且字符串为空或仅由零字节组成，则返回 -1</returns>
    public async Task<long> BitPositionAsync(bool bit, long start = 0, long end = -1, StringIndexType type = StringIndexType.Byte, CommandFlags flags = CommandFlags.None)
        => await Database.StringBitPositionAsync(RedisKey, bit, start, end, type, flags);

    /// <summary>
    /// [BITOP]在多个键（包含字符串值）之间执行按位运算，并将结果存储到当前键中
    /// </summary>
    /// <param name="operation">NOT 是一元运算符，此时会忽略第二个键</param>
    /// <param name="first"></param>
    /// <param name="second"></param>
    /// <param name="flags"></param>
    /// <returns></returns>
    public async Task<long> BitOperationAsync(Bitwise operation, RedisKey first, RedisKey second = default, CommandFlags flags = CommandFlags.None)
        => await Database.StringBitOperationAsync(operation, RedisKey, first, second, flags);

    /// <summary>
    /// [BITOP]在多个键（包含字符串值）之间执行按位运算，并将结果存储到当前键中
    /// </summary>
    /// <param name="operation">NOT 是一元运算符，此时会忽略第二个键</param>
    /// <param name="keys"></param>
    /// <param name="flags"></param>
    /// <returns></returns>
    public async Task<long> BitOperationAsync(Bitwise operation, IEnumerable<RedisKey> keys, CommandFlags flags = CommandFlags.None)
        => await Database.StringBitOperationAsync(operation, RedisKey, keys.ToArray(), flags);
}