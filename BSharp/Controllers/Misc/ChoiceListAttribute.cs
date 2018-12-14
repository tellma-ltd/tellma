using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Controllers.Misc
{
    /// <summary>
    /// Custom validation attribute, checks whether the value is one
    /// of a specified list of choices
    /// </summary>
    public class ChoiceListAttribute : ValidationAttribute
    {
        public object[] Choices { get; }
        public string[] DisplayNames { get; }

        public ChoiceListAttribute(object[] choices, string[] displayNames)
        {
            Choices = choices ?? throw new ArgumentNullException(nameof(choices));
            DisplayNames = displayNames ?? throw new ArgumentNullException(nameof(displayNames));
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            // If it doesn't match any of the choices => error
            if (value != null && !Choices.Contains(value))
            {
                string concatenatedChoices = string.Join(", ", DisplayNames.Select(e => e.ToString()));
                return new ValidationResult($"Only the following values are allowed: {concatenatedChoices}");
            }

            // All is good
            return ValidationResult.Success;
        }
    }
}
