using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Tellma.Entities;
using Tellma.Entities.Descriptors;

namespace Tellma.Data.Queries
{
    /// <summary>
    /// Represents a filter argument which is a full expression tree that evaluated to a boolean.
    /// For example: "(Order.Total > 1000) and (Customer.Gender = 'M')".
    /// The filter syntax is that which can be parsed into <see cref="Queryex"/>
    /// </summary>
    public class FilterExpression
    {
        public Queryex Expression { get; private set; }

        /// <summary>
        /// Parses a string representing a filter argument into a <see cref="FilterExpression"/>. 
        /// The filter argument is a full expression tree that evaluated to a boolean.
        /// For example: "(Order.Total > 1000) and (Customer.Gender = 'M')".
        /// </summary>
        public static FilterExpression Parse(string filter)
        {
            var expressions = Queryex.Parse(filter);
            if (expressions.Skip(1).Any())
            {
                throw new InvalidOperationException("Filter parameter must be a single expression without top level commas.");
            }

            var filterExpression = expressions.First();
            if (filterExpression == null)
            {
                return null;
            }

            // Some validation
            if (filterExpression.ContainsAggregations())
            {
                throw new InvalidOperationException("Filter expression cannot contain aggregation functions like Sum or Count.");
            }

            return new FilterExpression
            {
                Expression = expressions.First()
            };
        }

        ///// <summary>
        ///// Returns all the <see cref="FilterAtom"/> in this current expression tree
        ///// </summary>
        ///// <returns></returns>
        //public abstract IEnumerable<FilterAtom> Atoms();

        ///// <summary>
        ///// Implementation of <see cref="IEnumerable{T}"/>
        ///// </summary>
        //public IEnumerator<FilterAtom> GetEnumerator()
        //{
        //    return Atoms().GetEnumerator();
        //}

        ///// <summary>
        ///// Implementation of <see cref="IEnumerable{T}"/>
        ///// </summary>
        //IEnumerator IEnumerable.GetEnumerator()
        //{
        //    return Atoms().GetEnumerator();
        //}
    }

    public static class QueryexExtensions
    {
        public static int CastingCost(QueryexType from, QueryexType to)
        {
            const int max = 100000;
            if (from == to)
            {
                return 0;
            }

            return from switch
            {
                QueryexType.String => QueryexType.AnyExceptBoolean.HasFlag(to) ? 1 : max,
                QueryexType.Numeric => to switch {
                    QueryexType.String => 1,
                    QueryexType.Numeric => 1,
                    QueryexType.Date => 1,
                    QueryexType.DateTime => 1,
                    QueryexType.DateTimeOffset => 1,
                    QueryexType.HierarchyId => 1,
                    QueryexType.Geography => 1,
                    QueryexType.Bit => 1,
                    QueryexType.Boolean => max
                },
                QueryexType.Date => 1,
                QueryexType.DateTime => 1,
                QueryexType.DateTimeOffset => 1,
                QueryexType.HierarchyId => 1,
                QueryexType.Geography => 1,
                QueryexType.Bit => 1,
                QueryexType.Boolean => max
            }
        }

        public static QueryexType FindOverload(params QueryexType[] expressions)
        {


            QueryexType result = QueryexType.None;
        }

        public static QueryexType ExpressionType(this Queryex @this, TypeDescriptor desc)
        {
            static QueryexType FunctionType(QueryexFunction exp, TypeDescriptor desc)
            {
                switch (exp.Name?.ToLower())
                {
                    case "sum":
                    case "count":
                    case "avg":
                        return QueryexType.Numeric;

                    case "min":
                    case "max":
                        return exp.Arguments[0]?.ExpressionType(desc) ?? QueryexType.None;
                    case ""
                }
            }

            static QueryexType BinaryOperatorType(QueryexBinaryOperator exp, TypeDescriptor desc)
            {
                switch (exp.Operator?.ToLower())
                {
                    case "+":
                        if (exp.Left.ExpressionType(desc).HasFlag(QueryexType.Numeric) || exp.Right.ExpressionType(desc).HasFlag(QueryexType.Numeric))
                        {
                            return QueryexType.Numeric;
                        } 
                        else
                        {
                            return QueryexType.String;
                        }

                    case "-":
                    case "*":
                    case "/":
                    case "%":
                        return QueryexType.Numeric;

                    case "&&":
                    case "||":
                    case "and":
                    case "or":
                    case "=":
                    case "!=":
                    case "<>":
                    case ">":
                    case ">=":
                    case "<":
                    case "<=":
                    case "gt":
                    case "ge":
                    case "lt":
                    case "le":
                    case "childof":
                    case "descof":
                    case "contains":
                    case "startsw":
                    case "endsw":
                        return QueryexType.Boolean;

                    default:
                        throw new InvalidOperationException($"Unknown operator {exp.Operator}"); // Future proofing
                }
            }

            static QueryexType UnaryOperatorType(QueryexUnaryOperator exp, TypeDescriptor desc) // Finished
            {
                switch (exp.Operator?.ToLower())
                {
                    case "-":
                    case "+":
                        return QueryexType.Numeric;

                    case "!":
                    case "not":
                        return QueryexType.Boolean;

                    default:
                        throw new InvalidOperationException($"Unknown operator {exp.Operator}"); // Future proofing
                }
            }

            static QueryexType ColumnAccessType(QueryexColumnAccess exp, TypeDescriptor desc) // Finished
            {
                var currentEntityDesc = desc;
                foreach (var step in exp.Path)
                {
                    var navPropDesc = currentEntityDesc.NavigationProperty(step);
                    if (navPropDesc == null)
                    {
                        throw new QueryException($"Navigation property '{step}' does not exist on type '{currentEntityDesc.Name}'");
                    }

                    currentEntityDesc = navPropDesc.GetEntityDescriptor();
                }

                var propDesc = currentEntityDesc.Property(exp.Property);
                var propType = Nullable.GetUnderlyingType(propDesc.Type) ?? propDesc.Type;

                switch (propType.Name)
                {
                    case nameof(Char):
                    case nameof(String):
                        return QueryexType.String;

                    case nameof(Byte):
                    case nameof(SByte):
                    case nameof(Int16):
                    case nameof(UInt16):
                    case nameof(Int32):
                    case nameof(UInt32):
                    case nameof(Int64):
                    case nameof(UInt64):
                    case nameof(Single):
                    case nameof(Double):
                    case nameof(Decimal):
                        return QueryexType.Numeric;

                    case nameof(Boolean):
                        return QueryexType.Bit;

                    case nameof(DateTime):
                        return propDesc.IncludesTime ? QueryexType.DateTime : QueryexType.Date;

                    case nameof(DateTimeOffset):
                        return QueryexType.DateTimeOffset;

                    case nameof(HierarchyId):
                        return QueryexType.HierarchyId;

                    case nameof(Geography):
                        return QueryexType.Geography;

                    default:
                        throw new InvalidOperationException($"Bug: Could not map type {propType.Name} to a {nameof(QueryexType)}"); // Future proofing
                }
            }

            return @this switch
            {
                QueryexFunction exp => FunctionType(exp, desc),
                QueryexBinaryOperator exp => BinaryOperatorType(exp, desc),
                QueryexUnaryOperator exp => UnaryOperatorType(exp, desc),
                QueryexQuote exp => QueryexType.String,
                QueryexNumber exp => QueryexType.Numeric,
                QueryexBit exp => QueryexType.Bit,
                QueryexNull exp => QueryexType.AnyExceptBoolean,
                QueryexColumnAccess exp => ColumnAccessType(exp, desc),
                _ => throw new InvalidOperationException($"Bug: Unknown Queryex type {@this.GetType()}"), // Future proofing
            };
        }

        public static bool IsAggregateFunction(this Queryex @this)
        {
            if (@this is QueryexFunction exp)
            {
                return exp.Name?.ToLower() switch
                {
                    "sum" => true,
                    "count" => true,
                    "avg" => true,
                    "min" => true,
                    "max" => true,
                    _ => false
                };
            }
            else
            {
                return false;
            }
        }

        public static bool ContainsAggregations(this Queryex @this)
        {
            return @this switch
            {
                QueryexFunction exp => exp.IsAggregateFunction() || exp.Arguments.Any(arg => arg.ContainsAggregations()),
                QueryexBinaryOperator exp => exp.Left.ContainsAggregations() || exp.Right.ContainsAggregations(),
                QueryexUnaryOperator exp => exp.Operand.ContainsAggregations(),
                _ => false,
            };
        }

        public static List<QueryexType> DataType(this Queryex @this, TypeDescriptor root)
        {
            switch (@this)
            {
                case QueryexNull _: return new List<QueryexType>();
                case QueryexNull _: return new List<QueryexType>();
                case QueryexNull _: return new List<QueryexType>();
                case QueryexNull _: return new List<QueryexType>();
                case QueryexNull _: return new List<QueryexType>();
            }

            return @this switch
            {
                QueryexFunction exp => exp.IsAggregateFunction() || exp.Arguments.Any(arg => arg.ContainsAggregations()),
                QueryexBinaryOperator exp => exp.Left.ContainsAggregations() || exp.Right.ContainsAggregations(),
                QueryexUnaryOperator exp => exp.Operand.ContainsAggregations(),
                _ => false,
            };
        }

        public static bool IsBoolean(this Queryex @this)
        {
            return @this switch
            {
                QueryexFunction exp => exp.Name?.ToLower() switch
                {
                    "not" => true,
                    "contains" => true,
                    "descof" => true,
                    "childof" => true,
                    _ => false
                },
                QueryexBinaryOperator exp => exp.Left.ContainsAggregations() || exp.Right.ContainsAggregations(),
                QueryexUnaryOperator exp => exp.Operand.ContainsAggregations(),
                _ => false,
            };
        }

        public static readonly Dictionary<string, QueryexFunctionDescriptor> _functions = new Dictionary<string, QueryexFunctionDescriptor>(StringComparer.OrdinalIgnoreCase)
        {
            ["if"] = new QueryexFunctionDescriptor
            {
                Parameters = new QueryexParamDescriptor[]
                {
                    new QueryexParamDescriptor { Name = "condition", Type = QueryexType.Boolean },
                    new QueryexParamDescriptor { Name = "value_if_true", Type = QueryexType.X },
                    new QueryexParamDescriptor { Name = "value_if_false", Type = QueryexType.X }
                },
                ResultType = QueryexType.X
            },
            ["sum"] = new QueryexFunctionDescriptor
            {
                Parameters = new QueryexParamDescriptor[]
                {
                    new QueryexParamDescriptor { Name = "expression", Type = QueryexType.Boolean },
                    new QueryexParamDescriptor { Name = "condition", Type = QueryexType.X, IsOptional = true },
                },
                ResultType = QueryexType.X
            },
        };

        public static QueryexType CommonType(QueryexType t1, QueryexType t2)
        {
            if (t1.HasFlag(t2))
            {
                return t2; // t2 is more specific
            }
            else if (t2.HasFlag(t1))
            {
                return t1; // t2 is more specific
            }


            return 0;
        }
    }

    public class QueryexFunctionDescriptor
    {
        public QueryexParamDescriptor[] Parameters { get; set; }

        public QueryexType ResultType { get; set; }
    }

    public class QueryexUnaryOpDescriptor
    {

        public QueryexParamDescriptor Operand { get; set; }

        public Func<QueryexType, QueryexType> ResultType { get; set; }
    }

    public class QueryexBinaryOpDescriptor
    {
        public QueryexParamDescriptor Left { get; set; }

        public QueryexParamDescriptor Right { get; set; }

        public Func<QueryexType, QueryexType, QueryexType> ResultType { get; set; }
    }

    public class QueryexParamDescriptor
    {
        public string Name { get; set; }

        public bool IsOptional { get; set; }

        public QueryexType Type { get; set; } // Empty means any
    }
}
