namespace AiUo.Security;

/// <summary>
/// 用户类型权限
/// </summary>
public enum UserRole
{
    /// <summary>
    /// 未知
    /// </summary>
    Unknow = 0,
    /// <summary>
    /// 普通用户
    /// </summary>
    User = 1,
    /// <summary>
    /// 开发用户
    /// </summary>
    Dev = 2,
    /// <summary>
    /// 线上测试用户（调用第三方扣减）
    /// </summary>
    Tester = 3,
    /// <summary>
    /// 线上测试用户（不调用第三方扣减）
    /// </summary>
    LocalTester = 4,
    /// <summary>
    /// 仿真测试人员
    /// </summary>
    Staging = 5,
    /// <summary>
    /// 联调测试用户
    /// </summary>
    Debug = 6,
    /// <summary>
    /// 管理员
    /// </summary>
    Admin = 9
}