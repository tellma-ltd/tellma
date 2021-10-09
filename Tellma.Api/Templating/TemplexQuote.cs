using System.Threading.Tasks;

namespace Tellma.Api.Templating
{
    /// <summary>
    /// Represents a constant string expression enclosed in single quotation marks, e.g. 'Hello, World'.
    /// </summary>
    public class TemplexQuote : TemplexConstant
    {
        /// <summary>
        /// The value of the <see cref="TemplexQuote"/> expression.
        /// </summary>
        public string Value { get; set; }

        public override Task<object> Evaluate(EvaluationContext ctx)
        {
            return Task.FromResult<object>(Value);
        }

        public override string ToString()
        {
            return $"'{Value}'";
        }

        /// <summary>
        /// Validates the token against all the rules for expression quote literals.
        /// </summary>
        /// <param name="token">The token to test.</param>
        /// <param name="quoteValue">The parsed value as a string.</param>
        /// <returns>True if the token is a valid quote, false otherwise.</returns>
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
    }
}
