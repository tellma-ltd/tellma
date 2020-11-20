using System;

namespace Tellma.Services.Utilities
{
    /// <summary>
    /// Contains convenience functions for calendar conversion and manipulation
    /// </summary>
    public static class CalendarUtilities
    {
        public const string EthiopianCode = "ET";
        public const string GregorianCode = "GR";

        public static readonly string[] AllCalendarCodes = { EthiopianCode, GregorianCode };


        /// <summary>
        /// Converts from Gregorian Calendar to Julian Day Number.
        /// https://quasar.as.utexas.edu/BillInfo/JulianDatesG.html
        /// </summary>
        private static int GregorianToJdn(int day, int month, int year)
        {
            decimal d = day;
            decimal m = month;
            decimal y = year;

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
        private static (int day, int month, int year) JdnToGregorian(int jdn)
        {
            decimal q = jdn;
            decimal z = jdn;
            decimal w = Math.Floor((z - 1867216.25m) / 36524.25m);
            decimal x = Math.Floor(w / 4m);
            decimal a = z + 1 + w - x;
            decimal b = a + 1524m;
            decimal c = Math.Floor((b - 122.1m) / 365.25m);
            decimal d = Math.Floor(365.25m * c);
            decimal e = Math.Floor((b - d) / 30.6001m);
            decimal f = Math.Floor(30.6001m * e);

            decimal day = b - d - f + (q - z);
            decimal month = e <= 13 ? e - 1 : e - 13;

            decimal year;
            if (month <= 2)
            {
                // If Jan or Feb
                year = c - 4715m;
            }
            else
            {
                year = c - 4716m;
            }

            return ((int)day, (int)month, (int)year);
        }

        /// <summary>
        /// Converts from Ethiopian Calendar to Julian Day Number (JDN).
        /// http://www.geez.org/Calendars/
        /// </summary>
        private static int EthiopianToJdn(int day, int month, int year)
        {
            int jdOffset = 1723856;
            int jdn = (jdOffset + 365)
               + 365 * (year - 1)
               + (year / 4)
               + 30 * month
               + day - 31;

            return jdn;
        }

        /// <summary>
        /// Converts from Julian Day Number (JDN) to Ethiopian Calendar.
        /// http://www.geez.org/Calendars/
        /// </summary>
        private static (int day, int month, int year) JdnToEthiopian(int jdn)
        {
            int jdOffset = 1723856;
            int r = (jdn - jdOffset) % 1461;
            int n = (r % 365) + 365 * (r / 1460);

            int year = 4 * ((jdn - jdOffset) / 1461)
                + (r / 365)
                - (r / 1460);

            int month = (n / 30) + 1;
            int day = (n % 30) + 1;

            return (day, month, year);
        }

        /// <summary>
        /// Converts from Gregorian Calendar to Ethiopian Calendar.
        /// </summary>
        public static (int day, int month, int year) GregorianToEthiopian(int day, int month, int year)
        {
            int jdn = GregorianToJdn(day, month, year);
            return JdnToEthiopian(jdn);
        }

        /// <summary>
        /// Converts from Ethiopian Calendar to Gregorian Calendar.
        /// </summary>
        public static (int day, int month, int year) EthiopianToGregorian(int day, int month, int year)
        {
            int jdn = EthiopianToJdn(day, month, year);
            return JdnToGregorian(jdn);
        }
    }
}
