using System;
using System.Linq;

namespace Tellma.Controllers.Templating
{
    /// <summary>
    /// Represents a scoping template component that does not generate any output itself, but influences
    /// the behavior of a <see cref="TemplateTree"/> component within its scope.
    /// <see cref="StructureBase"/> starts with a distinguishable asterisk inside double curly brackets.
    /// Examples are {{ *foreach x in expr }}, {{ *if expr }} and {{ *define x as expr }},
    /// The scope of a <see cref="StructureBase"/> is terminated with an {{ *end }} component
    /// </summary>
    public abstract class StructureBase : TemplateBase
    {
        /// <summary>
        /// The scoped template, the behavior of which the <see cref="StructureBase"/> influences
        /// </summary>
        public TemplateTree Template { get; set; }

        public const string _define = "*define";
        public const string _foreach = "*foreach";
        public const string _if = "*if";
        public const string _end = "*end";

        /// <summary>
        /// Parses the given string into one of the implementations of <see cref="StructureBase"/>.
        /// </summary>
        public static StructureBase Parse(string exp)
        {
            if (string.IsNullOrWhiteSpace(exp))
            {
                return null;
            }

            var expLower = exp.ToLower();
            if (expLower.StartsWith(_define + " "))
            {
                var removedDefine = exp[(_define.Length + 1)..];
                var pieces = removedDefine.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (pieces.Length < 3 || pieces[1].ToLower() != "as")
                {
                    throw new TemplateException($"Define expression should take the form: {_define} <var> as <expression>");
                }

                var varName = pieces[0];
                var expression = string.Join(' ', pieces.Skip(2));

                TemplexVariable.ValidateIteratorVariableName(varName);

                return new StructureDefine
                {
                    VariableName = varName,
                    Value = TemplexBase.Parse(expression?.Trim()) ?? throw new TemplateException($"Define expression should take the form: {_define} <var> as <expression>"),
                    Template = null, // Handled by the caller
                };
            }
            else if (expLower.StartsWith(_if + " "))
            {
                var removedIf = exp[(_if.Length + 1)..];
                return new StructureIf
                {
                    ConditionCandidate = TemplexBase.Parse(removedIf?.Trim()) ?? throw new TemplateException($"If expression should take the form: {_if} <expression>"),
                    Template = null, // Handled by the caller
                };
            }
            else if (expLower.StartsWith(_foreach + " "))
            {
                var removedForeach = exp[(_foreach.Length + 1)..];
                var pieces = removedForeach.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (pieces.Length < 3 || pieces[1].ToLower() != "in")
                {
                    throw new TemplateException($"Foreach expression should take the form: {_foreach} <var> in <expression>");
                }

                var varName = pieces[0];
                var expression = string.Join(' ', pieces.Skip(2));

                // This throws exceptions if the variable name violates any rules 
                TemplexVariable.ValidateIteratorVariableName(varName);

                return new StructureForeach
                {
                    IteratorVariableName = varName,
                    ListCandidate = TemplexBase.Parse(expression?.Trim()) ?? throw new TemplateException($"Foreach expression should take the form: {_foreach} <var> in <expression>"),
                    Template = null, // Handled by the caller
                };
            }
            else
            {
                throw new TemplateException($"Unrecognized structural expression {exp}, allowed structural expressions are {_define}, {_foreach} and {_if}");
            }
        }
    }
}
