using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace AiUo.AspNet.Validations.FluentValidation
{
    /// <summary>
    /// FluentValidation 规则扩展方法
    /// </summary>
    public static class FluentValidationRuleExtensions
    {
        /// <summary>
        /// 验证中国身份证号
        /// </summary>
        public static IRuleBuilderOptions<T, string> ChineseIdCard<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder.Must(IsValidChineseIdCard)
                .WithMessage("身份证号格式不正确");
        }

        /// <summary>
        /// 验证中国手机号
        /// </summary>
        public static IRuleBuilderOptions<T, string> ChinesePhone<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder.Matches(@"^1[3-9]\d{9}$")
                .WithMessage("手机号格式不正确");
        }

        /// <summary>
        /// 验证URL格式
        /// </summary>
        public static IRuleBuilderOptions<T, string> Url<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder.Must(url =>
            {
                if (string.IsNullOrEmpty(url)) return true;
                return Uri.TryCreate(url, UriKind.Absolute, out var result) &&
                       (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
            })
            .WithMessage("URL格式不正确");
        }

        /// <summary>
        /// 验证JSON格式
        /// </summary>
        public static IRuleBuilderOptions<T, string> Json<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder.Must(json =>
            {
                if (string.IsNullOrEmpty(json)) return true;
                try
                {
                    System.Text.Json.JsonDocument.Parse(json);
                    return true;
                }
                catch
                {
                    return false;
                }
            })
            .WithMessage("JSON格式不正确");
        }

        /// <summary>
        /// 验证文件扩展名
        /// </summary>
        public static IRuleBuilderOptions<T, string> FileExtension<T>(this IRuleBuilder<T, string> ruleBuilder, params string[] allowedExtensions)
        {
            var extensions = allowedExtensions.Select(ext => ext.ToLowerInvariant()).ToArray();
            return ruleBuilder.Must(fileName =>
            {
                if (string.IsNullOrEmpty(fileName)) return true;
                var extension = System.IO.Path.GetExtension(fileName)?.ToLowerInvariant();
                return extensions.Contains(extension);
            })
            .WithMessage($"文件扩展名不被允许，允许的扩展名：{string.Join(", ", allowedExtensions)}");
        }

        /// <summary>
        /// 验证密码强度
        /// </summary>
        public static IRuleBuilderOptions<T, string> StrongPassword<T>(this IRuleBuilder<T, string> ruleBuilder, 
            int minLength = 8, bool requireUppercase = true, bool requireLowercase = true, 
            bool requireDigit = true, bool requireSpecialChar = true)
        {
            return ruleBuilder.Must(password =>
            {
                if (string.IsNullOrEmpty(password)) return false;
                if (password.Length < minLength) return false;
                if (requireUppercase && !password.Any(char.IsUpper)) return false;
                if (requireLowercase && !password.Any(char.IsLower)) return false;
                if (requireDigit && !password.Any(char.IsDigit)) return false;
                if (requireSpecialChar && !password.Any(ch => !char.IsLetterOrDigit(ch))) return false;
                return true;
            })
            .WithMessage($"密码必须至少{minLength}位，" +
                        (requireUppercase ? "包含大写字母，" : "") +
                        (requireLowercase ? "包含小写字母，" : "") +
                        (requireDigit ? "包含数字，" : "") +
                        (requireSpecialChar ? "包含特殊字符" : ""));
        }

        /// <summary>
        /// 验证IP地址
        /// </summary>
        public static IRuleBuilderOptions<T, string> IpAddress<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder.Must(ip =>
            {
                if (string.IsNullOrEmpty(ip)) return true;
                return System.Net.IPAddress.TryParse(ip, out _);
            })
            .WithMessage("IP地址格式不正确");
        }

        /// <summary>
        /// 验证MAC地址
        /// </summary>
        public static IRuleBuilderOptions<T, string> MacAddress<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder.Matches(@"^([0-9A-Fa-f]{2}[:-]){5}([0-9A-Fa-f]{2})$")
                .WithMessage("MAC地址格式不正确");
        }

        /// <summary>
        /// 验证银行卡号
        /// </summary>
        public static IRuleBuilderOptions<T, string> BankCard<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder.Must(IsValidBankCard)
                .WithMessage("银行卡号格式不正确");
        }

        /// <summary>
        /// 验证中国邮政编码
        /// </summary>
        public static IRuleBuilderOptions<T, string> ChinesePostalCode<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder.Matches(@"^[1-9]\d{5}$")
                .WithMessage("邮政编码格式不正确");
        }

        /// <summary>
        /// 验证QQ号
        /// </summary>
        public static IRuleBuilderOptions<T, string> QQNumber<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder.Matches(@"^[1-9][0-9]{4,10}$")
                .WithMessage("QQ号格式不正确");
        }

        /// <summary>
        /// 验证微信号
        /// </summary>
        public static IRuleBuilderOptions<T, string> WeChatId<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder.Matches(@"^[a-zA-Z]([a-zA-Z0-9_-]){5,19}$")
                .WithMessage("微信号格式不正确");
        }

        /// <summary>
        /// 验证车牌号
        /// </summary>
        public static IRuleBuilderOptions<T, string> ChineseLicensePlate<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder.Matches(@"^[京津沪渝冀豫云辽黑湘皖鲁新苏浙赣鄂桂甘晋蒙陕吉闽贵粤青藏川宁琼使领A-Z]{1}[A-Z]{1}[A-Z0-9]{4}[A-Z0-9挂学警港澳]{1}$")
                .WithMessage("车牌号格式不正确");
        }

        /// <summary>
        /// 验证统一社会信用代码
        /// </summary>
        public static IRuleBuilderOptions<T, string> UnifiedSocialCreditCode<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder.Matches(@"^[0-9A-HJ-NPQRTUWXY]{2}\d{6}[0-9A-HJ-NPQRTUWXY]{10}$")
                .WithMessage("统一社会信用代码格式不正确");
        }

        /// <summary>
        /// 验证集合元素数量
        /// </summary>
        public static IRuleBuilderOptions<T, IEnumerable<TElement>> CollectionCount<T, TElement>(
            this IRuleBuilder<T, IEnumerable<TElement>> ruleBuilder, int minCount, int maxCount = int.MaxValue)
        {
            return ruleBuilder.Must(collection =>
            {
                if (collection == null) return minCount == 0;
                var count = collection.Count();
                return count >= minCount && count <= maxCount;
            })
            .WithMessage($"集合元素数量必须在{minCount}到{maxCount}之间");
        }

        /// <summary>
        /// 验证集合元素唯一性
        /// </summary>
        public static IRuleBuilderOptions<T, IEnumerable<TElement>> UniqueElements<T, TElement>(
            this IRuleBuilder<T, IEnumerable<TElement>> ruleBuilder)
        {
            return ruleBuilder.Must(collection =>
            {
                if (collection == null) return true;
                var list = collection.ToList();
                return list.Count == list.Distinct().Count();
            })
            .WithMessage("集合中存在重复元素");
        }

        /// <summary>
        /// 异步验证唯一性（通常用于数据库查询）
        /// </summary>
        public static IRuleBuilderOptions<T, TProperty> UniqueAsync<T, TProperty>(
            this IRuleBuilder<T, TProperty> ruleBuilder, 
            Func<TProperty, CancellationToken, Task<bool>> uniqueCheck)
        {
            return ruleBuilder.MustAsync(async (value, cancellation) =>
            {
                if (value == null) return true;
                return await uniqueCheck(value, cancellation);
            })
            .WithMessage("该值已存在，请使用其他值");
        }

        /// <summary>
        /// 验证年龄范围
        /// </summary>
        public static IRuleBuilderOptions<T, DateTime> AgeRange<T>(this IRuleBuilder<T, DateTime> ruleBuilder, int minAge, int maxAge)
        {
            return ruleBuilder.Must(birthDate =>
            {
                var age = DateTime.Today.Year - birthDate.Year;
                if (birthDate.Date > DateTime.Today.AddYears(-age)) age--;
                return age >= minAge && age <= maxAge;
            })
            .WithMessage($"年龄必须在{minAge}到{maxAge}岁之间");
        }

        /// <summary>
        /// 验证中国身份证号的私有方法
        /// </summary>
        private static bool IsValidChineseIdCard(string idCard)
        {
            if (string.IsNullOrEmpty(idCard)) return true;
            if (idCard.Length != 18) return false;

            // 验证前17位是否为数字
            for (int i = 0; i < 17; i++)
            {
                if (!char.IsDigit(idCard[i])) return false;
            }

            // 验证最后一位校验码
            var weights = new int[] { 7, 9, 10, 5, 8, 4, 2, 1, 6, 3, 7, 9, 10, 5, 8, 4, 2 };
            var checkCodes = new char[] { '1', '0', 'X', '9', '8', '7', '6', '5', '4', '3', '2' };

            int sum = 0;
            for (int i = 0; i < 17; i++)
            {
                sum += (idCard[i] - '0') * weights[i];
            }

            var checkCode = checkCodes[sum % 11];
            return idCard[17] == checkCode;
        }

        /// <summary>
        /// 验证银行卡号的私有方法（Luhn算法）
        /// </summary>
        private static bool IsValidBankCard(string cardNumber)
        {
            if (string.IsNullOrEmpty(cardNumber)) return true;
            if (!cardNumber.All(char.IsDigit)) return false;
            if (cardNumber.Length < 13 || cardNumber.Length > 19) return false;

            int sum = 0;
            bool alternate = false;
            for (int i = cardNumber.Length - 1; i >= 0; i--)
            {
                int digit = cardNumber[i] - '0';
                if (alternate)
                {
                    digit *= 2;
                    if (digit > 9) digit -= 9;
                }
                sum += digit;
                alternate = !alternate;
            }
            return sum % 10 == 0;
        }
    }

    /// <summary>
    /// 验证结果扩展方法
    /// </summary>
    public static class ValidationResultExtensions
    {
        /// <summary>
        /// 获取第一个错误消息
        /// </summary>
        public static string GetFirstErrorMessage(this FluentValidationResult result)
        {
            return result.Errors?.FirstOrDefault()?.ErrorMessage;
        }

        /// <summary>
        /// 获取指定属性的错误消息
        /// </summary>
        public static IEnumerable<string> GetErrorMessages(this FluentValidationResult result, string propertyName)
        {
            return result.Errors?
                .Where(e => e.PropertyName.Equals(propertyName, StringComparison.OrdinalIgnoreCase))
                .Select(e => e.ErrorMessage) ?? Enumerable.Empty<string>();
        }

        /// <summary>
        /// 转换为字典格式
        /// </summary>
        public static Dictionary<string, List<string>> ToDictionary(this FluentValidationResult result)
        {
            var dict = new Dictionary<string, List<string>>();
            if (result.Errors != null)
            {
                foreach (var error in result.Errors)
                {
                    if (!dict.ContainsKey(error.PropertyName))
                    {
                        dict[error.PropertyName] = new List<string>();
                    }
                    dict[error.PropertyName].Add(error.ErrorMessage);
                }
            }
            return dict;
        }

        /// <summary>
        /// 转换为简单的错误消息列表
        /// </summary>
        public static List<string> ToErrorMessages(this FluentValidationResult result)
        {
            return result.Errors?.Select(e => e.ErrorMessage).ToList() ?? new List<string>();
        }
    }
}