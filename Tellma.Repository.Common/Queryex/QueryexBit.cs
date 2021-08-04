using System;

namespace Tellma.Repository.Common.Queryex
{
    /// <summary>
    /// Represents a literal boolean value.
    /// <para/>
    /// Examples:<br/> 
    /// - true<br/>
    /// - false<br/>
    /// </summary>
    public class QueryexBit : QueryexBase
    {
        public QueryexBit(bool value)
        {
            Value = value;
        }

        /// <summary>
        /// The parsed boolean value of the <see cref="QueryexBit"/>.
        /// </summary>
        public bool Value { get; }

        public override string ToString()
        {
            return Value.ToString().ToLower();
        }

        public override (string, QxType, QxNullity) CompileNative(QxCompilationContext ctx)
        {
            string sql = Value ? "CAST(1 AS BIT)" : "CAST(0 AS BIT)";
            return (sql, QxType.Bit, QxNullity.NotNull);
        }

        public override bool Equals(object exp)
        {
            return exp is QueryexBit n && Value == n.Value;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override QueryexBase Clone(string[] prefix = null) => new QueryexBit(Value);
    }
}
