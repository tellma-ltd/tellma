using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tellma.Controllers.Templating
{
    public class ExpressionFunction : ExpressionBase
    {
        public string FunctionName { get; set; } // Literal must be a function

        public ExpressionBase[] Arguments { get; set; }

        public override async IAsyncEnumerable<Path> ComputeSelect(EvaluationContext ctx)
        {
            // (1) The selects from the arguments
            foreach (var arg in Arguments)
            {
                await foreach (var select in arg.ComputeSelect(ctx))
                {
                    yield return select;
                }
            }

            // (2) Any additional selects specific to the function implementation
            if (ctx.TryGetFunction(FunctionName, out TemplateFunction functionEntry))
            {
                await foreach (var select in functionEntry.ComputeAdditionalSelect(Arguments, ctx))
                {
                    yield return select;
                }
            }
        }

        public override async IAsyncEnumerable<Path> ComputePaths(EvaluationContext ctx)
        {
            if (ctx.TryGetFunction(FunctionName, out TemplateFunction functionEntry))
            {
                await foreach (var select in functionEntry.ComputePaths(Arguments, ctx))
                {
                    yield return select;
                }
            }
        }

        public override async Task<object> Evaluate(EvaluationContext ctx)
        {
            if (ctx.TryGetFunction(FunctionName, out TemplateFunction functionEntry))
            {
                var argValues = new object[Arguments.Length];
                for (int i = 0; i < Arguments.Length; i++)
                {
                    argValues[i] = await Arguments[i].Evaluate(ctx);
                }

                return await functionEntry.Evaluate(argValues, ctx);
            }
            else
            {
                throw new TemplateException($"Unknown function: {FunctionName}");
            }
        }

        public override string ToString()
        {
            var args = string.Join(", ", Arguments.Select(a => a.ToString()));
            return $"{FunctionName}({args})";
        }

        public static ExpressionFunction Make(string functionName, ExpressionBase[] args)
        {
            return new ExpressionFunction
            {
                FunctionName = functionName,
                Arguments = args
            };
        }
    }
}
