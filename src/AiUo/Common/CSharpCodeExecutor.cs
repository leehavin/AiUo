using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;

namespace AiUo.Common;

/// <summary>
/// C#代码动态编译执行
/// </summary>
public class CSharpCodeExecutor
{
    private string _code;
    private List<MetadataReference> _references = new List<MetadataReference>();
    private List<string> _usings = new List<string>();
    public Assembly CodeAssembly { get; private set; }
        
    public CSharpCodeExecutor(string code)
    {
        _code = code;
    }

    #region AddReference & AddUsing
    /// <summary>
    /// 添加引用
    /// </summary>
    /// <param name="dllName">dll名称，如:System.Data.SqlClient</param>
    public void AddReferences(string dllName)
        => _references.Add(MetadataReference.CreateFromFile(Assembly.Load(new AssemblyName(dllName)).Location));
    public void AddReferences(List<string> dllNames)
        => dllNames.ForEach(x => AddReferences(x));
    public void AddReferences(MetadataReference reference)
        => _references.Add(reference);

    /// <summary>
    /// 等同于using xxx
    /// </summary>
    /// <param name="usingString"></param>
    public void AddUsing(string usingString)
        => _usings.Add(usingString);
    public void AddUsing(List<string> usingStrings)
        => _usings.AddRange(usingStrings);
    #endregion

    #region Execute
    /// <summary>
    /// 执行动态代码中的方法
    /// </summary>
    /// <param name="typeName"></param>
    /// <param name="methodName"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public object ExecuteMethod(string typeName, string methodName, params object[] parameters)
    {
        var type = GetCodeAssembly().GetType(typeName);
        var obj = Activator.CreateInstance(type);
        return type.InvokeMember(methodName, BindingFlags.Default | BindingFlags.InvokeMethod, null, obj, parameters);
    }

    /// <summary>
    /// 执行动态代码中的方法
    /// </summary>
    /// <param name="typeName">类名</param>
    /// <param name="methodName">方法名</param>
    /// <param name="parameters">执行参数</param>
    /// <returns></returns>
    public object ExecuteStaticMethod(string typeName, string methodName, params object[] parameters)
    {
        var method = GetCodeAssembly().CreateInstance(typeName).GetType().GetMethod(methodName);
        return method.Invoke(null, parameters);
    }
    #endregion

    #region Utils
    private Assembly GetCodeAssembly()
    {
        if (CodeAssembly == null)
            Build();
        return CodeAssembly;
    }
    private void Build()
    {
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            _references.Add(MetadataReference.CreateFromFile(asm.Location));
        }
        // 随机程序集名称
        string assemblyName = Path.GetRandomFileName();
        // 丛代码中转换表达式树
        var syntaxTree = CSharpSyntaxTree.ParseText(_code);
        var compiler = CSharpCompilation.Create(
            assemblyName,
            syntaxTrees: new[] { syntaxTree },
            references: _references,
            options: new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary, 
                usings: _usings)
        );
        using (var ms = new MemoryStream())
        {
            var result = compiler.Emit(ms);
            if (!result.Success || result.Diagnostics.FirstOrDefault(x => x.Severity > 0) != null)
            {
                var msg = string.Join(Environment.NewLine, result.Diagnostics.Select(item => item.GetMessage()));
                throw new Exception(msg);
            }
            ms.Seek(0, SeekOrigin.Begin);
            CodeAssembly = Assembly.Load(ms.ToArray());
        }
    }
    #endregion
}