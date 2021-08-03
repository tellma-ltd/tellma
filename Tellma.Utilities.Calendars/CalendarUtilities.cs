using Microsoft.Extensions.Localization;
using System;
using System.Text;
namespace Tellma.Utilities.Calendars
{
    /// <summary>
    /// Contains convenience functions for calendar conversion and formatting.
    /// </summary>
    public static class CalendarUtilities
    {
        #region JDN Functions

        /// <summary>
        /// Converts from Gregorian Calendar to Julian Day Number.
        /// https://quasar.as.utexas.edu/BillInfo/JulianDatesG.html
        /// </summary>
        public static int DateTimeToJdn(DateTime time)
        {
            decimal d = time.Day;
            decimal m = time.Month;
            decimal y = time.Year;

            // If Jan or Feb
            if (m <= 2)
            {
                y--;
                m += 12;
            }

            decimal a = Math.Floor(y / 100);
            decimal b = Math.Floor(a / 4);
            decimal c = 2 - a + b;
            decimal e = Math.Floor(365.25m * (y + 4716));
            decimal f = Math.Floor(30.6001m * (m + 1));

            decimal jdn = c + d + e + f - 1524m;
            return (int)jdn;
        }

        /// <summary>
        /// Converts from Julian Day Number (JDN) to Gregorian Calendar.
        /// https://quasar.as.utexas.edu/BillInfo/JulianDatesG.html
        /// </summary>
        public static DateTime JdnToDateTime(int jdn, int hour, int minute, int second, int millisecond)
        {
            //decimal q = jdn;
            decimal z = jdn;
            decimal w = Math.Floor((z - 1867216.25m) / 36524.25m);
            decimal x = Math.Floor(w / 4m);
            decimal a = z + 1 + w - x;
            decimal b = a + 1524m;
            decimal c = Math.Floor((b - 122.1m) / 365.25m);
            decimal d = Math.Floor(365.25m * c);
            decimal e = Math.Floor((b - d) / 30.6001m);
            decimal f = Math.Floor(30.6001m * e);

            decimal day = b - d - f; // + (q - z);
            decimal month = e <= 13 ? e - 1 : e - 13;
            decimal year = month <= 2 ? c - 4715m : c - 4716m;

            return new DateTime((int)year, (int)month, (int)day, hour, minute, second, millisecond);
        }

        #endregion

        #region Formatting

        private static string MonthPostfix(int month, string calendarCode)
        {
            return calendarCode?.ToUpper() switch
            {
                Calendars.Gregorian => $"{month}",
                Calendars.Ethiopian => $"Et{month}",
                Calendars.UmAlQura => $"Uq{month}",
                _ => $"{month}",
            };
        }

        private static string MonthFullName(int month, IStringLocalizer localizer, string calendarCode)
        {
            var result = localizer[$"FullMonth{MonthPostfix(month, calendarCode)}"];
            return result.ResourceNotFound ? month.ToString("D4") : result.Value;
        }

        private static string MonthShortName(int month, IStringLocalizer localizer, string calendarCode)
        {
            var result = localizer[$"ShortMonth{MonthPostfix(month, calendarCode)}"];
            return result.ResourceNotFound ? month.ToString("D3") : result.Value;
        }

        public static string FormatDate(DateTime time, IStringLocalizer localizer, string format, string calendarCode)
        {
            format ??= "dd/MM/yyyy";

            var bldr = new StringBuilder(format);
            var calendar = Calendars.GetCalendarFromCode(calendarCode);

            // (1) Year
            if (format.Contains("yyyy"))
            {
                bldr.Replace("yyyy", calendar.GetYear(time).ToString("D4"));
            }
            else if (format.Contains("yyy"))
            {
                bldr.Replace("yyy", calendar.GetYear(time).ToString("D3"));
            }
            else if (format.Contains("yy"))
            {
                bldr.Replace("yy", calendar.GetYear(time).ToString("D2"));
            }

            // (2) Day
            if (format.Contains("dd"))
            {
                bldr.Replace("dd", calendar.GetDayOfMonth(time).ToString("D2"));
            }
            else if (format.Contains("d"))
            {
                bldr.Replace("d", calendar.GetDayOfMonth(time).ToString());
            }

            // (3) Month
            if (format.Contains("MMMM"))
            {
                bldr.Replace("MMMM", MonthFullName(calendar.GetMonth(time), localizer, calendarCode));
            }
            else if (format.Contains("MMM"))
            {
                bldr.Replace("MMM", MonthShortName(calendar.GetMonth(time), localizer, calendarCode));
            }
            else if (format.Contains("MM"))
            {
                bldr.Replace("MM", calendar.GetMonth(time).ToString("D2"));
            }
            else if (format.Contains("M"))
            {
                bldr.Replace("MM", calendar.GetMonth(time).ToString());
            }

            return bldr.ToString();
        }

        #endregion
    }
}
