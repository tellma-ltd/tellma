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
    }
}
