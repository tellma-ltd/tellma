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
        public static string CombineSql(string selectSql, string joinSql, string principalQuerySql, string whereSql, string orderbySql, string offsetFetchSql, string groupbySql)
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

        /// <summary>
        /// Turns a filter expression into an SQL WHERE clause (without the WHERE keyword), adds all required parameters into the <see cref="SqlStatementParameters"/>
        /// </summary>
        public static string FilterToSql(FilterExpression e, Func<Type, string> sources, SqlStatementParameters ps, JoinTrie joinTree, int userId, DateTime? userToday)
        {
            if (e == null)
            {
                return null;
            }

            // This inner function just relieves us of having to pass all the above parameters each time, they just become part of its closure
            string FilterToSqlInner(FilterExpression exp)
            {
                if (exp is FilterConjunction conExp)
                {
                    return $"({FilterToSqlInner(conExp.Left)}) AND ({FilterToSqlInner(conExp.Right)})";
                }

                if (exp is FilterDisjunction disExp)
                {
                    return $"({FilterToSqlInner(disExp.Left)}) OR ({FilterToSqlInner(disExp.Right)})";
                }

                if (exp is FilterNegation notExp)
                {
                    return $"NOT ({FilterToSqlInner(notExp.Inner)})";
                }

                if (exp is FilterAtom atom)
                {
                    // (A) Prepare the symbol corresponding to the path, e.g. P1
                    var join = joinTree[atom.Path];
                    if (join == null)
                    {
                        // Developer mistake
                        throw new InvalidOperationException($"The path '{string.Join('/', atom.Path)}' was not found in the joinTree");
                    }
                    var symbol = join.Symbol;

                    // (B) Determine the type of the property and its value
                    var propName = atom.Property;
                    var prop = join.EntityDescriptor.Property(propName);
                    if (prop == null)
                    {
                        // Developer mistake
                        throw new InvalidOperationException($"Could not find property {propName} on type {join.EntityDescriptor}");
                    }

                    // The type of the first operand
                    var propType = Nullable.GetUnderlyingType(prop.Type) ?? prop.Type;
                    if (!string.IsNullOrWhiteSpace(atom.Modifier))
                    {
                        // So far all modifiers are only applicable for date properties
                        if (propType != typeof(DateTime) && propType != typeof(DateTimeOffset))
                        {
                            // Developer mistake
                            throw new InvalidOperationException($"The modifier {atom.Modifier} is not valid for property {propName} since it is not of type DateTime or DateTimeOffset");
                        }

                        // So far all modifiers are date modifiers that return INT
                        propType = typeof(int);
                    }

                    // The expected type of the second operand (different in the case of hierarchyId)
                    var expectedValueType = propType;
                    if (expectedValueType == typeof(HierarchyId))
                    {
                        var idType = join.EntityDescriptor.Property("Id")?.Type;
                        if (idType == null)
                        {
                            // Programmer mistake
                            throw new InvalidOperationException($"Type {join.EntityDescriptor} is a tree structure but has no Id property");
                        }

                        expectedValueType = Nullable.GetUnderlyingType(idType) ?? idType;
                    }

                    // (C) Prepare the value (e.g. "'Huntington Rd.'")
                    var valueString = atom.Value;
                    object value;
                    bool isNull = false;
                    switch (valueString?.ToLower())
                    {
                        // This checks all built-in values
                        case "null":
                            value = null;
                            isNull = true;
                            break;

                        case "me":
                            value = userId;
                            break;

                        // Relative DateTime values
                        case "startofyear":
                            EnsureNullModifier(atom);
                            EnsureTypeDateTime(atom, propName, propType);

                            value = StartOfYear(userToday);
                            break;

                        case "endofyear":
                            EnsureNullModifier(atom);
                            EnsureTypeDateTime(atom, propName, propType);

                            value = StartOfYear(userToday).AddYears(1);
                            break;

                        case "startofquarter":
                            EnsureNullModifier(atom);
                            EnsureTypeDateTime(atom, propName, propType);

                            value = StartOfQuarter(userToday);
                            break;

                        case "endofquarter":
                            EnsureNullModifier(atom);
                            EnsureTypeDateTime(atom, propName, propType);

                            value = StartOfQuarter(userToday).AddMonths(3);
                            break;

                        case "startofmonth":
                            EnsureNullModifier(atom);
                            EnsureTypeDateTime(atom, propName, propType);

                            value = StartOfMonth(userToday);
                            break;

                        case "endofmonth":
                            EnsureNullModifier(atom);
                            EnsureTypeDateTime(atom, propName, propType);

                            value = StartOfMonth(userToday).AddMonths(1);
                            break;

                        case "today":
                            EnsureNullModifier(atom);
                            EnsureTypeDateTime(atom, propName, propType);

                            value = Today(userToday);
                            break;

                        case "endofday":
                            EnsureNullModifier(atom);
                            EnsureTypeDateTime(atom, propName, propType);

                            value = Today(userToday).AddDays(1);
                            break;

                        case "now":
                            EnsureNullModifier(atom);
                            EnsureTypeDateTimeOffset(atom, propName, propType);

                            var now = DateTimeOffset.Now;
                            value = now;
                            break;

                        default:
                            if (expectedValueType == typeof(string) || expectedValueType == typeof(char))
                            {
                                if (!valueString.StartsWith("'") || !valueString.EndsWith("'"))
                                {
                                    // Developer mistake
                                    throw new InvalidOperationException($"Property {propName} is of type String, therefore the value it is compared to must be enclosed in single quotation marks");
                                }

                                valueString = valueString[1..^1];
                            }

                            try
                            {
                                value = ParseFilterValue(valueString, expectedValueType);
                            }
                            catch (ArgumentException)
                            {
                                // Developer mistake
                                throw new InvalidOperationException($"The filter value '{valueString}' could not be parsed into a valid {propType}");
                            }

                            break;
                    }

                    var paramSymbol = isNull ? "NULL" : "@" + ps.AddParameter(value);

                    // (D) Prepare the SQL property
                    string propSQL = AtomSql(symbol, atom.Property, null, atom.Modifier);

                    // (E) parse the operator (e.g. "eq")
                    switch (atom.Op?.ToLower() ?? "")
                    {
                        case Ops.gt:
                        case Ops.gtSign:
                            return $"{propSQL} > {paramSymbol}";

                        case Ops.ge:
                        case Ops.geSign:
                            return $"{propSQL} >= {paramSymbol}";

                        case Ops.lt:
                        case Ops.ltSign:
                            return $"{propSQL} < {paramSymbol}";

                        case Ops.le:
                        case Ops.leSign:
                            return $"{propSQL} <= {paramSymbol}";

                        case Ops.eq:
                        case Ops.eqSign:
                            string eqSql = isNull ? "IS" : "=";
                            return $"{propSQL} {eqSql} {paramSymbol}";

                        case Ops.ne:
                        case Ops.neSign:
                        case Ops.neSign2:
                            string neSql = isNull ? "IS NOT" : "<>";
                            return $"{propSQL} {neSql} {paramSymbol}";

                        case Ops.contains: // Must be text
                            EnsureTypeString(atom, propName, propType);
                            paramSymbol = "@" + ps.AddParameter(value.ToString().Replace("%", "[%]"));
                            return $"{propSQL} LIKE N'%' + {paramSymbol} + N'%'";

                        case Ops.ncontains: // Must be text
                            EnsureTypeString(atom, propName, propType);
                            paramSymbol = "@" + ps.AddParameter(value.ToString().Replace("%", "[%]"));
                            return $"{propSQL} NOT LIKE N'%' + {paramSymbol} + N'%'";

                        case Ops.startsw: // Must be text
                            EnsureTypeString(atom, propName, propType);
                            paramSymbol = "@" + ps.AddParameter(value.ToString().Replace("%", "[%]"));
                            return $"{propSQL} LIKE {paramSymbol} + N'%'";

                        case Ops.nstartsw: // Must be text
                            EnsureTypeString(atom, propName, propType);
                            paramSymbol = "@" + ps.AddParameter(value.ToString().Replace("%", "[%]"));
                            return $"{propSQL} NOT LIKE {paramSymbol} + N'%'";

                        case Ops.endsw: // Must be text
                            EnsureTypeString(atom, propName, propType);
                            paramSymbol = "@" + ps.AddParameter(value.ToString().Replace("%", "[%]"));
                            return $"{propSQL} LIKE N'%' + {paramSymbol}";

                        case Ops.nendsw: // Must be text
                            EnsureTypeString(atom, propName, propType);
                            paramSymbol = "@" + ps.AddParameter(value.ToString().Replace("%", "[%]"));
                            return $"{propSQL} NOT LIKE N'%' + {paramSymbol}";

                        case Ops.childof: // Must be hierarchy Id
                            {
                                EnsureTypeHierarchyId(atom, propName, propType);
                                string treeSource = sources(join.EntityDescriptor.Type);
                                string parentNode = isNull ? "HierarchyId::GetRoot()" :
                                    $"(SELECT [Node] FROM {treeSource} As [T] WHERE [T].[Id] = {paramSymbol})";

                                return $"{propSQL}.GetAncestor(1) = {parentNode}";
                            }

                        case Ops.descof: // Must be hierarchy Id
                            {
                                EnsureTypeHierarchyId(atom, propName, propType);
                                string treeSource = sources(join.EntityDescriptor.Type);
                                string parentNode = isNull ? "HierarchyId::GetRoot()" :
                                    $"(SELECT [Node] FROM {treeSource} As [T] WHERE [T].[Id] = {paramSymbol})";

                                return $"{propSQL}.IsDescendantOf({parentNode}) = 1";
                            }

                        default:
                            // Developer mistake
                            throw new InvalidOperationException($"The filter operator '{atom.Op}' is not recognized");
                    }
                }

                // Programmer mistake
                throw new InvalidOperationException("Unknown AST type");

            }

            return FilterToSqlInner(e);
        }

        /// <summary>
        /// The default Convert.ChangeType cannot handle converting types to
        /// nullable types also it cannot handle DateTimeOffset
        /// this method overcomes these limitations, credit: https://bit.ly/2DgqJmL
        /// </summary>
        public static object ParseFilterValue(this string stringValue, Type targetType)
        {
            if (stringValue is null)
            {
                return null;
            }

            if (targetType is null)
            {
                throw new ArgumentNullException(nameof(targetType));
            }

            targetType = Nullable.GetUnderlyingType(targetType) ?? targetType;
            try
            {
                if (targetType == typeof(DateTimeOffset))
                {
                    // Convert.ChangeType throws an error for DateTimeOffset
                    // So must take care of this manually
                    return DateTimeOffset.Parse(stringValue);
                }

                // Everything else can be handled by Convert.ChangeType
                return Convert.ChangeType(stringValue, targetType);
            }
            catch
            {
                throw new InvalidOperationException($"Failed to convert '{stringValue}' to type: {targetType.Name}");
            }
        }

        private static DateTime Today(DateTime? userToday)
        {
            return userToday ?? DateTime.Today;
        }

        private static DateTime StartOfMonth(DateTime? userToday)
        {
            var today = Today(userToday);
            return new DateTime(today.Year, today.Month, 1);
        }

        private static DateTime StartOfQuarter(DateTime? userToday)
        {
            var today = Today(userToday);
            int quarter = (today.Month - 1) / 3 + 1;
            return new DateTime(today.Year, (quarter - 1) * 3 + 1, 1);
        }

        private static DateTime StartOfYear(DateTime? userToday)
        {
            var today = Today(userToday);
            return new DateTime(today.Year, 1, 1);
        }

        /// <summary>
        /// Creates a result column using a symbol, a property name and an aggregation like this Aggregation([Symbol].[PropertyName])
        /// </summary>
        public static string AtomSql(string symbol, string propName, string aggregation, string modifier)
        {
            var result = $"[{symbol}].[{propName}]";

            if (!string.IsNullOrWhiteSpace(modifier))
            {
                // So far all modifiers are date parts and have no parameters, in the future this may change
                var lowerCaseFunction = modifier.ToLower();
                var sqlFunction = Modifiers.All.FirstOrDefault(fn => lowerCaseFunction.Equals(fn)) ??
                    throw new InvalidOperationException($"Unrecognized modifier '{modifier}'");

                result = $"DATEPART({sqlFunction}, {result})";
            }

            // Apply the aggregation if any
            if (!string.IsNullOrWhiteSpace(aggregation))
            {
                var sqlAggregation = aggregation switch
                {
                    Aggregations.count => "COUNT({0})",
                    Aggregations.sum => "SUM({0})",
                    Aggregations.avg => "AVG(CAST({0} AS DECIMAL(18,8)))",
                    Aggregations.min => "MIN({0})",
                    Aggregations.max => "MAX({0})",
                    _ => throw new InvalidOperationException($"Unrcognized aggregation '{aggregation}'"),
                };
                result = string.Format(sqlAggregation, result);
            }

            return result;
        }

        private static void EnsureNullModifier(FilterAtom atom)
        {
            if (!string.IsNullOrWhiteSpace(atom.Modifier))
            {
                throw new InvalidOperationException($"Filter keyword '{atom.Value}' cannot be used with a date modifier such as '{atom.Modifier}'");
            }
        }

        private static void EnsureTypeHierarchyId(FilterAtom atom, string propName, Type propType)
        {
            if (propType != typeof(HierarchyId))
            {
                // Developer mistake
                throw new InvalidOperationException($"Filter operator '{atom.Op}' cannot be used with Property {propName} because it is not of type HierarchyId");
            }
        }

        private static void EnsureTypeString(FilterAtom atom, string propName, Type propType)
        {
            if (propType != typeof(string))
            {
                // Developer mistake
                throw new InvalidOperationException($"Filter operator '{atom.Op}' cannot be used with Property {propName} because it is not of type String");
            }
        }

        private static void EnsureTypeDateTime(FilterAtom atom, string propName, Type propType)
        {
            if (propType != typeof(DateTime))
            {
                // Developer mistake
                throw new InvalidOperationException($"Filter keyword '{atom.Value}' cannot be used with property {propName} because it is not of type DateTime");
            }
        }

        private static void EnsureTypeDateTimeOffset(FilterAtom atom, string propName, Type propType)
        {
            if (propType != typeof(DateTimeOffset))
            {
                // Developer mistake
                throw new InvalidOperationException($"Filter keyword '{atom.Value}' cannot be used with property {propName} because it is not of type DateTimeOffset");
            }
        }
    }
}
