﻿using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Tellma.Model.Common
{
    /// <summary>
    /// Indicates that the adorned property must have one of the specified values.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public class ChoiceListAttribute : DataTypeAttribute
    {
        public object[] Choices { get; }
        public string[] DisplayNames { get; }

        public ChoiceListAttribute(object[] choices, string[] displayNames = null) : base("Choice")
        {
            Choices = choices ?? throw new ArgumentNullException(nameof(choices));
            DisplayNames = displayNames ?? choices.Select(e => e.ToString()).ToArray();

            if(Choices.Any(e => string.IsNullOrEmpty(e?.ToString())))
            {
                // Programmer error
                throw new ArgumentException($"One of the choices cannot be blank");
            }

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
            if (!string.IsNullOrEmpty(value?.ToString()) && !Choices.Contains(value))
            {
                // This is a programmer error, no need to localize it
                string concatenatedChoices = string.Join(", ", Choices.Select(e => e.ToString()));
                return new ValidationResult($"Only the following values are allowed: {concatenatedChoices}");
            }

            // All is good
            return ValidationResult.Success;
        }
    }
}
