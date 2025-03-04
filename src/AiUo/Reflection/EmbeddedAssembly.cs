using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Cryptography;
using System.IO;
using AiUo.IO;

namespace AiUo.Reflection;

/// <summary>
/// 加载嵌入式Assembly类（将程序集以资源的方式嵌入到当前程序集，使用的时候释放出来）
/// TODO: EmbeddedAssembly未处理
/// </summary>
public class EmbeddedAssembly
{
    private static Dictionary<string, Assembly> _dic = new Dictionary<string, Assembly>();

    /// <summary>
    /// 从嵌入资源加载程序集到内存
    /// </summary>
    /// <param name="embeddedResource"></param>
    /// <param name="fileName"></param>
    public static void Load(string embeddedResource, string fileName)
    {
        byte[] data = null;
        Assembly asm = null;
        Assembly curAsm = Assembly.GetExecutingAssembly();
        using (Stream stm = curAsm.GetManifestResourceStream(embeddedResource))
        {
            if (stm == null)
                throw new Exception(embeddedResource + "嵌入资源未找到。");

            data = IOUtil.ReadStreamToBytes(stm);
            try
            {
                asm = Assembly.Load(data);
                _dic.Add(asm.FullName, asm);
                return;
            }
            catch { /* 非托管DLL无法通过字节加载 */ }
        }

        bool fileExist = false;
        string tempFile = Path.GetTempPath() + fileName;

        using (var sha1 = SHA1.Create())
        {
            string fileHash = BitConverter.ToString(sha1.ComputeHash(data)).Replace("-", string.Empty);
            if (File.Exists(tempFile))
            {
                byte[] data2 = File.ReadAllBytes(tempFile);
                string fileHash2 = BitConverter.ToString(sha1.ComputeHash(data2)).Replace("-", string.Empty);
                fileExist = (fileHash == fileHash2);//判断文件是否相同
            }
            else
            {
                fileExist = false;
            }
        }

        //不存在创建
        if (!fileExist)
        {
            System.IO.File.WriteAllBytes(tempFile, data);
        }

        asm = Assembly.LoadFile(tempFile);
        _dic.Add(asm.FullName, asm);
    }

    /// <summary>
    /// 获取已加载到内存中的程序集
    /// </summary>
    /// <param name="assemblyFullName"></param>
    /// <returns></returns>
    public static Assembly Get(string assemblyFullName)
    {
        Assembly ret = null;
        if (_dic != null && _dic.ContainsKey(assemblyFullName))
            ret = _dic[assemblyFullName];
        return ret;
    }
}