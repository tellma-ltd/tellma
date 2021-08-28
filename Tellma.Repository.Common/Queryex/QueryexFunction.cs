using System;
using System.Collections.Generic;
using System.Linq;
using Tellma.Utilities.Calendars;
using Tellma.Utilities.Common;

namespace Tellma.Repository.Common.Queryex
{
    /// <summary>
    /// Represents a function invocation.
    /// <para/>
    /// Examples:<br/> 
    /// - Sum(...)<br/>
    /// - IsNull(...)<br/>
    /// - StartOfMonth(...)<br/>
    /// </summary>
    public class QueryexFunction : QueryexBase
    {
        public QueryexFunction(string name, params QueryexBase[] args)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            Name = name;
            Arguments = args ?? throw new ArgumentNullException(nameof(args));
        }

        /// <summary>
        /// The name of the function.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The list of expressions that are the function arguments in their original order.
        /// </summary>
        public QueryexBase[] Arguments { get; }

        public override string ToString()
        {
            return $"{Name}({string.Join(", ", Arguments.Select(e => e.ToString().DeBracket()))})";
        }

        public override IEnumerable<QueryexBase> Children => Arguments;

        public override bool TryCompile(QxType targetType, QxCompilationContext ctx, out string resultSql, out QxNullity resultNullity)
        {
            string nameLower = Name?.ToLower();
            switch (nameLower)
            {
                case "min":
                case "max":
                    {
                        var (arg1, conditionSql) = AggregationParameters(ctx);

                        if (arg1.TryCompile(targetType, ctx, out string expSql, out resultNullity))
                        {
                            resultSql = AggregationCompile(expSql, conditionSql);
                            return true;
                        }
                        else
                        {
                            resultSql = null;
                            resultNullity = default;
                            return false;
                        }
                    }
                case "adddays":
                case "addmonths":
                case "addyears": // (date: Date | DateTime | DateTimeOffset, number: numeric) => Date | DateTime | DateTimeOffset
                    {
                        if (targetType == QxType.Date || targetType == QxType.DateTime || targetType == QxType.DateTimeOffset)
                        {
                            var (numberSql, arg2) = AddDatePartParameters(ctx, nameLower);

                            if (arg2.TryCompile(targetType, ctx, out string dateSql, out QxNullity dateNullity))
                            {
                                // Calculate the result
                                (resultSql, resultNullity) = AddDatePartCompile(nameLower, numberSql, dateSql, dateNullity);
                                return true;
                            }
                            else
                            {
                                resultSql = null;
                                resultNullity = default;
                                return false;
                            }
                        }
                        else
                        {
                            resultSql = null;
                            resultNullity = default;
                            return false;
                        }
                    }

                case "if": // (condition: boolean, value_if_true: X, value_if_false: X) => X
                    {
                        var (conditionSql, arg2, arg3) = IfParameters(ctx);

                        if (arg2.TryCompile(targetType, ctx, out string ifTrueSql, out QxNullity ifTrueNullity) &&
                            arg3.TryCompile(targetType, ctx, out string ifFalseSql, out QxNullity ifFalseNullity))
                        {
                            (resultSql, resultNullity) = IfCompile(conditionSql, ifTrueSql, ifTrueNullity, ifFalseSql, ifFalseNullity);
                            return true;
                        }
                        else
                        {
                            resultSql = null;
                            resultNullity = default;
                            return false;
                        }
                    }

                case "isnull": // (value: X, fallback_value: X) => X
                    {
                        var (arg1, arg2) = IsNullParameters();

                        if (arg1.TryCompile(targetType, ctx, out string expSql, out QxNullity expNullity) &&
                            arg2.TryCompile(targetType, ctx, out string replacementSql, out QxNullity replacementNullity))
                        {
                            (resultSql, resultNullity) = IsNullCompile(expSql, expNullity, replacementSql, replacementNullity);
                            return true;
                        }
                        else
                        {
                            resultSql = null;
                            resultNullity = default;
                            return false;
                        }
                    }
                case "today": // () => date
                    {
                        if (targetType == QxType.Date || targetType == QxType.DateTime)
                        {
                            (resultSql, resultNullity) = CompileToday(ctx, targetType);
                            return true;
                        }
                        else
                        {
                            resultSql = null;
                            resultNullity = default;
                            return false;
                        }
                    }

                default:
                    return base.TryCompile(targetType, ctx, out resultSql, out resultNullity);

            }
        }

        public override (string sql, QxType type, QxNullity nullity) CompileNative(QxCompilationContext ctx)
        {
            // The result
            string resultSql;
            QxType resultType;
            QxNullity resultNullity;

            string nameLower = Name?.ToLower();
            switch (nameLower)
            {
                case "sum":
                case "count":
                case "avg":
                case "min":
                case "max":
                    {
                        var (arg1, conditionSql) = AggregationParameters(ctx);

                        string expSql;
                        if (nameLower == "count" || nameLower == "max" || nameLower == "min")
                        {
                            // Accept anything except boolean
                            (expSql, resultType, resultNullity) = arg1.CompileNative(ctx);
                            if (resultType == QxType.Boolean)
                            {
                                throw new QueryException($"Function '{Name}': The first argument {arg1} cannot be a {QxType.Boolean} expression.");
                            }

                            if (nameLower == "count")
                            {
                                // Count always returns numeric, the other two return the same type of their argument
                                resultType = QxType.Numeric;
                            }
                        }
                        else if (arg1.TryCompile(QxType.Numeric, ctx, out expSql, out resultNullity))
                        {
                            // Accept only numeric and return only numeric
                            resultType = QxType.Numeric; // The other 2 both take numeric and return numeric
                        }
                        else
                        {
                            throw new QueryException($"Function '{Name}': The first argument {arg1} could not be interpreted as a {QxType.Numeric}.");
                        }

                        resultSql = AggregationCompile(expSql, conditionSql);
                        break;
                    }

                case "year":
                case "quarter":
                case "month":
                case "day": // (date: Date | DateTime | DateTimeOffset, calendar?: string) => numeric
                    {
                        if (Arguments.Length < 1 || Arguments.Length > 2)
                        {
                            throw new QueryException($"No overload for function '{Name}' accepts {Arguments.Length} arguments.");
                        }

                        string datePart = nameLower;

                        var arg1 = Arguments[0];
                        if (arg1.TryCompile(QxType.Date, ctx, out string dateSql, out QxNullity dateNullity) ||
                            arg1.TryCompile(QxType.DateTime, ctx, out dateSql, out dateNullity) ||
                            arg1.TryCompile(QxType.DateTimeOffset, ctx, out dateSql, out dateNullity))
                        {
                            string calendar = Calendars.Gregorian; // Default
                            if (Arguments.Length >= 2)
                            {
                                var arg2 = Arguments[1];
                                if (arg2 is QueryexQuote calendarQuote)
                                {
                                    calendar = calendarQuote.Value.ToLower();
                                }
                                else
                                {
                                    throw new QueryException($"Function '{Name}': The second argument must be a simple quote like this: '{Calendars.UmAlQura.ToUpper()}'.");
                                }
                            }

                            resultType = QxType.Numeric;
                            resultNullity = dateNullity;
                            resultSql = calendar switch
                            {
                                Calendars.Gregorian => $"DATEPART({datePart.ToUpper()}, {dateSql.DeBracket()})", // Use SQL's built in function
                                Calendars.UmAlQura => $"[dbo].[fn_UmAlQura_DatePart]('{datePart[0]}', {dateSql.DeBracket()})",
                                Calendars.Ethiopian => $"[dbo].[fn_Ethiopian_DatePart]('{datePart[0]}', {dateSql.DeBracket()})",

                                _ => throw new QueryException(
                                    $"Function '{Name}': The second argument {Arguments[1]} must be one of the supported calendars: '{string.Join("', '", Calendars.SupportedCalendars.Select(e => e.ToUpper()))}'.")
                            };

                            break;
                        }
                        else
                        {
                            throw new QueryException($"Function '{Name}': The first argument {arg1} could not be interpreted as a {QxType.Date}, {QxType.DateTime} or {QxType.DateTimeOffset}.");
                        }
                    }

                case "weekday": // (date: Date | DateTime | DateTimeOffset) => numeric
                case "hour":
                case "minute":
                case "second": // (date: DateTime | DateTimeOffset) => numeric
                    {
                        int expectedArgCount = 1;
                        if (Arguments.Length != expectedArgCount)
                        {
                            throw new QueryException($"Function '{Name}' accepts exactly {expectedArgCount} argument(s).");
                        }

                        string datePart = nameLower;

                        // Those do not accept a QxType.Date
                        bool supportsDate = datePart == "weekday"; // Only this one accepts a date

                        var arg1 = Arguments[0];
                        if ((supportsDate && arg1.TryCompile(QxType.Date, ctx, out string dateSql, out QxNullity dateNullity)) ||
                            arg1.TryCompile(QxType.DateTime, ctx, out dateSql, out dateNullity) ||
                            arg1.TryCompile(QxType.DateTimeOffset, ctx, out dateSql, out dateNullity))
                        {
                            resultType = QxType.Numeric;
                            resultNullity = dateNullity;
                            resultSql = $"DATEPART({datePart.ToUpper()}, {dateSql.DeBracket()})";

                            break;
                        }
                        else
                        {
                            throw new QueryException($"Function '{Name}': The first argument {arg1} could not be interpreted as a {(supportsDate ? $"{QxType.Date}, " : "")}{QxType.DateTime} or {QxType.DateTimeOffset}.");
                        }
                    }

                case "addyears":
                case "addmonths":
                case "adddays": // (number: numeric, date: Date | DateTime | DateTimeOffset) => Date | DateTime | DateTimeOffset
                    {
                        var (numberSql, arg2) = AddDatePartParameters(ctx, nameLower);

                        // Argument #2 Date
                        if (arg2.TryCompile(QxType.Date, ctx, out string dateSql, out QxNullity dateNullity))
                        {
                            resultType = QxType.Date;
                        }
                        else if (arg2.TryCompile(QxType.DateTime, ctx, out dateSql, out dateNullity))
                        {
                            resultType = QxType.DateTime;
                        }
                        else if (arg2.TryCompile(QxType.DateTimeOffset, ctx, out dateSql, out dateNullity))
                        {
                            resultType = QxType.DateTimeOffset;
                        }
                        else
                        {
                            throw new QueryException($"Function '{Name}': The second argument {arg2} could not be interpreted as a {QxType.Date}, {QxType.DateTime} or {QxType.DateTimeOffset}.");
                        }

                        // Calculate the result
                        (resultSql, resultNullity) = AddDatePartCompile(nameLower, numberSql, dateSql, dateNullity);
                        break;
                    }

                case "startofyear":
                case "startofmonth":
                case "date": // (date: Date | DateTime | DateTimeOffset) => Date
                    {
                        if (Arguments.Length < 1 || Arguments.Length > 2)
                        {
                            throw new QueryException($"No overload for function '{Name}' accepts {Arguments.Length} arguments.");
                        }

                        // Argument #1: Date
                        var arg1 = Arguments[0];
                        QxType argumentType;
                        if (arg1.TryCompile(QxType.Date, ctx, out string dateSql, out QxNullity dateNullity))
                        {
                            argumentType = QxType.Date;
                        }
                        else if (arg1.TryCompile(QxType.DateTime, ctx, out dateSql, out dateNullity))
                        {
                            argumentType = QxType.DateTime;
                        }
                        else if (arg1.TryCompile(QxType.DateTimeOffset, ctx, out dateSql, out dateNullity))
                        {
                            argumentType = QxType.DateTimeOffset;
                        }
                        else
                        {
                            throw new QueryException($"Function '{Name}': The argument {arg1} could not be interpreted as a {QxType.Date}, {QxType.DateTime} or {QxType.DateTimeOffset}.");
                        }

                        // Argument #2: Calendar
                        string calendar = Calendars.Gregorian; // Default
                        if (Arguments.Length >= 2)
                        {
                            var arg2 = Arguments[1];
                            if (arg2 is QueryexQuote calendarQuote)
                            {
                                calendar = calendarQuote.Value.ToLower();
                            }
                            else
                            {
                                throw new QueryException($"Function '{Name}': The second argument must be a simple quote like this: '{Calendars.UmAlQura.ToUpper()}'.");
                            }
                        }

                        resultType = QxType.Date; // Always date
                        resultNullity = dateNullity;

                        switch (nameLower)
                        {
                            case "date":
                                // Date is calendar independent
                                if (argumentType == QxType.Date)
                                {
                                    resultSql = dateSql; // Return the date as is
                                }
                                else
                                {
                                    resultSql = $"CAST({dateSql} AS DATE)";
                                }
                                break;
                            case "startofmonth":
                                resultSql = calendar switch
                                {
                                    Calendars.Gregorian => $"DATEADD(DAY, 1, EOMONTH({dateSql.DeBracket()}, -1))", // resultSql = $"DATEFROMPARTS(YEAR({dateSql}), MONTH({dateSql}), 1)";
                                    Calendars.UmAlQura => $"[dbo].[fn_UmAlQura_StartOfMonth]({dateSql.DeBracket()})",
                                    Calendars.Ethiopian => $"[dbo].[fn_Ethiopian_StartOfMonth]({dateSql.DeBracket()})",

                                    _ => throw new QueryException($"Function '{Name}': The second argument {Arguments[1]} must be one of the supported calendars: '{string.Join("', '", Calendars.SupportedCalendars.Select(e => e.ToUpper()))}'.")
                                };
                                break;
                            case "startofyear":
                                resultSql = calendar switch
                                {
                                    Calendars.Gregorian => $"DATEFROMPARTS(YEAR({dateSql.DeBracket()}), 1, 1)",
                                    Calendars.UmAlQura => $"[dbo].[fn_UmAlQura_StartOfYear]({dateSql.DeBracket()})",
                                    Calendars.Ethiopian => $"[dbo].[fn_Ethiopian_StartOfYear]({dateSql.DeBracket()})",

                                    _ => throw new QueryException($"Function '{Name}': The second argument {Arguments[1]} must be one of the supported calendars: '{string.Join("', '", Calendars.SupportedCalendars.Select(e => e.ToUpper()))}'.")
                                };
                                break;
                            default:
                                throw new InvalidOperationException($"Unhandled {nameLower}");
                        }

                        break;
                    }

                //case "diffyears":
                //case "diffmonths":
                case "diffdays":
                case "diffhours":
                case "diffminutes":
                case "diffseconds": // (date1: Date | DateTime | DateTimeOffset, date2: Date | DateTime | DateTimeOffset) => numeric
                    {
                        int expectedArgCount = 2;
                        if (Arguments.Length != expectedArgCount)
                        {
                            throw new QueryException($"Function '{Name}' accepts exactly {expectedArgCount} argument(s).");
                        }

                        // Arguments #1 a Date
                        var arg1 = Arguments[0];
                        if (!(arg1.TryCompile(QxType.Date, ctx, out string date1Sql, out QxNullity date1Nullity) ||
                            arg1.TryCompile(QxType.DateTime, ctx, out date1Sql, out date1Nullity) ||
                            arg1.TryCompile(QxType.DateTimeOffset, ctx, out date1Sql, out date1Nullity)))
                        {
                            throw new QueryException($"Function '{Name}': The first argument {arg1} could not be interpreted as a {QxType.Date}, {QxType.DateTime} or {QxType.DateTimeOffset}.");
                        }

                        var arg2 = Arguments[1];
                        if (!(arg2.TryCompile(QxType.Date, ctx, out string date2Sql, out QxNullity date2Nullity) ||
                        arg2.TryCompile(QxType.DateTime, ctx, out date2Sql, out date2Nullity) ||
                        arg2.TryCompile(QxType.DateTimeOffset, ctx, out date2Sql, out date2Nullity)))
                        {
                            throw new QueryException($"Function '{Name}': The second argument {arg2} could not be interpreted as a {QxType.Date}, {QxType.DateTime} or {QxType.DateTimeOffset}.");
                        }

                        string datePart = nameLower[4..^1]; // Remove "diff" and "s"
                        decimal secondsPerUnit = datePart switch
                        {
                            "day" => 60m * 60m * 24m,
                            "hour" => 60m * 60m,
                            "minute" => 60m,
                            "second" => 1m,
                            _ => throw new Exception()
                        };

                        resultType = QxType.Numeric;
                        resultNullity = date1Nullity | date2Nullity;
                        resultSql = $"(DATEDIFF(SECOND, {date1Sql.DeBracket()}, {date2Sql.DeBracket()}) / {secondsPerUnit:F1})";

                        break;
                    }

                case "not": // (condition: boolean) => boolean                    
                    {
                        int expectedArgCount = 1;
                        if (Arguments.Length != expectedArgCount)
                        {
                            throw new QueryException($"Function '{Name}' accepts exactly {expectedArgCount} argument(s).");
                        }

                        var arg1 = Arguments[0];
                        if (arg1.TryCompile(QxType.Boolean, ctx, out string operandSql, out QxNullity operandNullity))
                        {
                            resultType = QxType.Boolean;
                            resultNullity = operandNullity;
                            if (resultNullity != QxNullity.NotNull)
                            {
                                // Developer mistake
                                throw new InvalidOperationException($"[Bug] A nullable boolean expression: {this}");
                            }

                            if (operandSql == FALSE)
                            {
                                resultSql = TRUE;
                            }
                            else if (operandSql == TRUE)
                            {
                                resultSql = FALSE;
                            }
                            else
                            {
                                resultSql = $"(NOT {operandSql})";
                            }

                            break;
                        }
                        else
                        {
                            throw new QueryException($"Function '{Name}': The argument {arg1} could not be interpreted as a {QxType.Boolean}.");
                        }
                    }

                case "abs": // (value: numeric) => numeric
                    {
                        int expectedArgCount = 1;
                        if (Arguments.Length != expectedArgCount)
                        {
                            throw new QueryException($"Function '{Name}' accepts exactly {expectedArgCount} argument(s).");
                        }

                        var arg1 = Arguments[0];
                        if (!arg1.TryCompile(QxType.Numeric, ctx, out string operandSql, out QxNullity operandNullity))
                        {
                            throw new QueryException($"Function '{Name}': The argument {arg1} could not be interpreted as a {QxType.Numeric}.");
                        }

                        resultType = QxType.Numeric;
                        resultNullity = operandNullity;
                        resultSql = $"ABS({operandSql.DeBracket()})"; // -ve sign

                        break;
                    }

                case "if": // (condition: boolean, value_if_true: X, value_if_false: X) => X
                    {
                        var (conditionSql, arg2, arg3) = IfParameters(ctx);

                        // Complie natively
                        var (ifTrueSql, ifTrueType, ifTrueNullity) = arg2.CompileNative(ctx);
                        if (ifTrueType == QxType.Boolean)
                        {
                            throw new QueryException($"Function '{Name}': The second argument {arg2} cannot be a {QxType.Boolean} expression.");
                        }

                        var (ifFalseSql, ifFalseType, ifFalseNullity) = arg3.CompileNative(ctx);
                        if (ifFalseType == QxType.Boolean)
                        {
                            throw new QueryException($"Function '{Name}': The third argument {arg3} cannot be a {QxType.Boolean} expression.");
                        }

                        if ((ifTrueType == ifFalseType) ||
                            (ifTrueType > ifFalseType && arg2.TryCompile(ifFalseType, ctx, out ifTrueSql, out ifTrueNullity)) ||
                            (ifFalseType > ifTrueType && arg3.TryCompile(ifTrueType, ctx, out ifFalseSql, out ifFalseNullity)))
                        {
                            // Calculate result type, SQL and nullity
                            resultType = ifTrueType > ifFalseType ? ifFalseType : ifTrueType;
                            (resultSql, resultNullity) = IfCompile(conditionSql, ifTrueSql, ifTrueNullity, ifFalseSql, ifFalseNullity);
                            break;
                        }
                        else
                        {
                            throw new QueryException($"Function '{Name}' cannot be used on expressions {arg2} and {arg3} because they have incompatible data types.");
                        }
                    }

                case "isnull": // (value: X, fallback_value: X) => X
                    {
                        var (exp, replacement) = IsNullParameters();

                        var (expSql, expType, expNullity) = exp.CompileNative(ctx);
                        if (expType == QxType.Boolean)
                        {
                            throw new QueryException($"Function '{Name}': The first argument {exp} cannot be a {QxType.Boolean} expression.");
                        }

                        var (replacementSql, replacementType, replacementNullity) = replacement.CompileNative(ctx);
                        if (replacementType == QxType.Boolean)
                        {
                            throw new QueryException($"Function '{Name}': The second argument {replacement} cannot be a {QxType.Boolean} expression.");
                        }

                        // Calculate the native type
                        if ((expType == replacementType) ||
                            (expType > replacementType && exp.TryCompile(replacementType, ctx, out expSql, out expNullity)) ||
                            (replacementType > expType && replacement.TryCompile(expType, ctx, out replacementSql, out replacementNullity)))
                        {
                            // Calculate result type, SQL and nullity
                            resultType = expType > replacementType ? replacementType : expType;
                            (resultSql, resultNullity) = IsNullCompile(expSql, expNullity, replacementSql, replacementNullity);
                            break;
                        }
                        else
                        {
                            throw new QueryException($"Function '{Name}' cannot be used on expressions {exp} and {replacement} because they have incompatible data types.");
                        }
                    }

                case "today": // () => Date | DateTime
                    {
                        resultType = QxType.Date;
                        (resultSql, resultNullity) = CompileToday(ctx, resultType);
                        break;
                    }

                case "now": // () => DateTimeOffset
                    {
                        if (Arguments.Length > 0)
                        {
                            throw new QueryException($"Function '{Name}' does not accept any arguments.");
                        }

                        resultType = QxType.DateTimeOffset;
                        resultNullity = QxNullity.NotNull;

                        string varDef = $"N'{ctx.Now:o}'";
                        string varName = ctx.Variables.AddVariable("DATETIMEOFFSET(7)", varDef);
                        resultSql = $"@{varName}";
                        break;
                    }

                case "me": // () => numeric
                    {
                        if (Arguments.Length > 0)
                        {
                            throw new QueryException($"Function '{Name}' does not accept any arguments.");
                        }

                        resultType = QxType.Numeric;
                        if (ctx.UserId != 0)
                        {
                            resultNullity = QxNullity.NotNull;
                            resultSql = ctx.UserId.ToString();
                        }
                        else
                        {
                            resultNullity = QxNullity.Null;
                            resultSql = null; // Handled later
                        }

                        break;
                    }

                default:
                    {
                        throw new QueryException($"Unknown function '{Name}'.");
                    }
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
            return exp is QueryexFunction func &&
                StringComparer.OrdinalIgnoreCase.Equals(func.Name, Name) &&
                func.Arguments.Length == Arguments.Length &&
                Enumerable.Range(0, Arguments.Length)
                    .All(i => func.Arguments[i].Equals(Arguments[i]));
        }

        public override int GetHashCode()
        {
            var nameCode = StringComparer.OrdinalIgnoreCase.GetHashCode(Name);
            var argsCode = Arguments
                .Select(arg => arg.GetHashCode())
                .Aggregate(0, (code1, code2) => code1 ^ code2);

            return nameCode ^ argsCode;
        }

        public override QueryexBase Clone(string[] prefix = null) => new QueryexFunction(Name, Arguments.Select(e => e.Clone(prefix)).ToArray());

        #region Helper Functions

        private (QueryexBase exp, string conditionSql) AggregationParameters(QxCompilationContext ctx)
        {
            if (Arguments.Length < 1 || Arguments.Length > 2)
            {
                throw new QueryException($"No overload for function '{Name}' accepts {Arguments.Length} arguments.");
            }

            var arg1 = Arguments[0];

            string conditionSql = null;
            if (Arguments.Length >= 2)
            {
                var arg2 = Arguments[1];
                if (arg2.TryCompile(QxType.Boolean, ctx, out conditionSql, out QxNullity conditionNullity))
                {
                    if (conditionNullity != QxNullity.NotNull)
                    {
                        // Developer mistake
                        throw new InvalidOperationException($"[Bug] nullable boolean expression {this}.");
                    }
                }
                else
                {
                    throw new QueryException($"Function '{Name}': The second argument {arg2} could not be interpreted as a {QxType.Boolean}.");
                }
            }

            return (arg1, conditionSql);
        }

        private string AggregationCompile(string expSql, string conditionSql)
        {
            return conditionSql == null ?
                $"{Name.ToUpper()}({expSql.DeBracket()})" :
                $"{Name.ToUpper()}(IIF({conditionSql.DeBracket()}, {expSql.DeBracket()}, NULL))";
        }

        private (string conditionSql, QueryexBase valueIfTrue, QueryexBase valueIfFalse) IfParameters(QxCompilationContext ctx)
        {
            int expectedArgCount = 3;
            if (Arguments.Length != expectedArgCount)
            {
                throw new QueryException($"Function '{Name}' accepts exactly {expectedArgCount} argument(s).");
            }

            var arg1 = Arguments[0];
            if (!arg1.TryCompile(QxType.Boolean, ctx, out string conditionSql, out QxNullity conditionNullity))
            {
                throw new QueryException($"Function '{Name}': The first argument {arg1} could not be interpreted as a {QxType.Boolean}.");
            }

            if (conditionNullity != QxNullity.NotNull)
            {
                // Developer mistake
                throw new InvalidOperationException($"[Bug] Nullable boolean expression {this}.");
            }

            var arg2 = Arguments[1];
            var arg3 = Arguments[2];

            return (conditionSql, arg2, arg3);
        }

        private static (string sql, QxNullity nullity) IfCompile(string conditionSql, string ifTrueSql, QxNullity ifTrueNullity, string ifFalseSql, QxNullity ifFalseNullity)
        {
            string resultSql;
            QxNullity resultNullity;

            if (conditionSql == TRUE)
            {
                resultNullity = ifTrueNullity;
                resultSql = ifTrueSql;
            }
            else if (conditionSql == FALSE)
            {
                resultNullity = ifFalseNullity;
                resultSql = ifFalseSql;
            }
            else
            {
                if (ifTrueNullity == QxNullity.NotNull && ifFalseNullity == QxNullity.NotNull)
                {
                    resultNullity = QxNullity.NotNull;
                }
                else if (ifTrueNullity == QxNullity.Null && ifFalseNullity == QxNullity.Null)
                {
                    resultNullity = QxNullity.Null;
                }
                else
                {
                    resultNullity = QxNullity.Nullable;
                }

                resultSql = $"IIF({conditionSql.DeBracket()}, {ifTrueSql.DeBracket()}, {ifFalseSql.DeBracket()})";
            }

            return (resultSql, resultNullity);
        }

        private (QueryexBase exp, QueryexBase replacement) IsNullParameters()
        {
            int expectedArgCount = 2;
            if (Arguments.Length != expectedArgCount)
            {
                throw new QueryException($"Function '{Name}' accepts exactly {expectedArgCount} argument(s).");
            }

            var arg1 = Arguments[0];
            var arg2 = Arguments[1];

            return (arg1, arg2);
        }

        private static (string sql, QxNullity nullity) IsNullCompile(string expSql, QxNullity expNullity, string replacementSql, QxNullity replacementNullity)
        {
            string resultSql;
            QxNullity resultNullity;

            resultNullity = expNullity & replacementNullity;
            resultSql = $"ISNULL({expSql.DeBracket()}, {replacementSql.DeBracket()})";

            return (resultSql, resultNullity);
        }

        private (string numberSql, QueryexBase dateExp) AddDatePartParameters(QxCompilationContext ctx, string nameLower)
        {
            int expectedArgCount = 2;
            if (Arguments.Length != expectedArgCount)
            {
                throw new QueryException($"Function '{Name}' accepts exactly {expectedArgCount} argument(s).");
            }

            string datePart = nameLower[3..^1]; // Remove "add" and "s"

            // Argument #1 Number
            var arg1 = Arguments[0];
            if (arg1.TryCompile(QxType.Numeric, ctx, out string numberSql, out QxNullity numberNullity))
            {
                if (numberNullity != QxNullity.NotNull)
                {
                    throw new QueryException($"Function '{Name}': The first argument {arg1} cannot be a nullable expression.");
                }
            }
            else
            {
                throw new QueryException($"Function '{Name}': The first argument {arg1} could not be interpreted as a {QxType.Numeric}.");
            }

            var dateExp = Arguments[1];
            return (numberSql, dateExp);
        }

        private static (string sql, QxNullity nullity) AddDatePartCompile(string nameLower, string numberSql, string dateSql, QxNullity dateNullity)
        {
            string datePart = nameLower[3..^1]; // Remove "add" and "s"
            QxNullity resultNullity = dateNullity;
            string resultSql = $"DATEADD({datePart.ToUpper()}, {numberSql.DeBracket()}, {dateSql.DeBracket()})"; // Use SQL's built in function

            return (resultSql, resultNullity);
        }

        private (string sql, QxNullity nullity) CompileToday(QxCompilationContext ctx, QxType type)
        {
            if (Arguments.Length > 0)
            {
                throw new QueryException($"Function '{Name}' does not accept any arguments.");
            }

            string resultSql;
            switch (type)
            {
                case QxType.Date:
                    {
                        string varDef = $"N'{ctx.Today:yyyy-MM-dd}'";
                        string varName = ctx.Variables.AddVariable("DATE", varDef);
                        resultSql = $"@{varName}";
                        break;
                    }
                case QxType.DateTime:
                    {
                        string varDef = $"N'{ctx.Today:yyyy-MM-ddTHH:mm:ss.ff}'";
                        string varName = ctx.Variables.AddVariable("DATETIME2(2)", varDef);
                        resultSql = $"@{varName}";
                        break;
                    }
                default:
                    throw new InvalidOperationException($"Bug: Calling {nameof(CompileToday)} on an invalidtype {type}.");
            }

            return (resultSql, QxNullity.NotNull);
        }

        #endregion

        #region Function Name Validation

        /// <summary>
        /// First character of a function name must be a letter.
        /// </summary>
        /// <param name="token">The token to test</param>
        /// <returns>True if the first character of the function name is valid according to the condition above, false otherwise</returns>
        private static bool ProperFirstChar(string token)
        {
            return !string.IsNullOrEmpty(token) && char.IsLetter(token[0]);
        }

        /// <summary>
        /// All characters of a function name must be letters, numbers, underscores or dots.
        /// </summary>
        /// <param name="token">The token to test</param>
        /// <returns>True if the characters the function name are valid according to the condition above, false otherwise</returns>
        private static bool ProperChars(string token)
        {
            return !string.IsNullOrEmpty(token) &&
                token.All(c => char.IsLetterOrDigit(c) || c == '_');
        }

        /// <summary>
        /// The function name must not be one of the reserved keywords
        /// </summary>
        /// <param name="token">The token to test</param>
        /// <returns>False if the function name is one of the keywords, true otherwise </returns>
        private static bool NotReservedKeyword(string token)
        {
            return token.ToLower() switch
            {
                "null" or "true" or "false" or "asc" or "desc" => false,
                _ => true,
            };
        }

        /// <summary>
        /// Validates the function's name against all the rules: <see cref="ProperFirstChar(string)"/>,
        /// <see cref="ProperChars(string)"/> and <see cref="NotReservedKeyword(string)"/>
        /// </summary>
        /// <param name="token">The token to test</param>
        /// <returns>True if it passes all the validation rules, false otherwise</returns>
        public static bool IsValidFunctionName(string token)
        {
            return ProperFirstChar(token) && ProperChars(token) && NotReservedKeyword(token);
        }

        #endregion
    }
}
