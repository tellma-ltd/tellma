using System.Linq;
using System.Threading.Tasks;

namespace Tellma.Api.Templating
{
    /// <summary>
    /// Represents a constant decimal expression, e.g. 127.5.
    /// </summary>
    public class TemplexDecimal : TemplexConstant
    {
        /// <summary>
        /// The value of the <see cref="TemplexDecimal"/> expression.
        /// </summary>
        public decimal Value { get; set; }

        public override Task<object> Evaluate(EvaluationContext ctx)
        {
            return Task.FromResult<object>(Value);
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        /// <summary>
        /// Validates the token against all the rules for expression decimal literals.
        /// A token is a decimal if it starts with a digit, ends with a digit, with all
        /// intermediate characters being digits except for a maximum of one decimal point.
        /// </summary>
        /// <param name="token">The token to test.</param>
        /// <param name="decimalValue">The parsed value as a decimal.</param>
        /// <returns>True if the token is a valid decimal, false otherwise.</returns>
        public static bool IsValidDecimal(string token, out decimal decimalValue)
        {
            decimalValue = 0m;
            var pieces = token.Split('.');
            return pieces.Length <= 2 && pieces.All(piece => piece.Length > 0 && piece.All(char.IsDigit)) && decimal.TryParse(token, out decimalValue);

            // return char.IsDigit(token[0]) && char.IsDigit(token[^1]) && token.All(c => char.IsDigit(c) || c == '.') && decimal.TryParse(token, out decimalValue);
        }
    }
}
