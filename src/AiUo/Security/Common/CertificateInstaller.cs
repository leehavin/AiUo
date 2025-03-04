using System.Security.Cryptography.X509Certificates;

namespace AiUo.Security;

/// <summary>
/// 证书安装服务工具
/// </summary>
public class CertificateInstaller
{
    private X509Certificate2 _cert;
    private string _subjectName;

    /// <summary>
    /// 证书存储区的位置，默认StoreLocation.LocalMachine
    /// </summary>
    public StoreLocation TargetStoreLocation { get; set; } = StoreLocation.LocalMachine;

    /// <summary>
    /// 证书存储区的名称，默认 StoreName.My
    /// </summary>
    public StoreName TargetStoreName { get; set; } = StoreName.My;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="certFile">证书文件名</param>
    /// <param name="password">证书密码</param>
    /// <param name="flags"></param>
    public CertificateInstaller(string certFile, string password = null, X509KeyStorageFlags flags = X509KeyStorageFlags.PersistKeySet)
    {
        _cert = new X509Certificate2(certFile, password, flags);
        _subjectName = _cert.Subject.Substring(_cert.Subject.IndexOf('=') + 1);
    }

    /// <summary>
    /// 安装证书
    /// </summary>
    public void Install()
    {
        var store = GetStore(OpenFlags.ReadWrite);
        try
        {
            store.Add(_cert);
        }
        finally
        {
            store.Close();
        }
    }

    /// <summary>
    /// 移除证书
    /// </summary>
    public void Remove()
    {
        var store = GetStore(OpenFlags.ReadWrite);
        try
        {
            store.Remove(_cert);
        }
        finally
        {
            store.Close();
        }
    }

    /// <summary>
    /// 检测证书是否存在
    /// </summary>
    /// <returns></returns>
    public bool Exist()
    {
        bool ret = false;
        var store = GetStore(OpenFlags.ReadOnly);
        try
        {
            var certs = store.Certificates.Find(X509FindType.FindBySubjectName, _subjectName, false);
            ret = (certs.Count > 0);
        }
        finally
        {
            store.Close();
        }
        return ret;
    }

    /// <summary>
    /// 检查证书，如不存在则安装
    /// </summary>
    public void Check()
    {
        if (!Exist())
        {
            Install();
        }
    }
    private X509Store GetStore(OpenFlags flags = OpenFlags.ReadOnly)
    {
        X509Store ret = new X509Store(TargetStoreName, TargetStoreLocation);
        ret.Open(flags);
        return ret;
    }
}