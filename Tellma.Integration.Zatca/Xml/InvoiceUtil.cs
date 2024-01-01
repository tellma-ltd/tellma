namespace Tellma.Integration.Zatca
{
    internal static class InvoiceUtil
    {
        /// <summary>
        /// Returns the current <see cref="DateTime"/> in Saudi Arabia's time zone.
        /// </summary>
        public static DateTime NowInSaudiArabia()
        {
            var saudiTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Arab Standard Time");
            return TimeZoneInfo.ConvertTime(DateTime.Now, saudiTimeZone);
        }
    }
}
