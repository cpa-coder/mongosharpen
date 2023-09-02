namespace MongoSharpen;

public static class DateExtensions
{
    /// <summary>
    /// Convert DateTime to UTC
    /// </summary>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    public static DateTime ToUtc(this DateTime dateTime) => DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);

  
}