namespace Framework.Utils.Helpers;

public static class DateTimeHelper
{
    public static string Today(string format = "yyyyMMdd")
    {
        return DateTime.Now.ToString(format);
    }
}