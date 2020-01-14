using BSharp.Entities;
using BSharp.Services.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Text;

namespace BSharp.Data.Queries
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
        /// Takes a string in the form of "month(A/B/C)" and returns the path ["A", "B"], the property "C"
        /// and the function "month" (as long as it is one of the functions in <see cref="Functions"/>
        /// trimming all the strings  along the way
        /// </summary>
        /// <param name="atom"></param>
        /// <returns></returns>
        public static (string Function, string[] Path, string Property) ExtractFunctionPathAndProperty(string atom)
        {
            atom = atom.Trim();

            // Extract the function (day, month etc...) if any
            string[] functionKeywords = Functions.All;
            string function = functionKeywords.FirstOrDefault(fn =>
                atom.ToLower().StartsWith(fn) &&
                atom.Substring(fn.Length).Trim().StartsWith("("));
            if (function != null)
            {
                atom = atom.Substring(function.Length);
                atom = atom.TrimStart();
                atom = atom.Substring(1); // to remove the bracket

                if (atom.EndsWith(")"))
                {
                    atom = atom.Remove(atom.Length - 1).Trim();
                }
            }

            var (path, property) = ExtractPathAndProperty(atom);

            return (function, path, property);
        }

        private static readonly ConcurrentDictionary<Type, IEnumerable<PropertyInfo>> _cacheGetMappedProperties = new ConcurrentDictionary<Type, IEnumerable<PropertyInfo>>();

        /// <summary>
        /// Retrieves all properties that are not navigation, and that do not have the <see cref="NotMappedAttribute"/>
        /// adorning them, the implementation uses a memory cache internally for maximum performance
        /// </summary>
        public static IEnumerable<PropertyInfo> GetMappedProperties(this Type type)
        {
            return _cacheGetMappedProperties.GetOrAdd(type, (t) =>
            {
                return t.GetPropertiesBaseFirst(BindingFlags.Public | BindingFlags.Instance)
                .Where(e => e.GetCustomAttribute<NotMappedAttribute>() == null && !e.PropertyType.IsList() && !e.PropertyType.IsEntity());
            });
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
        public static string FilterToSql(FilterExpression e, Func<Type, string> sources, SqlStatementParameters ps, JoinTree joinTree, int currentUserId, TimeZoneInfo currentUserTimeZone)
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
                    var prop = join.Type.GetProperty(propName);
                    if (prop == null)
                    {
                        // Developer mistake
                        throw new InvalidOperationException($"Could not find property {propName} on type {join.Type}");
                    }

                    // The type of the first operand
                    var propType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                    if (!string.IsNullOrWhiteSpace(atom.Function))
                    {
                        // So far all functions are only applicable for date properties
                        if (propType != typeof(DateTime) && propType != typeof(DateTimeOffset))
                        {
                            // Developer mistake
                            throw new InvalidOperationException($"The function {atom.Function} is not valid for property {propName} since it is not of type DateTime or DateTimeOffset");
                        }

                        // So far all functions are date functions that return INT
                        propType = typeof(int);
                    }

                    // The expected type of the second operand (different in the case of hierarchyId)
                    var expectedValueType = propType;
                    if (expectedValueType == typeof(HierarchyId))
                    {
                        var idType = join.Type.GetProperty("Id")?.PropertyType;
                        if (idType == null)
                        {
                            // Programmer mistake
                            throw new InvalidOperationException($"Type {join.Type} is a tree structure but has no Id property");
                        }

                        expectedValueType = Nullable.GetUnderlyingType(idType) ?? idType;
                    }

                    // (C) Prepare the value (e.g. "'Huntington Rd.'")
                    var valueString = atom.Value;
                    object value;
                    bool isNull = false;
                    switch(valueString?.ToLower())
                    {
                        // This checks all built-in values
                        case "null":
                            value = null;
                            isNull = true;
                            break;

                        case "me":
                            value = currentUserId;
                            break;

                        case "today":
                            var today = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, currentUserTimeZone).Date;
                            value = today;
                            break;

                        case "now":
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
                                value = valueString.ChangeType(expectedValueType, currentUserTimeZone);
                            }
                            catch (ArgumentException)
                            {
                                // Developer mistake
                                throw new InvalidOperationException($"The filter value '{valueString}' could not be parsed into a valid {propType}");
                            }
                            break;
                    }

                    string paramSymbol = isNull ? "NULL" : "@" + ps.AddParameter(value);

                    // (D) Prepare the SQL property
                    string propSQL = AtomSql(symbol, atom.Property, null, atom.Function);

                    // (E) parse the operator (e.g. "eq")
                    switch (atom.Op?.ToLower() ?? "")
                    {
                        case Ops.gt:
                            return $"{propSQL} > {paramSymbol}";

                        case Ops.ge:
                            return $"{propSQL} >= {paramSymbol}";

                        case Ops.lt:
                            return $"{propSQL} < {paramSymbol}";

                        case Ops.le:
                            return $"{propSQL} <= {paramSymbol}";

                        case Ops.eq:
                            string eqSql = isNull ? "IS" : "=";
                            return $"{propSQL} {eqSql} {paramSymbol}";

                        case Ops.ne:
                            string neSql = isNull ? "IS NOT" : "<>";
                            return $"{propSQL} {neSql} {paramSymbol}";

                        case Ops.contains: // Must be text
                            EnsureTypeString(atom, propName, propType);
                            return $"{propSQL} LIKE N'%' + {paramSymbol} + N'%'";

                        case Ops.ncontains: // Must be text
                            EnsureTypeString(atom, propName, propType);
                            return $"{propSQL} NOT LIKE N'%' + {paramSymbol} + N'%'";

                        case Ops.startsw: // Must be text
                            EnsureTypeString(atom, propName, propType);
                            return $"{propSQL} LIKE {paramSymbol} + N'%'";

                        case Ops.nstartsw: // Must be text
                            EnsureTypeString(atom, propName, propType);
                            return $"{propSQL} NOT LIKE {paramSymbol} + N'%'";

                        case Ops.endsw: // Must be text
                            EnsureTypeString(atom, propName, propType);
                            return $"{propSQL} LIKE N'%' + {paramSymbol}";

                        case Ops.nendsw: // Must be text
                            EnsureTypeString(atom, propName, propType);
                            return $"{propSQL} NOT LIKE N'%' + {paramSymbol}";

                        case Ops.childof: // Must be hierarchy Id
                            {
                                EnsureTypeHierarchyId(atom, propName, propType);
                                string treeSource = sources(join.Type);
                                string parentNode = isNull ? "HierarchyId::GetRoot()" :
                                    $"(SELECT [Node] FROM {treeSource} As [T] WHERE [T].[Id] = {paramSymbol})";

                                return $"{propSQL}.GetAncestor(1) = {parentNode}";
                            }

                        case Ops.descof: // Must be hierarchy Id
                            {
                                EnsureTypeHierarchyId(atom, propName, propType);
                                string treeSource = sources(join.Type);
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
        /// Creates a result column using a symbol, a property name and an aggregation like this Aggregation([Symbol].[PropertyName])
        /// </summary>
        public static string AtomSql(string symbol, string propName, string aggregation, string function)
        {
            var result = $"[{symbol}].[{propName}]";

            if (!string.IsNullOrWhiteSpace(function))
            {
                // So far all functions are date parts, in the future more could be added
                result = $"DATEPART({function}, {result})";
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
        
        private static void EnsureTypeHierarchyId(FilterAtom atom, string propName, Type propType)
        {
            if (propType != typeof(HierarchyId))
            {
                // Developer mistake
                throw new InvalidOperationException($"Property {propName} is not of type hierarchyid, therefore cannot use the operator '{atom.Op}'");
            }
        }

        private static void EnsureTypeString(FilterAtom atom, string propName, Type propType)
        {
            if (propType != typeof(string))
            {
                // Developer mistake
                throw new InvalidOperationException($"Property {propName} is not of type String, therefore cannot use the operator '{atom.Op}'");
            }
        }

        /// <summary>
        /// Changes a function mapping a <see cref="Type"/> to <see cref="SqlSource"/> into a function mapping a <see cref="Type"/> to raw SQL strings.
        /// The <see cref="SqlSource.Parameters"/> are automatically added to the <see cref="SqlStatementParameters"/> inside the function whenever a 
        /// source is requested
        /// </summary>
        public static Func<Type, string> RawSources(Func<Type, SqlSource> sources, SqlStatementParameters ps)
        {
            // This hashset ensures that parameters to request a certain type are only added once
            var aleadyRequested = new HashSet<Type>();
            return (t) =>
            {
                var source = sources(t);
                if (aleadyRequested.Add(t) && source.Parameters != null)
                {
                    foreach (var p in source.Parameters)
                    {
                        ps.AddParameter(p);
                    }
                }

                // Return the raw SQL script
                return source.SQL;
            };
        }
    }
}
