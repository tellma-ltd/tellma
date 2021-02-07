using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tellma.Controllers.Templating
{
    public class TemplexIndexer : TemplexBase
    {
        public TemplexBase ListCandidate { get; set; } // Must evaluate to a model entity
        public int Index { get; set; }

        public override IAsyncEnumerable<Path> ComputeSelect(EvaluationContext ctx)
        {
            return ListCandidate.ComputeSelect(ctx);
        }

        public override IAsyncEnumerable<Path> ComputePaths(EvaluationContext ctx)
        {
            return ListCandidate.ComputePaths(ctx);
        }

        public override async Task<object> Evaluate(EvaluationContext ctx)
        {
            var listCandidate = await ListCandidate.Evaluate(ctx);
            if (listCandidate == null)
            {
                // Template indexer implements null propagation out of the box
                return null;
            }

            if (listCandidate is IList list)
            {
                if (Index < 0 || Index >= list.Count)
                {
                    // Template indexer implements null propagation out of the box
                    return null;
                }

                return list[Index];
            }
            else
            {
                throw new TemplateException($"Indexer '#{Index}' is only valid on model entity lists");
            }
        }

        public static TemplexIndexer Make(TemplexBase listCandidate, int index)
        {
            return new TemplexIndexer
            {
                ListCandidate = listCandidate,
                Index = index
            };
        }

        public override string ToString()
        {
            return $"{ListCandidate}#{Index}";
        }
    }
}
