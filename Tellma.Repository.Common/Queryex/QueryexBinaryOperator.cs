using System;
using System.Collections.Generic;
using System.Linq;
using Tellma.Model.Common;

namespace Tellma.Repository.Common.Queryex
{
    /// <summary>
    /// Represents a binary operator.
    /// <para/>
    /// Examples:<br/> 
    /// - E1 + E2<br/>
    /// - E1 contains E2<br/>
    /// - E1 and E2<br/>
    /// </summary>
    public class QueryexBinaryOperator : QueryexBase
    {
        public QueryexBinaryOperator(string op, QueryexBase left, QueryexBase right)
        {
            Operator = op ?? throw new ArgumentNullException(nameof(op));
            Left = left ?? throw new ArgumentNullException(nameof(left));
            Right = right ?? throw new ArgumentNullException(nameof(right));
        }

        /// <summary>
        /// The binary operator.
        /// </summary>
        public string Operator { get; }

        /// <summary>
        /// The expression that is the left operand of the binary operator.
        /// </summary>
        public QueryexBase Left { get; }

        /// <summary>
        /// The expression that is the right operand of the binary operator.
        /// </summary>
        public QueryexBase Right { get; }

        public override string ToString()
        {
            return $"({Left} {Operator} {Right})";
        }

        public override IEnumerable<QueryexBase> Children
        {
            get
            {
                yield return Left;
                yield return Right;
            }
        }

        public override bool TryCompile(QxType targetType, QxCompilationContext ctx, out string resultSql, out QxNullity resultNullity)
        {
            // This here is merely an optimization
            if (Operator == "+")
            {
                if ((targetType == QxType.String && // String concatenation
                    Left.TryCompile(QxType.String, ctx, out string leftSql, out QxNullity leftNullity) &&
                    Right.TryCompile(QxType.String, ctx, out string rightSql, out QxNullity rightNullity)) ||
                    (targetType == QxType.Numeric && // Numeric
                    Left.TryCompile(QxType.Numeric, ctx, out leftSql, out leftNullity) &&
                    Right.TryCompile(QxType.Numeric, ctx, out rightSql, out rightNullity)))
                {
                    resultNullity = leftNullity | rightNullity;
                    resultSql = $"({leftSql} + {rightSql})";
                    return true;
                }
                else
                {
                    // No other types are possible for +
                    resultNullity = default;
                    resultSql = null;
                    return false;
                }
            }

            return base.TryCompile(targetType, ctx, out resultSql, out resultNullity);
        }

        public override (string, QxType, QxNullity) CompileNative(QxCompilationContext ctx)
        {
            // Note: The way the logic is structured assumes that for ALL operators
            // if either operand is NULL, then the result is NULL

            // Convenience variables
            string leftSql;
            string rightSql;
            QxNullity leftNullity;
            QxNullity rightNullity;

            // The result
            string resultSql;
            QxType resultType;
            QxNullity resultNullity;

            string opLower = Operator?.ToLower();
            switch (opLower)
            {
                case "+":
                    // + maye be either addition or string concatenation
                    // The output type is uniquely determined by the input types (target output type doesn't matter)
                    // since there is no implicit cast from numeric to string or vice versa
                    {
                        if (Left.TryCompile(QxType.Numeric, ctx, out leftSql, out leftNullity) &&
                            Right.TryCompile(QxType.Numeric, ctx, out rightSql, out rightNullity))
                        {
                            // Addition
                            resultType = QxType.Numeric;
                        }
                        else if (Left.TryCompile(QxType.String, ctx, out leftSql, out leftNullity) &&
                            Right.TryCompile(QxType.String, ctx, out rightSql, out rightNullity))
                        {
                            // String concatenation
                            resultType = QxType.String;
                        }
                        else
                        {
                            throw new QueryException($"Operator '{opLower}' cannot be used on expressions {Left} and {Right} because they have incompatible data types.");
                        }

                        resultNullity = leftNullity | rightNullity;
                        resultSql = $"({leftSql} + {rightSql})";
                        break;
                    }

                case "-":
                case "*":
                case "/":
                case "%":
                    // These only accept numerics and return a numeric
                    {
                        resultType = QxType.Numeric;

                        string opSql = opLower;

                        if (!Left.TryCompile(QxType.Numeric, ctx, out leftSql, out leftNullity))
                        {
                            throw new QueryException($"Operator '{Operator}': Left operand {Left} could not be interpreted as {QxType.Numeric}.");
                        }

                        if (!Right.TryCompile(QxType.Numeric, ctx, out rightSql, out rightNullity))
                        {
                            throw new QueryException($"Operator '{Operator}': Right operand {Right} could not be interpreted as {QxType.Numeric}.");

                        }
                        resultNullity = leftNullity | rightNullity;
                        resultSql = $"({leftSql} {opSql} {rightSql})";
                        break;

                    }

                case "&&":
                case "and":
                case "||":
                case "or":
                    // These only accept booleans and return a boolean
                    {
                        resultType = QxType.Boolean;

                        string opSql = opLower switch
                        {
                            "&&" => "AND",
                            "||" => "OR",
                            "and" => "AND",
                            "or" => "OR",
                            _ => opLower,
                        };

                        if (Left.TryCompile(QxType.Boolean, ctx, out leftSql, out leftNullity))
                        {
                            if (Right.TryCompile(QxType.Boolean, ctx, out rightSql, out rightNullity))
                            {
                                resultNullity = leftNullity | rightNullity;
                                if (resultNullity != QxNullity.NotNull)
                                {
                                    // Developer mistake
                                    throw new InvalidOperationException($"[Bug] A nullable boolean expression: {this}");
                                }

                                if (opSql == "AND")
                                {
                                    if (leftSql == FALSE || rightSql == FALSE)
                                    {
                                        resultSql = FALSE;
                                    }
                                    else if (leftSql == TRUE && rightSql == TRUE)
                                    {
                                        resultSql = TRUE;
                                    }
                                    else if (leftSql == TRUE)
                                    {
                                        resultSql = rightSql;
                                    }
                                    else if (rightSql == TRUE)
                                    {
                                        resultSql = leftSql;
                                    }
                                    else
                                    {
                                        resultSql = $"({leftSql} {opSql} {rightSql})";
                                    }

                                    break;
                                }
                                else if (opSql == "OR")
                                {
                                    if (leftSql == TRUE || rightSql == TRUE)
                                    {
                                        resultSql = TRUE;
                                    }
                                    else if (leftSql == FALSE && rightSql == FALSE)
                                    {
                                        resultSql = FALSE;
                                    }
                                    else if (leftSql == FALSE)
                                    {
                                        resultSql = rightSql;
                                    }
                                    else if (rightSql == FALSE)
                                    {
                                        resultSql = leftSql;
                                    }
                                    else
                                    {
                                        resultSql = $"({leftSql} {opSql} {rightSql})";
                                    }

                                    break;
                                }
                                else
                                {
                                    // Developer mistake
                                    throw new InvalidOperationException($"Unknown binary logical operator {opSql}.");
                                }
                            }
                            else
                            {
                                throw new QueryException($"Expression {Right} does not have a boolean type, it cannot be used with operator '{Operator}'.");
                            }
                        }
                        else
                        {
                            throw new QueryException($"Expression {Left} does not have a boolean type, it cannot be used with operator '{Operator}'.");
                        }
                    }

                case "<>":
                case "!=":
                case "ne":
                case ">":
                case "gt":
                case ">=":
                case "ge":
                case "<":
                case "lt":
                case "<=":
                case "le":
                case "=":
                case "eq":
                    // These accept any data type (Except boolean) and always spit out a boolean
                    {
                        resultType = QxType.Boolean;

                        // Translate to SQL operator
                        string opSql = opLower switch
                        {
                            "eq" => "=",
                            "ne" => "<>",
                            "gt" => ">",
                            "ge" => ">=",
                            "lt" => "<",
                            "le" => "<=",
                            _ => opLower,
                        };

                        QxType leftType;
                        (leftSql, leftType, leftNullity) = Left.CompileNative(ctx);
                        if (leftType == QxType.Boolean)
                        {
                            throw new QueryException($"Operator '{Operator}': The left operand {Left} cannot be a {QxType.Boolean} expression.");
                        }

                        QxType rightType;
                        (rightSql, rightType, rightNullity) = Right.CompileNative(ctx);
                        if (rightType == QxType.Boolean)
                        {
                            throw new QueryException($"Operator '{Operator}': The right operand {Right} cannot be a {QxType.Boolean} expression.");
                        }

                        if ((leftType == rightType) ||
                            (leftType > rightType && Left.TryCompile(rightType, ctx, out leftSql, out leftNullity)) ||
                            (rightType > leftType && Right.TryCompile(leftType, ctx, out rightSql, out rightNullity)))
                        {
                            // Comparison functions always return a non nullable boolean
                            resultNullity = QxNullity.NotNull;
                            if (opSql == "=")
                            {
                                resultSql = leftNullity switch
                                {
                                    QxNullity.NotNull => rightNullity switch
                                    {
                                        QxNullity.NotNull => $"({leftSql} = {rightSql})",
                                        QxNullity.Nullable => $"({rightSql} IS NOT NULL AND {leftSql} = {rightSql})",
                                        QxNullity.Null => FALSE,
                                        _ => throw new InvalidOperationException($"Unknown nullity {rightNullity}"),
                                    },
                                    QxNullity.Nullable => rightNullity switch
                                    {
                                        QxNullity.NotNull => $"({leftSql} IS NOT NULL AND {leftSql} = {rightSql})",
                                        QxNullity.Nullable => $"(({leftSql} IS NOT NULL AND {rightSql} IS NOT NULL AND {leftSql} = {rightSql}) OR ({leftSql} IS NULL AND {rightSql} IS NULL))",
                                        QxNullity.Null => $"({leftSql} IS NULL)",
                                        _ => throw new InvalidOperationException($"Unknown nullity {rightNullity}"),
                                    },
                                    QxNullity.Null => rightNullity switch
                                    {
                                        QxNullity.NotNull => FALSE,
                                        QxNullity.Nullable => $"({rightSql} IS NULL)",
                                        QxNullity.Null => TRUE,
                                        _ => throw new InvalidOperationException($"Unknown nullity {rightNullity}"),
                                    },
                                    _ => throw new InvalidOperationException($"Unknown nullity {leftNullity}"),
                                };

                                break;
                            }
                            else if (opSql == "!=" || opSql == "<>")
                            {
                                resultSql = leftNullity switch
                                {
                                    QxNullity.NotNull => rightNullity switch
                                    {
                                        QxNullity.NotNull => $"({leftSql} <> {rightSql})",
                                        QxNullity.Nullable => $"({rightSql} IS NULL OR {leftSql} <> {rightSql})",
                                        QxNullity.Null => TRUE,
                                        _ => throw new InvalidOperationException($"Unknown nullity {rightNullity}"),
                                    },
                                    QxNullity.Nullable => rightNullity switch
                                    {
                                        QxNullity.NotNull => $"({leftSql} IS NULL OR {leftSql} <> {rightSql})",
                                        QxNullity.Nullable => $"(({leftSql} IS NULL OR {rightSql} IS NULL OR {leftSql} <> {rightSql}) AND ({leftSql} IS NOT NULL OR {rightSql} IS NOT NULL))",
                                        QxNullity.Null => $"({leftSql} IS NOT NULL)",
                                        _ => throw new InvalidOperationException($"Unknown nullity {rightNullity}"),
                                    },
                                    QxNullity.Null => rightNullity switch
                                    {
                                        QxNullity.NotNull => TRUE,
                                        QxNullity.Nullable => $"({rightSql} IS NOT NULL)",
                                        QxNullity.Null => FALSE,
                                        _ => throw new InvalidOperationException($"Unknown nullity {rightNullity}"),
                                    },
                                    _ => throw new InvalidOperationException($"Unknown nullity {leftNullity}"),
                                };

                                break;
                            }
                            else // Comparison
                            {
                                resultSql = leftNullity switch
                                {
                                    QxNullity.NotNull => rightNullity switch
                                    {
                                        QxNullity.NotNull => $"({leftSql} {opSql} {rightSql})",
                                        QxNullity.Nullable => $"({rightSql} IS NOT NULL AND {leftSql} {opSql} {rightSql})",
                                        QxNullity.Null => FALSE,
                                        _ => throw new InvalidOperationException($"Unknown nullity {rightNullity}"),
                                    },
                                    QxNullity.Nullable => rightNullity switch
                                    {
                                        QxNullity.NotNull => $"({leftSql} IS NOT NULL AND {leftSql} {opSql} {rightSql})",
                                        QxNullity.Nullable => $"({leftSql} IS NOT NULL AND {rightSql} IS NOT NULL AND {leftSql} {opSql} {rightSql})",
                                        QxNullity.Null => FALSE,
                                        _ => throw new InvalidOperationException($"Unknown nullity {rightNullity}"),
                                    },
                                    QxNullity.Null => FALSE,
                                    _ => throw new InvalidOperationException($"Unknown nullity {leftNullity}"),
                                };

                                break;
                            }

                            // Should never reach this in theory
                            throw new InvalidOperationException($"[Bug] nullability checks for '{opSql}' were not exhaustive.");
                        }
                        else
                        {
                            // No mutual type was found 
                            throw new QueryException($"Operator '{Operator}' cannot be used on expressions {Left} and {Right} because they have incompatible data types.");
                        }
                    }

                case "descof":
                    // Accepts any data type (Except boolean) and always spit out a non-null boolean
                    // With the proviso that the first operand is always a column access and the 2nd operand does not contain any column access
                    {
                        resultType = QxType.Boolean;

                        // Make sure the left side is vanilla column access
                        if (Left is not QueryexColumnAccess columnAccess)
                        {
                            throw new QueryException($"Operator '{Operator}': The left operand {Left} must be a column access like AccountType.Concept.");
                        }

                        // Make sure the right side contains no column access
                        QueryexColumnAccess ca = Right.ColumnAccesses().FirstOrDefault();
                        if (ca == Right)
                        {
                            throw new QueryException($"Operator '{Operator}': The right operand cannot be a column access expression like {ca}.");
                        }
                        else if (ca != null)
                        {
                            throw new QueryException($"Operator '{Operator}': The right operand cannot contain a column access expression like {ca}.");
                        }

                        QxType leftType;
                        QxType rightType;
                        (leftSql, leftType, leftNullity) = Left.CompileNative(ctx);
                        (rightSql, rightType, rightNullity) = Right.CompileNative(ctx);
                        if (rightType == QxType.Boolean)
                        {
                            throw new QueryException($"Operator '{Operator}': The right operand {Right} cannot be a {QxType.Boolean} expression.");
                        }

                        if ((leftType == rightType) ||
                            (leftType > rightType && Left.TryCompile(rightType, ctx, out leftSql, out leftNullity)) ||
                            (rightType > leftType && Right.TryCompile(leftType, ctx, out rightSql, out rightNullity)))
                        {
                            resultNullity = QxNullity.NotNull;

                            // Prepare the operands
                            var join = ctx.Joins[columnAccess.Path];
                            if (join == null)
                            {
                                // Developer mistake
                                throw new InvalidOperationException($"Bug: The path '{this}' was not found in the joinTree.");
                            }

                            // Make sure it's a tree type
                            var nodeDesc = join.EntityDescriptor.Property("Node");
                            if (nodeDesc == null || nodeDesc.Type != typeof(HierarchyId))
                            {
                                throw new QueryException($"Operator '{Operator}' cannot be used on type {join.EntityDescriptor.Name} since it is not a tree type.");
                            }

                            var propName = columnAccess.Property;
                            var propDesc = join.EntityDescriptor.Property(propName);
                            if (propDesc == null)
                            {
                                // To prevent SQL injection
                                throw new QueryException($"Property '{propName}' does not exist on type {join.EntityDescriptor.Name}.");
                            }

                            // Add a variable before the statement to store the Node of the ancestor
                            string treeSource = ctx.Sources(join.EntityDescriptor.Type);
                            string varDef = $"ISNULL((SELECT TOP 1 [Node] FROM {treeSource} As [T] WHERE [T].[{propName}] = {rightSql}), HIERARCHYID::GetRoot())";
                            string varName = ctx.Variables.AddVariable("HIERARCHYID", varDef);

                            // Use the variable name in the query (more efficient)
                            resultSql = $"([{join.Symbol}].[Node].IsDescendantOf(@{varName}) = 1)";
                            break;
                        }
                        else
                        {
                            // No mutual type was found  
                            throw new QueryException($"Operator '{Operator}' cannot be used on expressions {Left} and {Right} because they have incompatible data types.");
                        }
                    }

                case "contains":
                case "startsw":
                case "endsw":
                    {
                        resultType = QxType.Boolean;

                        if (!Left.TryCompile(QxType.String, ctx, out leftSql, out leftNullity))
                        {
                            throw new QueryException($"Operator '{Operator}': Left operand {Left} could not be interpreted as {QxType.String}.");
                        }

                        if (Right is QueryexQuote quote)
                        {
                            // Since it will be used as the 2nd operand of a LIKE, it must be escaped
                            quote.EscapeForLike();
                        }

                        if (!Right.TryCompile(QxType.String, ctx, out rightSql, out rightNullity))
                        {
                            throw new QueryException($"Operator '{Operator}': Right operand {resultType} could not be interpreted as {QxType.String}.");
                        }

                        resultNullity = QxNullity.NotNull;

                        // Process right SQL to make it suitable as the second operand for the LIKE operator
                        string beforePercent = opLower == "contains" || opLower == "endsw" ? "N'%' + " : "";
                        string afterPercent = opLower == "contains" || opLower == "startsw" ? " + N'%'" : "";

                        // Escape the 2nd LIKE operand unless it's a quote then we already escaped it earlier
                        string escapedRightSql = rightSql;
                        if (!(Right is QueryexQuote))
                        {
                            escapedRightSql = $"REPLACE(REPLACE({escapedRightSql}, N'%', N'[%]'), N'_', N'[_]')";
                        }

                        escapedRightSql = $"{beforePercent}{escapedRightSql}{afterPercent}";

                        resultSql = leftNullity switch
                        {
                            QxNullity.NotNull => rightNullity switch
                            {
                                QxNullity.NotNull => $"({leftSql} LIKE {escapedRightSql})",
                                QxNullity.Nullable => $"({rightSql} IS NOT NULL AND {leftSql} LIKE {escapedRightSql})",
                                QxNullity.Null => FALSE,
                                _ => throw new InvalidOperationException($"Unknown nullity {rightNullity}"),
                            },
                            QxNullity.Nullable => rightNullity switch
                            {
                                QxNullity.NotNull => $"({leftSql} IS NOT NULL AND {leftSql} LIKE {escapedRightSql})",
                                QxNullity.Nullable => $"({leftSql} IS NOT NULL AND {rightSql} IS NOT NULL AND {leftSql} LIKE {escapedRightSql})",
                                QxNullity.Null => FALSE,
                                _ => throw new InvalidOperationException($"Unknown nullity {rightNullity}"),
                            },
                            QxNullity.Null => FALSE,
                            _ => throw new InvalidOperationException($"Unknown nullity {leftNullity}"),
                        };

                        break;
                    }

                default:
                    // Developer mistake
                    throw new InvalidOperationException($"Unknown binary operator {Operator}"); // Future proofing
            }

            // Return the result (or NULL if that's the only possible value)
            if (resultNullity == QxNullity.Null)
            {
                resultSql = "NULL";
            }

            return (resultSql, resultType, resultNullity);
        }

        public override bool Equals(object exp)
        {
            return exp is QueryexBinaryOperator bo
                && StringComparer.OrdinalIgnoreCase.Equals(bo.Operator, Operator)
                && bo.Left.Equals(Left)
                && bo.Right.Equals(Right);
        }

        public override int GetHashCode()
        {
            int opCode = StringComparer.OrdinalIgnoreCase.GetHashCode(Operator);
            return opCode ^ Left.GetHashCode() ^ Right.GetHashCode();
        }

        public override QueryexBase Clone(string[] prefix = null) => new QueryexBinaryOperator(Operator, Left.Clone(prefix), Right.Clone(prefix));
    }
}
