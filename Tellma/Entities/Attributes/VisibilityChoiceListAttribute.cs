using System;

namespace Tellma.Entities
{
    /// <summary>
    /// Adorns definition visibility properties. E.g. Currency Visibility. Which can take one of three values: "None", "Optional" or "Required"
    /// </summary>
    [AttributeUsage(validOn: AttributeTargets.Property)]
    public class VisibilityChoiceListAttribute : ChoiceListAttribute
    {
        public VisibilityChoiceListAttribute() : base(
            new object[] { "None", "Optional", "Required" }, 
            new string[] { "Visibility_None", "Visibility_Optional", "Visibility_Required" })
        {
        }
    }
}
