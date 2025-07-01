using FluentValidation;
using FluentValidationDemo.NetStandard.Models;
using System;
using System.Text.RegularExpressions;

namespace FluentValidationDemo.NetStandard.Validators
{
    /// <summary>
    /// 订单模型验证器
    /// </summary>
    public class OrderModelValidator : AbstractValidator<OrderModel>
    {
        public OrderModelValidator()
        {
            // 订单号验证
            RuleFor(x => x.OrderNo)
                .NotEmpty().WithMessage("订单号不能为空").WithErrorCode("ORDER_NO_REQUIRED")
                .Length(10, 50).WithMessage("订单号长度必须在10-50个字符之间").WithErrorCode("ORDER_NO_LENGTH")
                .Matches(@"^[A-Z0-9]+$").WithMessage("订单号只能包含大写字母和数字").WithErrorCode("ORDER_NO_FORMAT");

            // 客户姓名验证
            RuleFor(x => x.CustomerName)
                .NotEmpty().WithMessage("客户姓名不能为空").WithErrorCode("CUSTOMER_NAME_REQUIRED")
                .Length(2, 50).WithMessage("客户姓名长度必须在2-50个字符之间").WithErrorCode("CUSTOMER_NAME_LENGTH");

            // 客户邮箱验证
            RuleFor(x => x.CustomerEmail)
                .NotEmpty().WithMessage("客户邮箱不能为空").WithErrorCode("CUSTOMER_EMAIL_REQUIRED")
                .EmailAddress().WithMessage("客户邮箱格式不正确").WithErrorCode("CUSTOMER_EMAIL_FORMAT");

            // 客户电话验证
            RuleFor(x => x.CustomerPhone)
                .NotEmpty().WithMessage("客户电话不能为空").WithErrorCode("CUSTOMER_PHONE_REQUIRED")
                .Must(BeValidPhone).WithMessage("客户电话格式不正确").WithErrorCode("CUSTOMER_PHONE_FORMAT");

            // 收货地址验证
            RuleFor(x => x.ShippingAddress)
                .NotEmpty().WithMessage("收货地址不能为空").WithErrorCode("SHIPPING_ADDRESS_REQUIRED")
                .Length(10, 200).WithMessage("收货地址长度必须在10-200个字符之间").WithErrorCode("SHIPPING_ADDRESS_LENGTH");

            // 订单总金额验证
            RuleFor(x => x.TotalAmount)
                .GreaterThan(0).WithMessage("订单总金额必须大于0").WithErrorCode("TOTAL_AMOUNT_RANGE")
                .LessThanOrEqualTo(999999.99m).WithMessage("订单总金额不能超过999999.99").WithErrorCode("TOTAL_AMOUNT_MAX");

            // 订单状态验证
            RuleFor(x => x.Status)
                .InclusiveBetween(0, 9).WithMessage("订单状态值必须在0-9之间").WithErrorCode("ORDER_STATUS_RANGE");

            // 备注验证（可选）
            RuleFor(x => x.Remarks)
                .MaximumLength(500).WithMessage("备注不能超过500个字符").WithErrorCode("REMARKS_LENGTH")
                .When(x => !string.IsNullOrEmpty(x.Remarks));

            // 下单时间验证
            RuleFor(x => x.OrderTime)
                .LessThanOrEqualTo(DateTime.Now.AddMinutes(5)).WithMessage("下单时间不能是未来时间").WithErrorCode("ORDER_TIME_RANGE")
                .GreaterThan(new DateTime(2020, 1, 1)).WithMessage("下单时间不能早于2020年1月1日").WithErrorCode("ORDER_TIME_MIN");
        }

        /// <summary>
        /// 验证电话号码格式（支持手机号和固话）
        /// </summary>
        private bool BeValidPhone(string phone)
        {
            if (string.IsNullOrEmpty(phone))
                return false;

            // 手机号格式：1开头的11位数字
            var mobilePattern = @"^1[3-9]\d{9}$";
            // 固话格式：区号-号码 或 区号号码
            var landlinePattern = @"^(0\d{2,3}[-]?\d{7,8})$";

            return Regex.IsMatch(phone, mobilePattern) || Regex.IsMatch(phone, landlinePattern);
        }
    }
}