namespace eBike.Application.Common.Helpers;

public static class NumberToWordsConverter
{
    private static readonly string[] Units =
    {
        "Zero", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine", "Ten",
        "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen", "Seventeen", "Eighteen", "Nineteen"
    };

    private static readonly string[] Tens =
    {
        "Zero", "Ten", "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety"
    };

    /// <summary>
    /// Converts a numeric amount to words using the Bangladeshi currency numbering system (Crore, Lakh, Thousand, Hundred, Taka, Poisa).
    /// Example: 15275 => "Fifteen Thousand Two Hundred Seventy Five Taka Only"
    /// </summary>
    public static string ToTakaWords(decimal amount)
    {
        if (amount == 0) return "Zero Taka Only";

        long takaPart = (long)Math.Floor(Math.Abs(amount));
        int poisaPart = (int)Math.Round((Math.Abs(amount) - takaPart) * 100);

        string takaWords = ConvertNumberToWords(takaPart).Trim();
        string result = string.Empty;

        if (takaPart > 0)
        {
            result = takaWords + " Taka";
        }

        if (poisaPart > 0)
        {
            string poisaWords = ConvertNumberToWords(poisaPart).Trim();
            if (takaPart > 0)
            {
                result += " and " + poisaWords + " Poisa";
            }
            else
            {
                result = poisaWords + " Poisa";
            }
        }

        return result + " Only";
    }

    private static string ConvertNumberToWords(long number)
    {
        if (number == 0) return "";

        if (number >= 10000000)
        {
            long crore = number / 10000000;
            long remainder = number % 10000000;
            string remStr = remainder > 0 ? " " + ConvertNumberToWords(remainder) : "";
            return $"{ConvertNumberToWords(crore)} Crore{remStr}";
        }

        if (number >= 100000)
        {
            long lakh = number / 100000;
            long remainder = number % 100000;
            string remStr = remainder > 0 ? " " + ConvertNumberToWords(remainder) : "";
            return $"{ConvertNumberToWords(lakh)} Lakh{remStr}";
        }

        if (number >= 1000)
        {
            long thousand = number / 1000;
            long remainder = number % 1000;
            string remStr = remainder > 0 ? " " + ConvertNumberToWords(remainder) : "";
            return $"{ConvertNumberToWords(thousand)} Thousand{remStr}";
        }

        if (number >= 100)
        {
            long hundred = number / 100;
            long remainder = number % 100;
            string remStr = remainder > 0 ? " " + ConvertNumberToWords(remainder) : "";
            return $"{ConvertNumberToWords(hundred)} Hundred{remStr}";
        }

        if (number >= 20)
        {
            long ten = number / 10;
            long unit = number % 10;
            return unit > 0 ? $"{Tens[ten]} {Units[unit]}" : Tens[ten];
        }

        return Units[number];
    }
}
