using System;

namespace Tellma.Model.Common
{
    [AttributeUsage(validOn: AttributeTargets.Property)]
    public class ValidateRequiredAttribute : Attribute
    {
    }
}
