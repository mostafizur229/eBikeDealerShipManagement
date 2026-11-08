using eBike.Application.Common.Helpers;
using FluentValidation;

namespace eBike.Application.Common.Validation;

public static class BdPhoneValidationExtensions
{
    public static IRuleBuilderOptions<T, string?> ValidBdMobile<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .Must(mobile => string.IsNullOrWhiteSpace(mobile) || BdPhoneValidator.IsValidBdMobile(mobile))
            .WithMessage("Enter a valid Bangladeshi mobile number (e.g. 01712345678 or +8801712345678).");
    }

    public static IRuleBuilderOptions<T, string?> ValidRequiredBdMobile<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty().WithMessage("Mobile number is required.")
            .Must(BdPhoneValidator.IsValidBdMobile)
            .WithMessage("Enter a valid Bangladeshi mobile number (e.g. 01712345678 or +8801712345678).");
    }

    public static IRuleBuilderOptions<T, string?> ValidBdPhone<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .Must(phone => string.IsNullOrWhiteSpace(phone) || BdPhoneValidator.IsValidBdPhone(phone))
            .WithMessage("Enter a valid Bangladeshi phone number (e.g. 01712345678, 02-9568201, or +880...).");
    }
}
