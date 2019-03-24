using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace BSharp.Services.FilterParser
{
    public interface IFilterParser
    {
        /// <summary>
        /// Parses the OData like filter expression into a linq lambda expression
        /// </summary>
        Expression ParseFilterExpression<T>(string filter, ParameterExpression eParam, int? currentUserId = null, TimeZoneInfo currentUserTimeZone = null);

        /// <summary>
        /// Extracts the list of paths from the OData like filter expression
        /// for example given the filter expression: "A/B eq v and C eq 3"
        /// the result would be ["A/B", "C"]
        /// </summary>
        List<string> ExtractPaths(string filter);
    }
}
