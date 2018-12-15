using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.Localization;
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

        public ChoiceListAttribute(object[] choices, string[] displayNames = null)
        {
            Choices = choices ?? throw new ArgumentNullException(nameof(choices));
            DisplayNames = displayNames ?? choices.Select(e => e.ToString()).ToArray();

            if(Choices.Length != DisplayNames.Length)
            {
                // Programmer error
                throw new ArgumentException($"There are {Choices.Length} choices and {DisplayNames.Length} display names");
            }

            if(Choices.Length == 0)
            {
                // Programmer error
                throw new ArgumentException("At least one choice is required");
            }
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            // If it doesn't match any of the choices => error
            if (value != null && !Choices.Contains(value))
            {
                string concatenatedChoices = string.Join(", ", Choices.Select(e => e.ToString()));

                // This is a programmer error, no need to localize it
                return new ValidationResult($"Only the following values are allowed: {concatenatedChoices}");
            }

            // All is good
            return ValidationResult.Success;
        }
    }
}
