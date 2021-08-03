using System.Threading.Tasks;

namespace Tellma.Api.Templating
{
    /// <summary>
    /// Represents a constant boolean expression, e.g. true or false.
    /// </summary>
    public class TemplexBoolean : TemplexConstant
    {
        /// <summary>
        /// The value of the <see cref="TemplexBoolean"/> expression.
        /// </summary>
        public bool Value { get; set; }

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
