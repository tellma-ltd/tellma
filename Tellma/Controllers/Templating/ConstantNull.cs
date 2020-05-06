using System.Threading.Tasks;

namespace Tellma.Controllers.Templating
{
    /// <summary>
    /// Represents a constant null
    /// </summary>
    public class ConstantNull : ConstantBase
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
