using System.Collections.Generic;
using System.Threading.Tasks;
using Tellma.Services.Utilities;

namespace Tellma.Controllers.Templating
{
    /// <summary>
    /// Represents a logical negation. E.g. !A. Always evaluated to a boolean value
    /// </summary>
    public class TemplexNegation : TemplexBase
    {
        public TemplexBase Inner { get; set; }

        public override IAsyncEnumerable<Path> ComputeSelect(EvaluationContext ctx)
        {
            return Inner.ComputeSelect(ctx);
        }

        public override IAsyncEnumerable<Path> ComputePaths(EvaluationContext ctx)
        {
            return AsyncUtil.Empty<Path>();
        }

        public override async Task<object> Evaluate(EvaluationContext ctx)
        {
            var inner = await Inner.Evaluate(ctx) ?? false; // Null is treated as false
            if (!(inner is bool boolVal))
            {
                throw new TemplateException($"Operator '!' could not be applied. The expression ({Inner}) does not evaluate to a boolean value");
            }
            else
            {
                return !boolVal;
            }
        }

        public override string ToString()
        {
            return $"NOT({Inner})";
        }

        /// <summary>
        /// Creates a new <see cref="TemplexNegation"/>
        /// </summary>
        public static TemplexNegation Make(TemplexBase inner)
        {
            return new TemplexNegation
            {
                Inner = inner,
            };
        }
    }
}
