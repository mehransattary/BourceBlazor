using System.Globalization;

namespace AppShared.Helper;
public static class DateTimeExtensions
{
    public static string ToShamsi(this string value)
    {
        PersianCalendar pc = new PersianCalendar();
        return pc.GetYear(Convert.ToDateTime(value)) + "/" + pc.GetMonth(Convert.ToDateTime(value)).ToString("00") + "/" +
               pc.GetDayOfMonth(Convert.ToDateTime(value)).ToString("00");
    }
    public static string ToShamsi(this DateTime value)
    {
        PersianCalendar pc = new PersianCalendar();
        return pc.GetYear(value) + "/" + pc.GetMonth(value).ToString("00") + "/" +
               pc.GetDayOfMonth(value).ToString("00");
    }
    public static string ToTime(this int time) //122959 - 40503
    {
        string numericTimeStr = time.ToString();
        if (!(numericTimeStr.Length > 5))
        {
            numericTimeStr = "0" + numericTimeStr;
        }
        string hour = numericTimeStr.Substring(0, 2);
        string minites = numericTimeStr.Substring(2, 2);
        string second = numericTimeStr.Substring(4, 2);

        var date = hour + ":" + minites + ":" + second;
        return date;
    }
    public static string ToPersianDate(this int numericDate)
    {
        string numericDateStr = numericDate.ToString();
        string year = numericDateStr.Substring(0, 4);
        string month = numericDateStr.Substring(4, 2);
        string day = numericDateStr.Substring(6, 2);

        int parsedYear = int.Parse(year);
        int parsedMonth = int.Parse(month);
        int parsedDay = int.Parse(day);

        PersianCalendar pc = new PersianCalendar();

        var date = parsedYear + "-" + parsedMonth + "-" + parsedDay;
        var value = Convert.ToDateTime(date);

        var res = pc.GetYear(value) + "/" + pc.GetMonth(value).ToString("00") + "/" + pc.GetDayOfMonth(value).ToString("00");
        return res;
    }
}