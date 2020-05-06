using System.Globalization;
using Tellma.Controllers.Dto;
using Tellma.Data;

namespace Tellma.Controllers.Templating
{
    public static class TemplateUtil
    {
        /// <summary>
        /// Returns a function that throws an exception when evaluated indicating that
        /// the values needed to evaluate this function are not loaded yet.
        /// This is used as placeholder for the real function in static evaluation contexts
        /// that are required to execute <see cref="TemplateBase.ComputeSelect(EvaluationContext)"/>
        /// before the API call is made
        /// </summary>
        /// <param name="funcName">The name of the function (used in the error message)</param>
        /// <returns>A <see cref="PureFunction"/> that throws an exception when evaluated</returns>
        public static PureFunction FunctionThatThrows(string funcName)
        {
            return (object[] args, EvaluationContext ctx) => throw new TemplateException($"Attempt to evaluate a function '{funcName}' before it has been loaded");
        }

        /// <summary>
        /// Returns a variable that throws an exception when evaluated indicating that
        /// the values needed to evaluate this variable are not loaded yet.
        /// This is used as placeholder for the real variable in static evaluation contexts
        /// that are required to execute <see cref="TemplateBase.ComputeSelect(EvaluationContext)"/>
        /// before the API call is made
        /// </summary>
        /// <param name="varName">The name of the variable (used in the error message)</param>
        /// <returns>A <see cref="VariableEvaluator"/> that throws an exception when evaluated</returns>
        public static VariableEvaluator VariableThatThrows(string varName)
        {
            return () => throw new TemplateException($"Attempt to evaluate a variable '{varName}' before it has been loaded");
        }

        /// <summary>
        /// Returns the <see cref="CultureInfo"/> based on <see cref="GenerateMarkupArguments"/>.
        /// Throws an exception if not one of the supported cultures in the provided <see cref="TenantInfo"/>
        /// </summary>
        public static CultureInfo GetCulture(GenerateMarkupArguments args, TenantInfo tenantInfo)
        {
            // Some validation
            if (args.Culture != null && tenantInfo.PrimaryLanguageId != args.Culture && tenantInfo.SecondaryLanguageId != args.Culture && tenantInfo.TernaryLanguageId != args.Culture)
            {
                throw new BadRequestException($"Culture {args.Culture} is not supported in this company");
            }

            var culture = CultureInfo.CurrentUICulture;
            if (!string.IsNullOrWhiteSpace(args.Culture))
            {
                try
                {
                    culture = new CultureInfo(args.Culture);
                }
                catch (CultureNotFoundException)
                {
                    throw new BadRequestException($"Culture {args.Culture} does not exist");
                }
            }

            return culture;
        }
    }
}
