using System.Globalization;

namespace Tellma.Utilities.Calendars
{
    /// <summary>
    /// String representations of all supported calendars.
    /// </summary>
    public static class Calendars
    {
        /// <summary>
        /// Gregorian Calendar.
        /// </summary>
        public const string Gregorian = "gc";

        /// <summary>
        /// Ethiopian Calendar.
        /// </summary>
        public const string Ethiopian = "et";

        /// <summary>
        /// Umm Al Qura Calendar.
        /// </summary>
        public const string UmAlQura = "uq";

        /// <summary>
        /// All supported calendars.
        /// </summary>
        public static readonly string[] SupportedCalendars = new string[] { Gregorian, Ethiopian, UmAlQura };

        /// <summary>
        /// Retrieves the <see cref="Calendar"/> implementation that corresponds to <paramref name="calendarCode"/>.
        /// </summary>
        /// <param name="calendarCode">The string representation of the <see cref="Calendar"/> implementation.</param>
        public static Calendar GetCalendarFromCode(string calendarCode)
        {
            return calendarCode?.ToLower() switch
            {
                Gregorian => new GregorianCalendar(),
                Ethiopian => new EthiopianCalendar(),
                UmAlQura => new UmAlQuraCalendar(),
                _ => new GregorianCalendar(),
            };
        }
    }
}
