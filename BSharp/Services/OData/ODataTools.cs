using BSharp.Services.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BSharp.Services.OData
{
    public static class ODataTools
    {
        public static string PathString(string[] path, string property = null)
        {
            string result = string.Join(' ', path);
            if (!string.IsNullOrWhiteSpace(property))
            {
                result = $"{result}/{property}";
            }

            return result;
        }

        public static (string[] Path, string Property) ExtractPathAndProperty(string item)
        {
            var steps = item.Split('/').Select(e => e?.Trim());
            string[] path = steps.Take(steps.Count() - 1).ToArray();
            string property = steps.Last();

            return (path, property);
        }

        private static ConcurrentDictionary<Type, IEnumerable<PropertyInfo>> _cacheGetMappedProperties = new ConcurrentDictionary<Type, IEnumerable<PropertyInfo>>();
        public static IEnumerable<PropertyInfo> GetMappedProperties(this Type type)
        {
            if(!_cacheGetMappedProperties.ContainsKey(type))
            {
                // All public properties on the DTO excluding navigation properties and properties adorned with NotMapped
                _cacheGetMappedProperties[type] = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(e => e.GetCustomAttribute<NotMappedAttribute>() == null && !e.PropertyType.IsList() && e.PropertyType.GetProperty("Id") == null);
            }

            return _cacheGetMappedProperties[type];
        }

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
        /// Turns a filter expression into an SQL WHERE clause (without the WHERE keyword), adds all required parameters into the SqlStatementParameters
        /// </summary>
        public static string FilterToSql(FilterExpression e, Func<Type, string> sources, SqlStatementParameters ps, JoinTree joinTree, int currentUserId, TimeZoneInfo currentUserTimeZone)
        {
            if(e == null)
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

                    // (B) Determine the type of the property
                    var propName = atom.Property;
                    var prop = join.Type.GetProperty(propName);
                    if (prop == null)
                    {
                        // Developer mistake
                        throw new InvalidOperationException($"Could not find property {propName} on type {join.Type}");
                    }

                    var propType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                    bool isHierarchyId = propType == typeof(HierarchyId);
                    if (isHierarchyId)
                    {
                        var idType = join.Type.GetProperty("Id")?.PropertyType;
                        if(idType == null)
                        {
                            // Programmer mistake
                            throw new InvalidOperationException($"Type {join.Type} is a tree structure but has no Id property");
                        }

                        propType = Nullable.GetUnderlyingType(idType) ?? idType;
                    }

                    // (C) Prepare the value (e.g. "'Huntington Rd.'")
                    var valueString = atom.Value;
                    object value;
                    bool isNull = false;
                    if (valueString == "null")
                    {
                        value = null;
                        isNull = true;
                    }
                    else if (valueString?.ToLower() == "me")
                    {
                        value = currentUserId;
                    }
                    else
                    {
                        if (propType == typeof(string) || propType == typeof(char))
                        {
                            if (!valueString.StartsWith("'") || !valueString.EndsWith("'"))
                            {
                                // Developer mistake
                                throw new InvalidOperationException($"Property {propName} is of type String, therefore the value it is compared to must be enclosed in single quotation marks");
                            }

                            valueString = valueString.Substring(1, valueString.Length - 2);
                        }

                        try
                        {
                            value = valueString.ChangeType(prop.PropertyType, currentUserTimeZone);
                        }
                        catch (ArgumentException)
                        {
                            // Developer mistake
                            throw new InvalidOperationException($"The filter value '{valueString}' could not be parsed into a valid {propType}");
                        }
                    }

                    string paramSymbol = isNull ? "NULL" : "@" + ps.AddParameter(value);

                    // (D) parse the operator (e.g. "eq")
                    string propSQL = $"[{symbol}].[{atom.Property}]";
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
                            if (propType != typeof(string) || isHierarchyId)
                            {
                                // Developer mistake
                                throw new InvalidOperationException($"Property {propName} is not of type String, therefore cannot use the operator '{atom.Op}'");
                            }

                            return $"{propSQL} LIKE N'%' + {paramSymbol} + N'%'";

                        case Ops.ncontains: // Must be text
                            if (propType != typeof(string) || isHierarchyId)
                            {
                                // Developer mistake
                                throw new InvalidOperationException($"Property {propName} is not of type String, therefore cannot use the operator '{atom.Op}'");
                            }

                            return $"{propSQL} NOT LIKE N'%' + {paramSymbol} + N'%'";

                        case Ops.childof: // Must be hierarchy Id
                            {
                                if (!isHierarchyId)
                                {
                                    // Developer mistake
                                    throw new InvalidOperationException($"Property {propName} is not of type hierarchyid, therefore cannot use the operator '{atom.Op}'");
                                }

                                var treeSource = sources(join.Type);
                                string parentNode = isNull ? "HierarchyId::GetRoot()" :
                                    $"(SELECT [Node] FROM {treeSource} As [T] WHERE [T].[Id] = {paramSymbol})";

                                return $"{propSQL}.GetAncestor(1) = {parentNode}";
                            }

                        case Ops.descof: // Must be hierarchy Id
                            {
                                if (!isHierarchyId)
                                {
                                    // Developer mistake
                                    throw new InvalidOperationException($"Property {propName} is not of type hierarchyid, therefore cannot use the operator '{atom.Op}'");
                                }

                                var treeSource = sources(join.Type);
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

        public static string AtomSql(string symbol, string propName, string aggregation)
        {
            string sqlAggregation = null;
            switch (aggregation)
            {
                case Aggregations.count:
                    sqlAggregation = "COUNT({0})";
                    break;

                case Aggregations.dcount:
                    sqlAggregation = "COUNT(DISTINCT {0})";
                    break;

                case Aggregations.sum:
                    sqlAggregation = "SUM({0})";
                    break;

                case Aggregations.avg:
                    sqlAggregation = "AVG({0})";
                    break;

                case Aggregations.min:
                    sqlAggregation = "MIN({0})";
                    break;

                case Aggregations.max:
                    sqlAggregation = "MAX({0})";
                    break;
            }

            var result = $"[{symbol}].[{propName}]";

            // Apply the aggregation if any
            if (sqlAggregation != null)
            {
                result = string.Format(sqlAggregation, result);
            }

            return result;
        }

    }
}
