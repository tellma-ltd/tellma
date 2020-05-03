using System.Threading.Tasks;

namespace Tellma.Controllers.Templating
{
    /// <summary>
    /// Represents a constant boolean expression: true or false
    /// </summary>
    public class ConstantBoolean : ConstantBase
    {
        /// <summary>
        /// The value of the <see cref="ConstantBoolean"/> expression
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
