namespace MongoSharpen;

public static class DateExtensions
{
    /// <summary>
    /// Convert DateTime to UTC
    /// </summary>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    public static DateTime ToUtc(this DateTime dateTime) =>
        DateTime.SpecifyKind(new DateTime(dateTime.Year,
                dateTime.Month,
                dateTime.Day,
                dateTime.Hour,
                dateTime.Minute,
                dateTime.Second,
                dateTime.Millisecond),
            DateTimeKind.Utc);
}