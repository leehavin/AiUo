namespace AiUo.AspNet.Validations.FluentValidation.Services;

/// <summary>
/// FluentValidation的配置选项
/// </summary>
public sealed class FluentValidationOptions
{
    /// <summary>
    /// 获取或设置验证失败的默认错误代码
    /// </summary>
    public string DefaultErrorCode { get; set; } = "VALIDATION_ERROR";
    
    /// <summary>
    /// 获取或设置是否在第一次失败时停止验证
    /// </summary>
    public bool StopOnFirstFailure { get; set; } = false;
    
    /// <summary>
    /// 获取或设置要收集的验证错误的最大数量
    /// </summary>
    public int MaxErrors { get; set; } = 100;
    
    /// <summary>
    /// 获取或设置是否启用性能监控
    /// </summary>
    public bool EnablePerformanceMonitoring { get; set; } = false;
    
    /// <summary>
    /// 获取或设置性能监控阈值（毫秒）
    /// </summary>
    public int PerformanceThresholdMs { get; set; } = 1000;
    
    /// <summary>
    /// 验证配置选项
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when configuration is invalid</exception>
    public void Validate()
    {
        if (MaxErrors <= 0)
            throw new ArgumentException("MaxErrors must be greater than 0", nameof(MaxErrors));
            
        if (string.IsNullOrWhiteSpace(DefaultErrorCode))
            throw new ArgumentException("DefaultErrorCode cannot be null or empty", nameof(DefaultErrorCode));
            
        if (PerformanceThresholdMs < 0)
            throw new ArgumentException("PerformanceThresholdMs cannot be negative", nameof(PerformanceThresholdMs));
    }
}