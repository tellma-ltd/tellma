using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Controllers.Shared
{
    /// <summary>
    /// Custom validation attribute, checks whether the set value is one
    /// of a specified list of choices
    /// </summary>
    public class ChoiceListAttribute : ValidationAttribute
    {
        private readonly object[] _choices;

        public ChoiceListAttribute(params object[] choices)
        {
            _choices = choices;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            // If it doesn't match any of the choices => error
            if (value != null && !_choices.Contains(value))
            {
                string concatenatedChoices = string.Join(", ", _choices.Select(e => e.ToString()));
                return new ValidationResult($"Only the following values are allowed: {concatenatedChoices}");
            }

            // All is good
            return ValidationResult.Success;
        }
    }
}
