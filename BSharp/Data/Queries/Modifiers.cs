using System.Collections.Generic;

namespace BSharp.Data.Queries
{
    public static class Modifiers
    {
        /// <summary>
        /// All supported path modifiers
        /// </summary>
        public static readonly string[] All = { year, quarter, month, dayofyear, day, week, weekday };
        // IMPORTANT: This is also in the files below, please keep in sync.
        // ChoiceAttribute of ReportDimensionDefinition
        // filter-expression.ts
        // report-results.ts
        // report-definition.ts

        /// <summary>
        /// All supported path modifiers in a hash table
        /// </summary>
        public static readonly HashSet<string> AllHash = new HashSet<string>(All);

        /// <summary>
        /// Year
        /// </summary>
        public const string year = nameof(year);

        /// <summary>
        /// Quarter
        /// </summary>
        public const string quarter = nameof(quarter);

        /// <summary>
        /// Month
        /// </summary>
        public const string month = nameof(month);

        /// <summary>
        /// Day of Year
        /// </summary>
        public const string dayofyear = nameof(dayofyear);

        /// <summary>
        /// Day
        /// </summary>
        public const string day = nameof(day);

        /// <summary>
        /// Week
        /// </summary>
        public const string week = nameof(week);

        /// <summary>
        /// Week Day
        /// </summary>
        public const string weekday = nameof(weekday);
    }
}
