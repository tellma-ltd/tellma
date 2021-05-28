using System.Threading.Tasks;

namespace Tellma.Api.Templating
{
    /// <summary>
    /// Represents a constant null.
    /// </summary>
    public class TemplexNull : TemplexConstant
    {
        public override Task<object> Evaluate(EvaluationContext ctx)
        {
            return Task.FromResult<object>(null);
        }

        public override string ToString()
        {
            return "null";
        }
    }
}
