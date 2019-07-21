using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Services.OData
{
    public static class Aggregations
    {
        public static readonly string[] All = { count, dcount, sum, avg, max, min };

        public const string count = nameof(count);
        public const string dcount = nameof(dcount);
        public const string sum = nameof(sum);
        public const string avg = nameof(avg);
        public const string max = nameof(max);
        public const string min = nameof(min);
    }
}
