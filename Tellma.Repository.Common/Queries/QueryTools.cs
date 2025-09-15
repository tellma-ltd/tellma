using System.Text;

namespace Tellma.Repository.Common
{
    /// <summary>
    /// Contains helper methods that are used in the implementations of <see cref="EntityQuery{T}"/> and <see cref="AggregateQuery{T}"/>
    /// </summary>
    internal static class QueryTools
    {
        /// <summary>
        /// Takes a bunch of clauses and combines them into one nicely formatted SQL query
        /// </summary>
        /// <param name="selectSql">The SELECT clause (with an optional "INTO #TempTable" at the end)</param>
        /// <param name="joinSql">The FROM ... JOIN clause </param>
        /// <param name="principalQuerySql">The INNER JOIN clause of the principal query</param>
        /// <param name="whereSql">The WHERE clause</param>
        /// <param name="orderbySql">The ORDER BY clause</param>
        /// <param name="offsetFetchSql">The OFFSET ... FETCH clause</param>
        /// <param name="groupbySql">The GROUP BY clause</param>
        /// <param name="havingSql">The HAVING clause</param>
        /// <param name="selectFromTempSql">The SELECT * FROM #TempTable Clause</param>
        public static string CombineSql(
            string selectSql,
            string joinSql,
            string principalQuerySql,
            string whereSql,
            string orderbySql,
            string offsetFetchSql,
            string groupbySql,
            string havingSql,
            string selectFromTempSql,
            bool optionRecompile)
        {
            var finalSQL = new StringBuilder();

            finalSQL.AppendLine(selectSql);
            finalSQL.Append(joinSql);

            if (!string.IsNullOrWhiteSpace(principalQuerySql))
            {
                finalSQL.AppendLine();
                finalSQL.Append(principalQuerySql);
            }

            if (!string.IsNullOrWhiteSpace(whereSql))
            {
                finalSQL.AppendLine();
                finalSQL.Append(whereSql);
            }

            if (!string.IsNullOrWhiteSpace(groupbySql))
            {
                finalSQL.AppendLine();
                finalSQL.Append(groupbySql);
            }

            if (!string.IsNullOrWhiteSpace(havingSql))
            {
                finalSQL.AppendLine();
                finalSQL.Append(havingSql);
            }

            if (!string.IsNullOrWhiteSpace(orderbySql))
            {
                finalSQL.AppendLine();
                finalSQL.Append(orderbySql);
            }

            if (!string.IsNullOrWhiteSpace(offsetFetchSql))
            {
                finalSQL.AppendLine();
                finalSQL.Append(offsetFetchSql);
            }

            if (!string.IsNullOrWhiteSpace(selectFromTempSql))
            {
                finalSQL.AppendLine();
                finalSQL.AppendLine();
                finalSQL.Append(selectFromTempSql);
                if (!string.IsNullOrWhiteSpace(orderbySql))
                {
                    finalSQL.AppendLine();
                    finalSQL.Append(orderbySql);
                }
            }

            if (optionRecompile)
            {
                finalSQL.AppendLine();
                finalSQL.Append("OPTION(RECOMPILE) ");
            }

            return finalSQL.ToString();
        }
    }
}
