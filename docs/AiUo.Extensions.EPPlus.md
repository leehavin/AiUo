# AiUo.Extensions.EPPlus 模块

[![NuGet](https://img.shields.io/nuget/v/AiUo.Extensions.EPPlus.svg)](https://www.nuget.org/packages/AiUo.Extensions.EPPlus)
[![NuGet Downloads](https://img.shields.io/nuget/dt/AiUo.Extensions.EPPlus.svg)](https://www.nuget.org/packages/AiUo.Extensions.EPPlus)
[![License](https://img.shields.io/github/license/AiUo/AiUo.svg)](https://github.com/AiUo/AiUo/blob/main/LICENSE)

## 📖 概述

AiUo.Extensions.EPPlus 是对 EPPlus 库的扩展封装，提供了更简便的 Excel 文件操作方式。该模块简化了 Excel 文件的读写、格式化和数据处理操作，支持复杂的 Excel 模板和大数据量的高效处理。

## 🚀 快速开始

### 📦 安装

选择以下方式之一安装 AiUo.Extensions.EPPlus 模块：

#### .NET CLI

```bash
dotnet add package AiUo.Extensions.EPPlus
```

#### Package Manager

```powershell
Install-Package AiUo.Extensions.EPPlus
```

### ⚙️ 基本配置

```csharp
using AiUo.Extensions.EPPlus;

// 在 Program.cs 中配置服务
var builder = WebApplication.CreateBuilder(args);

// 添加 EPPlus 服务
builder.Services.AddEPPlus(options =>
{
    // 配置 EPPlus 许可证上下文
    options.LicenseContext = LicenseContext.NonCommercial;
    
    // 配置默认样式
    options.DefaultStyle = new ExcelStyle
    {
        Font = "微软雅黑",
        FontSize = 11
    };
});
```

## 🎯 主要功能

### 📑 Excel 文件操作
- 高效的 Excel 文件读写能力
- 完整的工作表管理功能
- 灵活的单元格操作支持
- 强大的样式设置功能

### 🔄 数据导入导出
- 智能的集合对象与 Excel 转换
- 可定制的数据映射规则
- 高性能的批量数据处理
- 完善的数据验证机制

### 📋 模板操作
- 便捷的模板文件生成
- 灵活的占位符替换
- 强大的模板结构支持
- 高效的区域数据填充

### 🎨 格式化与样式
- 丰富的单元格格式化选项
- 完整的条件格式支持
- 美观的表格样式定义
- 专业的图表生成功能

## 💡 使用示例

### 📝 创建 Excel 文件

```csharp
using AiUo.Extensions.EPPlus;

public class ExcelService
{
    private readonly IExcelHelper _excelHelper;

    public ExcelService(IExcelHelper excelHelper)
    {
        _excelHelper = excelHelper;
    }

    public void CreateExcelFile(string filePath)
    {
        using (var excel = _excelHelper.Create())
        {
            // 添加工作表
            var sheet = excel.AddSheet("Sheet1");
            
            // 设置单元格值
            sheet.SetValue("A1", "姓名");
            sheet.SetValue("B1", "年龄");
            sheet.SetValue("C1", "部门");
            
            // 设置样式
            sheet.SetHeaderStyle("A1:C1");
            
            // 保存文件
            excel.SaveAs(filePath);
        }
    }
}
```

### 📤 数据导出

```csharp
public class DataExportService
{
    private readonly IExcelHelper _excelHelper;

    public DataExportService(IExcelHelper excelHelper)
    {
        _excelHelper = excelHelper;
    }

    public void ExportToExcel<T>(List<T> data, string filePath) where T : class
    {
        using (var excel = _excelHelper.Create())
        {
            // 将集合数据导出到 Excel
            excel.ExportToSheet(data, "数据");
            
            // 自定义列标题
            excel.SetColumnHeaders(new Dictionary<string, string>
            {
                { "Name", "姓名" },
                { "Age", "年龄" },
                { "Department", "部门" }
            });
            
            // 保存文件
            excel.SaveAs(filePath);
        }
    }
}
```

## 📚 更多资源

- [API 文档](https://docs.aiuo.com/api/epplus)
- [示例代码](https://github.com/AiUo/AiUo/tree/main/samples/EPPlus)
- [贡献指南](https://github.com/AiUo/AiUo/blob/main/CONTRIBUTING.md)

## 📄 许可证

本项目采用 MIT 许可证 - 查看 [LICENSE](https://github.com/AiUo/AiUo/blob/main/LICENSE) 文件了解更多详情。