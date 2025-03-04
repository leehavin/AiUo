using Renci.SshNet;
using System.IO;
using System.Text;
using AiUo.Extensions.SshNet.Common;

namespace AiUo.Extensions.SshNet;

public static class SshUtil
{
    /// <summary>
    /// 获得追加或修改文件内容的sudo命令
    /// </summary>
    /// <param name="content"></param>
    /// <param name="file"></param>
    /// <param name="isAppend"></param>
    /// <returns></returns>
    public static string GetSudoEchoCmdString(string content, string file, bool isAppend=true)
    {
        var append = isAppend ? ">>" : ">";
        content = content.Replace("\"","\\\"\\\\\\\"").Replace("$", "\\\\\\$");
        var ret = $"sudo sh -c \"echo {content} {append} {file}\"";
        return ret;
    }

    /// <summary>
    /// 使用rsa-sha2-256 pem创建ConnectionInfo
    /// </summary>
    /// <param name="host"></param>
    /// <param name="port"></param>
    /// <param name="user"></param>
    /// <param name="pemContent"></param>
    /// <returns></returns>
    public static ConnectionInfo BuildConnectionInnfoUseRsaSha256(string host, int port, string user, string pemContent)
    {
        var ms = new MemoryStream(Encoding.UTF8.GetBytes(pemContent));
        var pk = new PrivateKeyFile(ms);
        RsaSha256Util.ConvertToKeyWithSha256Signature(pk);
        var rsa = new PrivateKeyAuthenticationMethod(user, pk);
        var conn = new ConnectionInfo(host, port, user, rsa);
        RsaSha256Util.SetupConnection(conn); // adds rsa-sha2-256
        return conn;
    }
}