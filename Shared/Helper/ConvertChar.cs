
namespace AppShared.Helper;

public static class ConvertChar
{
    public static string FixPersianChars(this string str)
    {
        return str.Replace("ی", "ي")
                  .Replace("ک", "ك")
                  .Replace("‌", "");
    }
}
