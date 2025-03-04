namespace AiUo.Configuration;

/// <summary>
/// 项目信息配置节
/// </summary>
public class ProjectSection : ConfigSection
{
    /// <summary>
    /// 配置节名称
    /// </summary>
    public override string SectionName => "Project";

    /// <summary>
    /// 项目标识
    /// </summary>
    public string ProjectId { get; set; }
    /// <summary>
    /// 项目描述
    /// </summary>
    public string Description { get; set; }
    /// <summary>
    /// 应用程序名称(相同名称的项目间数据保护,Session和Cookie共享)
    /// 数据保护共享的ApplicationName（redis）
    /// 如需跨应用共享session或cookie，需设置相同值
    /// </summary>
    public string ApplicationName { get; set; } = "aiuo";
    /// <summary>
    /// 线程池最小线程数（建议：每核cpu配置50，如4核可配置200，一般100-200）
    /// </summary>
    public int MinThreads { get; set; } = 100;
    /// <summary>
    /// 是否返回客户端错误信息(自定义异常和未处理异常的message)
    /// </summary>
    public bool ResponseErrorMessage { get; set; } = true;
    /// <summary>
    /// 是否返回客户端异常详细信息（exception序列化信息）
    /// </summary>
    public bool ResponseErrorDetail { get; set; } = false;

    /// <summary>
    /// 项目环境 EnvironmentNames(dev,sit,fat,uat,pre,pro)
    /// </summary>
    public string Environment { get; set; }
}