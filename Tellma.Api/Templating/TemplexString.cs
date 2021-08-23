using System.Threading.Tasks;

namespace Tellma.Api.Templating
{
    /// <summary>
    /// Represents a constant string expression enclosed in single quotation marks, e.g. 'Hello, World'.
    /// </summary>
    public class TemplexString : TemplexConstant
    {
        /// <summary>
        /// The value of the <see cref="TemplexString"/> expression.
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
    }
}
