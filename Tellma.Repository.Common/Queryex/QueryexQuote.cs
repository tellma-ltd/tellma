using System;

namespace Tellma.Repository.Common.Queryex
{
    /// <summary>
    /// Represents a literal string inside quotes.
    /// <para/>
    /// Examples:<br/> 
    /// - 'Hi'<br/>
    /// - 'Hello, World.'<br/>
    /// - 'It''s called Tellma ERP'<br/>
    /// </summary>
    public class QueryexQuote : QueryexBase
    {
        private bool _escaped = false;

        public QueryexQuote(string value)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// The parsed string value of the <see cref="QueryexQuote"/>.
        /// </summary>
        public string Value { get; private set; }

        /// <summary>
        /// Escapes special characters % and _ in <see cref="Value"/> such that it can be used as the 2nd operand for an SQL LIKE operator
        /// </summary>
        public void EscapeForLike()
        {
            if (!_escaped)
            {
                _escaped = true;
                Value = Value.Replace("%", "[%]").Replace("_", "[_]");
            }
        }

        public override string ToString()
        {
            return $"'{Value.Replace("'", "''")}'";
        }

        public override bool TryCompile(QxType targetType, QxCompilationContext ctx, out string resultSql, out QxNullity resultNullity)
        {
            switch (targetType)
            {
                case QxType.Date:
                    if (DateTime.TryParse(Value, out DateTime d))
                    {
                        d = d.Date; // Remove the time component
                        string varDef = $"N'{d:yyyy-MM-dd}'";
                        string varName = ctx.Variables.AddVariable("DATE", varDef);
                        resultSql = $"@{varName}";
                        resultNullity = QxNullity.NotNull;
                        return true;
                    }
                    else
                    {
                        resultSql = null;
                        resultNullity = default;
                        return false;
                    }
                case QxType.DateTime:
                    if (DateTime.TryParse(Value, out DateTime dt))
                    {
                        string varDef = $"N'{dt:yyyy-MM-ddTHH:mm:ss.ff}'";
                        string varName = ctx.Variables.AddVariable("DATETIME2(2)", varDef);
                        resultSql = $"@{varName}";
                        resultNullity = QxNullity.NotNull;
                        return true;
                    }
                    else
                    {
                        resultSql = null;
                        resultNullity = default;
                        return false;
                    }
                case QxType.DateTimeOffset:
                    if (DateTimeOffset.TryParse(Value, out DateTimeOffset dto))
                    {
                        string varDef = $"N'{dto:o}'";
                        string varName = ctx.Variables.AddVariable("DATETIMEOFFSET(7)", varDef);
                        resultSql = $"@{varName}";
                        resultNullity = QxNullity.NotNull;
                        return true;
                    }
                    else
                    {
                        resultSql = null;
                        resultNullity = default;
                        return false;
                    }
            }

            return base.TryCompile(targetType, ctx, out resultSql, out resultNullity);
        }

        public override (string, QxType, QxNullity) CompileNative(QxCompilationContext ctx)
        {
            // Strings must be added in parameters to prevent SQL injection vulnerability
            var parameterName = ctx.Parameters.AddParameter(Value);
            var sql = $"@{parameterName}";

            return (sql, QxType.String, QxNullity.NotNull);
        }

        /// <summary>
        /// Validates the token against all the rules for expression quote literals
        /// </summary>
        /// <param name="token">The token to test</param>
        /// <param name="quoteValue">The parsed value as a string</param>
        /// <returns>True if the token is a valid expression number, false otherwise</returns>
        public static bool IsValidQuote(string token, out string quoteValue)
        {
            bool match = token.Length >= 2 && token.StartsWith('\'') && token.EndsWith('\'');
            if (match)
            {
                quoteValue = token[1..^1];
            }
            else
            {
                quoteValue = null;
            }

            return match;
        }

        public override bool Equals(object exp)
        {
            return exp is QueryexQuote quote && Value == quote.Value;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override QueryexBase Clone(string[] prefix = null) => new QueryexQuote(Value);
    }
}
