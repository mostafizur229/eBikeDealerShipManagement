using System.Text.RegularExpressions;

namespace eBike.Application.Common.Helpers;

public static class BdPhoneValidator
{
    private static readonly Regex BdMobileRegex = new(
        @"^(?:\+?8801|01)[3-9]\d{8}$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant
    );

    private static readonly Regex BdPhoneOrLandlineRegex = new(
        @"^(?:\+?880|0)(?:1[3-9]\d{8}|[2-9]\d{6,8})$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant
    );

    public static bool IsValidBdMobile(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return false;
        string clean = Regex.Replace(input.Trim(), @"[\s\-\(\)]", "");
        return BdMobileRegex.IsMatch(clean);
    }

    public static bool IsValidBdPhone(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return false;
        string clean = Regex.Replace(input.Trim(), @"[\s\-\(\)]", "");
        return BdPhoneOrLandlineRegex.IsMatch(clean);
    }

    public static string NormalizeBdMobile(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;
        string clean = Regex.Replace(input.Trim(), @"[\s\-\(\)]", "");
        if (clean.StartsWith("+88"))
        {
            return clean;
        }
        if (clean.StartsWith("880"))
        {
            return "+" + clean;
        }
        return clean;
    }
}
