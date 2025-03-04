namespace AiUo.HCaptcha;

public class HCaptchaVerifyCodes
{
    public const string BAD_REQUEST = "bad-request";
    public const string MISSING_INPUT_SECRET = "missing-input-secret";
    public const string INVALID_INPUT_SECRET = "invalid-input-secret";
    public const string MISSING_INPUT_RESPONSE = "missing-input-response";
    public const string INVALID_INPUT_RESPONSE = "invalid-input-response";
    public const string INVALID_OR_ALREADY_SEEN_RESPONSE = "invalid-or-already-seen-response";
    public const string NOT_USING_DUMMY_PASSCODE = "not-using-dummy-passcode";
    public const string SITEKEY_SECRET_MISMATCH = "sitekey-secret-mismatch";
}