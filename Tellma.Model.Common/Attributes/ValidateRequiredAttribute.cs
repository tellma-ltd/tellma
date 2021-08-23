using System;
using System.ComponentModel.DataAnnotations;

namespace Tellma.Model.Common
{
    [AttributeUsage(validOn: AttributeTargets.Property)]
    public class ValidateRequiredAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            // Null violates the validation
            if (value == null)
            {
                return false;
            }

            // Empty or whitespace strings also violate the validation
            if (value is string stringValue)
            {
                return stringValue.Trim().Length != 0;
            }

            return true;
        }
    }
}
