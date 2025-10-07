namespace Sales.Application.Utils;

public class CommonData
{
    public static string GetTimestamp()
    {
        return TimeZoneInfo
            .ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time"))
            .ToString("yyyy-MM-dd'T'HH:mm:ss");
    }
}