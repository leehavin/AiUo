using System.Threading.Tasks;
using AiUo.Net;

namespace AiUo.OAuth.Providers;

internal class GoogleProvider : BaseOAuthProvider
{
    public override OAuthProviders Provider => OAuthProviders.Google;
    protected override string OAuthUrl => "https://accounts.google.com/o/oauth2/v2/auth";

    protected override string TokenUrl => "https://www.googleapis.com/oauth2/v3/token";

    protected override string UserInfoUrl => "https://www.googleapis.com/oauth2/v3/userinfo";

    protected override string AppendOAuthUrl() => "&scope=openid%20email%20profile";

    protected override string AppendUserUrl() => null;
    public override async Task<ResponseResult<OAuthUserDto>> GetUserInfo(OAuthUserIpo ipo)
    {
        return await RequestUser<SuccessRsp, ErrorRsp>(ipo, srsp =>
        {
            return new OAuthUserDto
            {
                OAuthId = srsp.sub,
                UserName = "",
                NickName = srsp.name,
                Avatar = srsp.picture,
                Location = srsp.locale,
                Email = srsp.email,
            };
        });
    }
    class SuccessRsp
    {
        public string sub { get; set; }
        public string name { get; set; }
        public string given_name { get; set; }
        public string family_name { get; set; }
        public string picture { get; set; }
        public string email { get; set; }
        public bool email_verified { get; set; }
        public string locale { get; set; }
    }
    class ErrorRsp 
    {
        public string error { get; set; }
        public string error_description { get; set; }
    }
}
/*
{
"sub": "111727930808116620319",
"name": "高洪锁",
"given_name": "洪锁",
"family_name": "高",
"picture": "https://lh3.googleusercontent.com/a/ACg8ocIJ3_pt_hwOaI47ERCI2v-sd8hwap2BFNnJAYQ6nSUr\u003ds96-c",
"email": "asuoghs@gmail.com",
"email_verified": true,
"locale": "zh-CN"
}
{
"error": "invalid_request",
"error_description": "Invalid Credentials"
}
 */