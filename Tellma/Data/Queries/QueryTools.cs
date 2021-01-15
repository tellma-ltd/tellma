using Tellma.Entities;
using Tellma.Services.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Tellma.Data.Queries
{
    /// <summary>
    /// Contains helper methods that are used in the implementations of <see cref="Query{T}"/> and <see cref="AggregateQuery{T}"/>
    /// </summary>
    internal static class QueryTools
    {
        /// <summary>
        /// Takes a string in the form of "A/B/C" and returns the path ["A", "B"] and the property "C"
        /// trimming all the strings along the way
        /// </summary>
        public static (string[] Path, string Property) ExtractPathAndProperty(string s)
        {
            var steps = s.Split('/').Select(e => e?.Trim());
            string[] path = steps.Take(steps.Count() - 1).ToArray();
            string property = steps.Last();

            return (path, property);
        }

        /// <summary>
        /// Takes a string in the form of "A/B/C|month" and returns the path ["A", "B"], the property "C"
        /// and the modifier "month" (as long as it is one of the modifiers in <see cref="Modifiers"/>
        /// trimming all the strings  along the way
        /// </summary>
        public static (string[] Path, string Property, string Modifier) ExtractPathPropertyAndModifier(string atom)
        {
            var pieces = atom.Split('|');

            // Get the modifier
            string modifier = null;
            if (pieces.Length > 1)
            {
                modifier = string.Join("|", pieces.Skip(1)).Trim();
            }

            // Get the path and property
            string pathAndProp = pieces[0].Trim();
            var (path, property) = ExtractPathAndProperty(pathAndProp);

            return (path, property, modifier);
        }

        /// <summary>
        /// This is alternative for <see cref="Type.GetProperties"/>
        /// that returns base class properties before inherited class properties
        /// Credit: https://bit.ly/2UGAkKj
        /// </summary>
        public static PropertyInfo[] GetPropertiesBaseFirst(this Type type, BindingFlags bindingAttr)
        {
            var orderList = new List<Type>();
            var iteratingType = type;
            do
            {
                orderList.Insert(0, iteratingType);
                iteratingType = iteratingType.BaseType;
            } while (iteratingType != null);

            var props = type.GetProperties(bindingAttr)
                .OrderBy(x => orderList.IndexOf(x.DeclaringType))
                .ToArray();

            return props;
        }

        /// <summary>
        /// Indents all the lines of the string by a specified number of spaces, useful when formatting nested SQL queries
        /// </summary>
        public static string IndentLines(string s, int spaces = 4)
        {
            var lines = s.Split(Environment.NewLine);
            StringBuilder bldr = new StringBuilder();
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                string indentedLine = new string(' ', spaces) + line;
                if (i == lines.Length - 1)
                {
                    bldr.Append(indentedLine);

                }
                else
                {
                    bldr.AppendLine(indentedLine);
                }
            }

            return bldr.ToString();
        }

        /// <summary>
        /// Takes a bunch of clauses and combines them into one nicely formatted SQL query
        /// </summary>
        /// <param name="selectSql">The SELECT clause</param>
        /// <param name="joinSql">The FROM ... JOIN clause </param>
        /// <param name="principalQuerySql">The INNER JOIN clause of the principal query</param>
        /// <param name="whereSql">The WHERE clause</param>
        /// <param name="orderbySql">The ORDER BY clause</param>
        /// <param name="offsetFetchSql">The OFFSET ... FETCH clause</param>
        /// <param name="groupbySql">The GROUP BY clause</param>
        /// <param name="havingSql">The HAVING clause</param>
        public static string CombineSql(string selectSql, string joinSql, string principalQuerySql, string whereSql, string orderbySql, string offsetFetchSql, string groupbySql, string havingSql)
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

            return finalSQL.ToString();
        }

        public static DateTime Today(DateTime? userToday)
        {
            return userToday ?? DateTime.Today;
        }

        public static DateTime StartOfMonth(DateTime? userToday)
        {
            var today = Today(userToday);
            return new DateTime(today.Year, today.Month, 1);
        }

        public static DateTime StartOfQuarter(DateTime? userToday)
        {
            var today = Today(userToday);
            int quarter = (today.Month - 1) / 3 + 1;
            return new DateTime(today.Year, (quarter - 1) * 3 + 1, 1);
        }

        public static DateTime StartOfYear(DateTime? userToday)
        {
            var today = Today(userToday);
            return new DateTime(today.Year, 1, 1);
        }
    }
}
