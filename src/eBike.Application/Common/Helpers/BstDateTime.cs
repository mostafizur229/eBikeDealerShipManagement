namespace eBike.Application.Common.Helpers;

public static class BstDateTime
{
    private static readonly TimeZoneInfo _bstTimeZone;

    static BstDateTime()
    {
        try
        {
            _bstTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Bangladesh Standard Time");
        }
        catch (TimeZoneNotFoundException)
        {
            try
            {
                _bstTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Dhaka");
            }
            catch (Exception)
            {
                _bstTimeZone = TimeZoneInfo.CreateCustomTimeZone("BST", TimeSpan.FromHours(6), "Bangladesh Standard Time", "Bangladesh Standard Time");
            }
        }
    }

    public static TimeZoneInfo TimeZone => _bstTimeZone;

    public static DateTime Now => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _bstTimeZone);
    public static DateTime UtcNow => DateTime.UtcNow;

    public static DateTime ToBst(DateTime utcDateTime)
    {
        var utc = utcDateTime.Kind == DateTimeKind.Utc
            ? utcDateTime
            : DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);

        return TimeZoneInfo.ConvertTimeFromUtc(utc, _bstTimeZone);
    }

    public static DateTime ToUtc(DateTime bstDateTime)
    {
        var unspecified = DateTime.SpecifyKind(bstDateTime, DateTimeKind.Unspecified);
        return TimeZoneInfo.ConvertTimeToUtc(unspecified, _bstTimeZone);
    }

    public static string FormatBst(DateTime dateTime, string format = "yyyy-MM-dd HH:mm:ss")
    {
        var bst = dateTime.Kind == DateTimeKind.Utc ? ToBst(dateTime) : dateTime;
        return bst.ToString(format);
    }
}
