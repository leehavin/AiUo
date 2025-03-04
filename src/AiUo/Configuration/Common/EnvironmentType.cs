using System.ComponentModel;

namespace AiUo.Configuration;

/// <summary>
/// 软件开发环境
/// </summary>
public enum EnvironmentType
{
    /// <summary>
    /// dev 开发环境
    /// </summary>
    [Description("开发环境")]
    DEV = 0,
    /// <summary>
    /// sit 系统集成测试，开发人员自己测试流程是否走通
    /// </summary>
    [Description("集成环境")]
    SIT = 1,
    /// <summary>
    /// fat 测试人员测试环境
    /// </summary>
    [Description("测试环境")]
    FAT = 2,
    /// <summary>
    /// uat 用户验收、仿真环境
    /// </summary>
    [Description("仿真环境")]
    UAT = 3,
    /// <summary>
    /// pre 灰度环境
    /// </summary>
    [Description("灰度环境")]
    PRE = 4,
    /// <summary>
    /// pro 生产环境
    /// </summary>
    [Description("生产环境")]
    PRO = 5
}