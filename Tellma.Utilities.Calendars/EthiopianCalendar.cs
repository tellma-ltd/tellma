using System;
using System.Globalization;

namespace Tellma.Utilities.Calendars
{
    /// <summary>
    /// Represents the Ethiopian Calendar.
    /// <para/>
    /// https://en.wikipedia.org/wiki/Ethiopian_calendar.
    /// </summary>
    public class EthiopianCalendar : Calendar
    {
        public const int EthiopianEra = 1;

        internal static DateTime minDate = new(1752, 9, 14);
        internal static DateTime maxDate = new(2500, 1, 1);

        static internal void CheckTicksRange(long ticks)
        {
            if (ticks < minDate.Ticks || ticks > maxDate.Ticks)
            {
                throw new ArgumentOutOfRangeException(nameof(ticks), $"The Ethiopian Calendar supports dates in the range {minDate} to {maxDate}.");
            }
        }

        public override int[] Eras => new int[] { EthiopianEra };

        public override DateTime AddMonths(DateTime time, int months)
        {
            var (year, month, day, hour, minute, second, millisecond) = DateTimeParts(time);

            int yMonths = 13;
            decimal dMonths = months;
            year += (int)Math.Floor((dMonths - 1) / yMonths);
            month += (((months - 1) % yMonths + yMonths) % yMonths) + 1;
            day = Math.Min(day, GetDaysInMonth(year, month));

            return ToDateTime(year, month, day, hour, minute, second, millisecond);
        }

        public override DateTime AddYears(DateTime time, int years)
        {
            var (year, month, day, hour, minute, second, millisecond) = DateTimeParts(time);

            year += years;
            day = Math.Min(day, GetDaysInMonth(year, month));

            return ToDateTime(year, month, day, hour, minute, second, millisecond);
        }

        public override int GetDayOfMonth(DateTime time)
        {
            var (_, _, day) = DateParts(time);
            return day;
        }

        public override DayOfWeek GetDayOfWeek(DateTime time)
        {
            return time.DayOfWeek;
        }

        public override int GetDayOfYear(DateTime time)
        {
            var (_, month, day) = DateParts(time);
            return ((month - 1) * 30) + day;
        }

        public override int GetDaysInMonth(int year, int month, int era)
        {
            if (month == 13)
            {
                return IsLeapMonth(year, month) ? 6 : 5;
            }
            else
            {
                return 30;
            }
        }

        public override int GetDaysInYear(int year, int era)
        {
            return IsLeapYear(year) ? 366 : 365;
        }

        public override int GetEra(DateTime time)
        {
            CheckTicksRange(time.Ticks);
            return EthiopianEra;
        }

        public override int GetMonth(DateTime time)
        {
            var (_, month, _) = DateParts(time);
            return month;
        }

        public override int GetMonthsInYear(int year, int era)
        {
            return 13;
        }

        public override int GetYear(DateTime time)
        {
            var (year, _, _) = DateParts(time);
            return year;
        }

        public override bool IsLeapDay(int year, int month, int day, int era)
        {
            return IsLeapMonth(year, month) && day == 6;
        }

        public override bool IsLeapMonth(int year, int month, int era)
        {
            return IsLeapYear(year) && month == 13;
        }

        public override bool IsLeapYear(int year, int era)
        {
            return (year % 4) == 3;
        }

        public override DateTime ToDateTime(int year, int month, int day, int hour, int minute, int second, int millisecond, int era)
        {
            int jdn = EthiopianToJdn(year, month, day);
            return CalendarUtilities.JdnToDateTime(jdn, hour, minute, second, millisecond);
        }

        private static (int year, int month, int day) DateParts(DateTime time)
        {
            int jdn = CalendarUtilities.DateTimeToJdn(time);
            return JdnToEthiopian(jdn);
        }

        private static (int year, int month, int day, int hour, int minute, int second, int millisecond) DateTimeParts(DateTime time)
        {
            var (year, month, day) = DateParts(time);
            return (year, month, day, time.Hour, time.Minute, time.Second, time.Millisecond);
        }

        public override DateTime MinSupportedDateTime => minDate;

        public override DateTime MaxSupportedDateTime => maxDate;

        public override CalendarAlgorithmType AlgorithmType => CalendarAlgorithmType.SolarCalendar;

        #region Calendar Calculations

        /// <summary>
        /// Converts from Ethiopian Calendar to Julian Day Number (JDN).
        /// http://www.geez.org/Calendars/
        /// </summary>
        private static int EthiopianToJdn(int year, int month, int day)
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
        private static (int year, int month, int day) JdnToEthiopian(int jdn)
        {
            int jdOffset = 1723856;
            int r = (jdn - jdOffset) % 1461;
            int n = (r % 365) + 365 * (r / 1460);

            int year = 4 * ((jdn - jdOffset) / 1461)
                + (r / 365)
                - (r / 1460);

            int month = (n / 30) + 1;
            int day = (n % 30) + 1;

            return (year, month, day);
        }

        #endregion
    }
}