using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tellma.Utilities.Common;

namespace Tellma.Api.Templating
{
    /// <summary>
    /// Represents a function invocation, e.g. Format(expr).
    /// </summary>
    public class TemplexFunction : TemplexBase
    {
        /// <summary>
        /// The name of the function, should be found in the <see cref="EvaluationContext"/> during evaluation.
        /// </summary>
        public string FunctionName { get; set; }

        /// <summary>
        /// The function arguments in order.
        /// </summary>
        public TemplexBase[] Arguments { get; set; }

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
            if (ctx.TryGetFunction(FunctionName, out EvaluationFunction functionEntry))
            {
                await foreach (var select in functionEntry.ComputeAdditionalSelect(Arguments, ctx))
                {
                    yield return select;
                }
            }
        }

        public override async IAsyncEnumerable<Path> ComputePaths(EvaluationContext ctx)
        {
            if (ctx.TryGetFunction(FunctionName, out EvaluationFunction functionEntry))
            {
                await foreach (var select in functionEntry.ComputePaths(Arguments, ctx))
                {
                    yield return select;
                }
            }
        }

        public override async Task<object> Evaluate(EvaluationContext ctx)
        {
            if (ctx.TryGetFunction(FunctionName, out EvaluationFunction functionEntry))
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
                throw new TemplateException($"Unknown function: {FunctionName}.");
            }
        }

        public override string ToString()
        {
            var args = string.Join(", ", Arguments.Select(a => a.ToString().DeBracket()));
            return $"{FunctionName}({args})";
        }

        /// <summary>
        /// Creates a new <see cref="TemplexFunction"/>.
        /// </summary>
        public static TemplexFunction Make(string functionName, TemplexBase[] args)
        {
            return new TemplexFunction
            {
                FunctionName = functionName,
                Arguments = args
            };
        }

        /// <summary>
        /// Validates the function names against all the rules which are 
        /// currently the same rules as variable names.
        /// </summary>
        /// <param name="name">The name to validate</param>
        /// <returns>True if <paramref name="name"/> is a valid function name, False otherwise.</returns>
        public static bool IsValidFunctionName(string name)
            => TemplexVariable.IsValidVariableName(name); // Same rules apply
    }
}
