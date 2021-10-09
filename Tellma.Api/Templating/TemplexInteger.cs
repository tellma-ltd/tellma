using System.Linq;
using System.Threading.Tasks;

namespace Tellma.Api.Templating
{
    /// <summary>
    /// Represents a constant integer expression, e.g. 15.
    /// </summary>
    public class TemplexInteger : TemplexConstant
    {
        /// <summary>
        /// The value of the <see cref="TemplexInteger"/> expression.
        /// </summary>
        public int Value { get; set; }

        public override Task<object> Evaluate(EvaluationContext ctx)
        {
            return Task.FromResult<object>(Value);
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        /// <summary>
        /// Validates the token against all the rules for expression integer literals.
        /// </summary>
        /// <param name="token">The token to test.</param>
        /// <param name="intValue">The parsed value as an integer.</param>
        /// <returns>True if the token is a valid integer, false otherwise.</returns>
        public static bool IsValidInteger(string token, out int intValue)
        {
            intValue = 0;
            return token.All(char.IsDigit) && int.TryParse(token, out intValue);
        }
    }
}
