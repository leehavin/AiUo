namespace AiUo.AspNet;

/// <summary>
/// 客户端同步通知服务
/// </summary>
public interface IClientSyncNotifyProvider
{
    /// <summary>
    /// 添加通知项Id集合
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="notifyIds">需要通知的项编码</param>
    /// <returns></returns>
    Task AddNotify(string userId, params int[] notifyIds);

    /// <summary>
    /// 获取返回给客户端的通知值
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    Task<string> GetNotifyValue(string userId);

    /// <summary>
    /// 解析通知值获得通知项Id集合
    /// </summary>
    /// <param name="notifyValue">通知值</param>
    /// <returns></returns>
    Task<List<int>> ParseNotifyIds(string notifyValue);
}