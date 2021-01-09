using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tellma.Services.Utilities;

namespace Tellma.Controllers.Templating
{
    /// <summary>
    /// Represents accessing the value of a variable from the <see cref="EvaluationContext"/>.
    /// Evaluates to the value of that variable from the supplied <see cref="EvaluationContext"/>
    /// </summary>
    public class TemplexVariable : TemplexBase
    {
        public static readonly List<string> _keywords = new List<string> { "null", "true", "false" };

        public string VariableName { get; set; }

        public override IAsyncEnumerable<Path> ComputeSelect(EvaluationContext ctx)
        {
            bool found = ctx.TryGetVariable(VariableName, out TemplateVariable variableEntry);

            return found ?
                variableEntry.ResolveSelect() :
                AsyncUtil.Empty<Path>();
        }

        public override IAsyncEnumerable<Path> ComputePaths(EvaluationContext ctx)
        {
            bool found = ctx.TryGetVariable(VariableName, out TemplateVariable variableEntry);

            return found ?
                variableEntry.ResolvePaths() :
                AsyncUtil.Empty<Path>();
        }

        public override string ToString()
        {
            return VariableName;
        }

        public override async Task<object> Evaluate(EvaluationContext ctx)
        {
            if (ctx.TryGetVariable(VariableName, out TemplateVariable variableEntry))
            {
                return await variableEntry.Evaluate();
            }
            else
            {
                throw new TemplateException($"Unknown variable: {VariableName}");
            }
        }

        #region Name Validation

        /// <summary>
        /// First character of a variable's name must be a letter, an underscore or a dollar sign
        /// </summary>
        /// <param name="varName">The variable name to test</param>
        /// <returns>True if the first character of the variable name is valid according to the condition above, false otherwise</returns>
        public static bool ProperFirstChar(string varName)
        {
            if (varName is null)
            {
                return false;
            }

            var firstChar = varName[0];
            return char.IsLetter(firstChar) || firstChar == '_' || firstChar == '$';
        }

        /// <summary>
        /// All characters of a variable's name must be letters, numbers, underscores or dollar signs
        /// </summary>
        /// <param name="varName">The variable name to test</param>
        /// <returns>True if the characters the variable name are valid according to the condition above, false otherwise</returns>
        public static bool ProperChars(string varName)
        {
            if (varName is null)
            {
                return false;
            }

            return varName.All(c => char.IsDigit(c) || char.IsLetter(c) || c == '_' || c == '$');
        }

        /// <summary>
        /// The variable's name must not be one of the reserved keywords, which are listed in <see cref="_keywords"/>
        /// </summary>
        /// <param name="varName">The variable name to test</param>
        /// <returns>False if the variable name is one of the keywords, true otherwise </returns>
        public static bool NotReservedKeyword(string varName)
        {
            return _keywords.All(kw => kw != varName.ToLower());
        }

        /// <summary>
        /// Validates the variable's name against all the rules: <see cref="ProperFirstChar(string)"/>,
        /// <see cref="ProperChars(string)"/> and <see cref="NotReservedKeyword(string)"/>
        /// </summary>
        /// <param name="varName">The variable name to test</param>
        /// <returns>True if it passes all the validation rules, false otherwise</returns>
        public static bool IsValidVariableName(string varName)
        {
            return ProperFirstChar(varName) && ProperChars(varName) && NotReservedKeyword(varName);
        }
        
        /// <summary>
        /// Validates the variable's name against all the rules: <see cref="ProperFirstChar(string)"/>,
        /// <see cref="ProperChars(string)"/> and <see cref="NotReservedKeyword(string)"/>.
        /// Throws a descriptive exception if it violates any of the rules
        /// </summary>
        /// <param name="varName">The variable name to test</param>
        public static void ValidateIteratorVariableName(string varName)
        {
            if (!ProperFirstChar(varName))
            {
                throw new TemplateException($"Iterator variable name {varName} must begin with a letter, an underscore '_' or a dollar sign '$'");
            }

            if (!ProperChars(varName))
            {
                throw new TemplateException($"Iterator variable name {varName} can only contain letters, numbers, underscores '_' and dollar signs '$'");
            }

            if (!IsValidVariableName(varName))
            {
                throw new TemplateException($"Iterator variable name {varName} is a reserved keyword");
            }
        }

        #endregion
    }
}
