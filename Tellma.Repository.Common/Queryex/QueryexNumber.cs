using System.Linq;

namespace Tellma.Repository.Common.Queryex
{
    /// <summary>
    /// Represents a literal numeric value.
    /// <para/>
    /// Examples:<br/> 
    /// - 3.14<br/>
    /// - 10<br/>
    /// - 0<br/>
    /// </summary>
    public class QueryexNumber : QueryexBase
    {
        public QueryexNumber(decimal value, int decimals)
        {
            Value = value;
            Decimals = decimals;
        }

        /// <summary>
        /// The parsed decimal value of the <see cref="QueryexNumber"/>.
        /// </summary>
        public decimal Value { get; }

        /// <summary>
        /// The number of decimals in the input string, for example 3.14 contains 2 decimals.
        /// The number of decimals is preserved in the compiled SQL result.
        /// </summary>
        public int Decimals { get; }

        public override string ToString()
        {
            return Value.ToString();
        }

        /// <summary>
        /// Validates the token against all the rules for expression decimal.
        /// </summary>
        /// <param name="token">The token to test.</param>
        /// <param name="decimalValue">The parsed the value as a decimal.</param>
        /// <returns>True if the token is a valid expression number, false otherwise.</returns>
        public static bool IsValidNumber(string token, out decimal decimalValue, out int decimals)
        {
            if (char.IsDigit(token[0]) && char.IsDigit(token[^1]) && token.All(c => char.IsDigit(c) || c == '.'))
            {
                decimalValue = decimal.Parse(token);
                var pieces = token.Split('.');
                decimals = pieces.Length <= 1 ? 0 : pieces[^1].Length;

                return true;
            }
            else
            {
                decimalValue = 0;
                decimals = 0;
                return false;
            }
        }

        public override (string, QxType, QxNullity) CompileNative(QxCompilationContext ctx)
        {
            string format = $"F{Decimals}";
            return (Value.ToString(format), QxType.Numeric, QxNullity.NotNull);
        }

        public override bool Equals(object exp)
        {
            return exp is QueryexNumber n && Value == n.Value;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override QueryexBase Clone(string[] prefix = null) => new QueryexNumber(Value, Decimals);
    }
}
