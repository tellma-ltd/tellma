using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Tellma.Services.Utilities;

namespace Tellma.Controllers.Templating
{
    /// <summary>
    /// Repeats the template within their scope as many times as there are items in <see cref="ListCandidate"/>, supplying the item
    /// to the <see cref="EvaluationContext"/> of each iteration of the template under the name <see cref="IteratorVariableName"/>.
    /// The following example lists the Memos of all the document lines: {{ *foreach line in doc.Lines }} {{ line.Memo }} {{ *end }}.
    /// In this example the <see cref="ListCandidate"/> is doc.Lines, and the <see cref="IteratorVariableName"/> is line
    /// </summary>
    public class StructureForeach : StructureBase
    {
        /// <summary>
        /// The list to iterate the template over
        /// </summary>
        public ExpressionBase ListCandidate { get; set; }

        /// <summary>
        /// The name of the iterator variable
        /// </summary>
        public string IteratorVariableName { get; set; }

        public override async IAsyncEnumerable<Path> ComputeSelect(EvaluationContext ctx)
        {
            if (ListCandidate != null)
            {
                // Expression select
                var select = ListCandidate.ComputeSelect(ctx);
                await foreach (var atom in select)
                {
                    yield return atom;
                }

                // Expression paths
                var paths = ListCandidate.ComputePaths(ctx);
                await foreach (var path in paths)
                {
                    yield return path.Append("Id");
                }

                // Inner template select
                var scopedCtx = ctx.Clone();
                scopedCtx.SetLocalVariable(IteratorVariableName, new TemplateVariable(
                                eval: TemplateUtil.VariableThatThrows(IteratorVariableName),
                                selectResolver: () => select,
                                pathsResolver: () => paths
                                ));

                await foreach (var atom in Template.ComputeSelect(scopedCtx))
                {
                    yield return atom;
                }
            }
        }

        public override async Task GenerateOutput(StringBuilder builder, EvaluationContext ctx, Func<string, string> encodeFunc = null)
        {
            if (ListCandidate != null)
            {
                var listObj = (await ListCandidate.Evaluate(ctx)) ?? new List<object>();
                if (listObj is IList list)
                {
                    foreach (var listItem in list)
                    {
                        // Initialize new evaluation context with the new variable in it
                        var scopedCtx = ctx.Clone();
                        scopedCtx.SetLocalVariable(IteratorVariableName, new TemplateVariable(
                                evalAsync: () => Task.FromResult(listItem),
                                selectResolver: () => AsyncUtil.Empty<Path>(), // It doesn't matter when generating output
                                pathsResolver: () => AsyncUtil.Empty<Path>() // It doesn't matter when generating output
                                ));

                        // Run the template again on that context
                        await Template.GenerateOutput(builder, scopedCtx, encodeFunc);
                    }
                }
                else
                {
                    throw new TemplateException($"Foreach expression could not be applied. Expression ({ListCandidate}) does not evaluate to a list");
                }
            }
        }
    }
}
