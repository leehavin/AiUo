using Amazon.Runtime;

namespace AiUo.Extensions.AWS.Common;

internal class AiUoCredentials : AWSCredentials
{
    public string AccessKey { get; private set; }

    public string SecretKey { get; private set; }

    public string Token { get; private set; }
    private ImmutableCredentials _cr;
    public AiUoCredentials(string accessKey, string secretKey, string token)
    {
        AccessKey = accessKey;
        SecretKey = secretKey;
        Token = token;
        _cr = new ImmutableCredentials(AccessKey, SecretKey, Token);
    }

    public override ImmutableCredentials GetCredentials()
    {
        return _cr;
    }
}