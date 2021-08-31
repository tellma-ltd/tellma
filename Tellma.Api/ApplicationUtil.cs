using Microsoft.Extensions.Localization;
using System.Text.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Tellma.Api.Dto;
using Tellma.Api.Metadata;
using Tellma.Model.Application;
using System.Text.Json.Serialization;

namespace Tellma.Api.Behaviors
{
    public static class ApplicationUtil
    {
        /// <summary>
        /// Normalizes and standardizes the control options JSON, removing any unknown properties.
        /// </summary>
        public static string PreprocessControlOptions(string control, string optionsJson, SettingsForClient settings)
        {
            if (string.IsNullOrWhiteSpace(optionsJson) || optionsJson == "{}")
            {
                return null; // Nothing to preprocess
            }

            string result = null;
            try
            {
                switch (control)
                {
                    case "text":
                    case "date":
                    case "datetime":
                    case "boolean":
                    case null:
                        return null;

                    case "serial":
                        {
                            var options = JsonSerializer.Deserialize<SerialControlOptions>(optionsJson, _serializerOptions);
                            result = JsonSerializer.Serialize(options, _serializerOptions);
                            break;
                        }
                    case "choice":
                        {
                            var options = JsonSerializer.Deserialize<ChoiceControlOptions>(optionsJson, _serializerOptions);
                            if (options.choices != null)
                            {
                                if (!options.choices.Any())
                                {
                                    // Delete choices if empty
                                    options.choices = null;
                                }
                                else
                                {
                                    // Delete name3 if no secondary language
                                    if (settings.SecondaryLanguageId == null)
                                    {
                                        options.choices.ForEach(e => e.name2 = null);
                                    }

                                    // Delete name3 if no ternary language
                                    if (settings.TernaryLanguageId == null)
                                    {
                                        options.choices.ForEach(e => e.name3 = null);
                                    }
                                }
                            }

                            result = JsonSerializer.Serialize(options, _serializerOptions);
                            break;
                        }
                    case "number":
                        {
                            var options = JsonSerializer.Deserialize<NumberControlOptions>(optionsJson, _serializerOptions);
                            result = JsonSerializer.Serialize(options, _serializerOptions);
                            break;
                        }
                    case "percent":
                        {
                            var options = JsonSerializer.Deserialize<PercentControlOptions>(optionsJson, _serializerOptions);
                            result = JsonSerializer.Serialize(options, _serializerOptions);
                            break;
                        }
                    default:
                        {
                            var options = JsonSerializer.Deserialize<NavigationControlOptions>(optionsJson, _serializerOptions);
                            result = JsonSerializer.Serialize(options, _serializerOptions);
                            break;
                        }
                }
            }
            catch (Exception)
            {
                return null;
            }

            if (result == "{}")
            {
                result = null;
            }

            return result;
        }

        /// <summary>
        /// Validates the control options JSON, returning any errors as localized messages.
        /// </summary>
        public static IEnumerable<string> ValidateControlOptions(string control, string controlOptions, IStringLocalizer localizer, SettingsForClient settings, DefinitionsForClient defs)
        {
            var errors = new List<string>();
            if (string.IsNullOrWhiteSpace(controlOptions))
            {
                return errors; // Nothing to validate
            }

            try
            {
                switch (control)
                {
                    case "text":
                    case "date":
                    case "datetime":
                    case "boolean":
                    case null:
                        break;

                    case "serial":
                        {
                            var options = JsonSerializer.Deserialize<SerialControlOptions>(controlOptions, _serializerOptions);

                            if ((options.codeWidth ?? 4) < 0)
                            {
                                string label = localizer["ControlOptions_codeWidth"];
                                errors.Add(localizer[ErrorMessages.Error_0MustBeGreaterOrEqualZero, label]);
                            }

                            const int max = 9; // More than that won't fit in INT datatype
                            if ((options.codeWidth ?? 4) > max)
                            {
                                string label = localizer["ControlOptions_codeWidth"];
                                errors.Add($"Field {label} cannot be larger than {max}.");
                            }
                        }
                        break;
                    case "choice":
                        {
                            var options = JsonSerializer.Deserialize<ChoiceControlOptions>(controlOptions, _serializerOptions);
                            if (options.choices != null)
                            {
                                if (options.choices.Any(e => string.IsNullOrWhiteSpace(e.value)))
                                {
                                    errors.Add(localizer[ErrorMessages.Error_Field0IsRequired, localizer["ControlOptions_value"]]);
                                }

                                if (options.choices.Any(e => string.IsNullOrWhiteSpace(e.name)))
                                {
                                    string label = localizer["Name"];
                                    if (settings.SecondaryLanguageId != null || settings.TernaryLanguageId != null)
                                    {
                                        label += $" ({settings.PrimaryLanguageSymbol})";
                                    }

                                    errors.Add(localizer[ErrorMessages.Error_Field0IsRequired, label]);
                                }
                            }
                        }
                        break;
                    case "number":
                        {
                            var options = JsonSerializer.Deserialize<NumberControlOptions>(controlOptions, _serializerOptions);
                            if ((options.minDecimalPlaces ?? 0) < 0)
                            {
                                string label = localizer["ControlOptions_minDecimalPlaces"];
                                errors.Add(localizer[ErrorMessages.Error_0MustBeGreaterOrEqualZero, label]);
                            }
                            if ((options.maxDecimalPlaces ?? 0) < 0)
                            {
                                string label = localizer["ControlOptions_maxDecimalPlaces"];
                                errors.Add(localizer[ErrorMessages.Error_0MustBeGreaterOrEqualZero, label]);
                            }
                            if ((options.minDecimalPlaces ?? 0) > (options.maxDecimalPlaces ?? 4))
                            {
                                string minLabel = localizer["ControlOptions_minDecimalPlaces"];
                                string maxLabel = localizer["ControlOptions_maxDecimalPlaces"];
                                errors.Add($"Field {minLabel} cannot be larger {maxLabel}.");
                            }
                        }
                        break;
                    case "percent":
                        {
                            var options = JsonSerializer.Deserialize<PercentControlOptions>(controlOptions, _serializerOptions);
                            if ((options.minDecimalPlaces ?? 0) < 0)
                            {
                                string label = localizer["ControlOptions_minDecimalPlaces"];
                                errors.Add(localizer[ErrorMessages.Error_0MustBeGreaterOrEqualZero, label]);
                            }
                            if ((options.maxDecimalPlaces ?? 0) < 0)
                            {
                                string label = localizer["ControlOptions_maxDecimalPlaces"];
                                errors.Add(localizer[ErrorMessages.Error_0MustBeGreaterOrEqualZero, label]);
                            }
                            if ((options.minDecimalPlaces ?? 0) > (options.maxDecimalPlaces ?? 4))
                            {
                                string minLabel = localizer["ControlOptions_minDecimalPlaces"];
                                string maxLabel = localizer["ControlOptions_maxDecimalPlaces"];
                                errors.Add($"Field {minLabel} cannot be larger {maxLabel}.");
                            }
                        }
                        break;
                    default:
                        {
                            const string invalidDefError = "Invalid definition.";
                            var options = JsonSerializer.Deserialize<NavigationControlOptions>(controlOptions, _serializerOptions);
                            if (options.definitionId != null)
                            {
                                var definitionId = options.definitionId.Value;
                                switch (control)
                                {
                                    case nameof(Document):
                                        if (!defs.Documents.ContainsKey(definitionId))
                                        {
                                            errors.Add(invalidDefError);
                                        }

                                        break;
                                    case nameof(Agent):
                                        if (!defs.Agents.ContainsKey(definitionId))
                                        {
                                            errors.Add(invalidDefError);
                                        }

                                        break;
                                    case nameof(Resource):
                                        if (!defs.Resources.ContainsKey(definitionId))
                                        {
                                            errors.Add(invalidDefError);
                                        }

                                        break;
                                    case nameof(Lookup):
                                        if (!defs.Lookups.ContainsKey(definitionId))
                                        {
                                            errors.Add(invalidDefError);
                                        }

                                        break;
                                }
                            }
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                errors.Add($"Error parsing {nameof(controlOptions)}: {ex.Message}");
            }

            return errors;
        }
       
        private static readonly JsonSerializerOptions _serializerOptions = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        #region Types

        private class SerialControlOptions
        {
            public string prefix { get; set; }
            public int? codeWidth { get; set; }
        }

        private class ChoiceControlOptions
        {
            public List<ChoiceControlOptionsChoice> choices { get; set; }

            public class ChoiceControlOptionsChoice
            {
                public string value { get; set; }
                public string name { get; set; }
                public string name2 { get; set; }
                public string name3 { get; set; }
            }
        }

        private class NumberControlOptions
        {
            public int? minDecimalPlaces { get; set; }
            public int? maxDecimalPlaces { get; set; }
            public string alignment { get; set; }
        }

        private class PercentControlOptions
        {
            public int? minDecimalPlaces { get; set; }
            public int? maxDecimalPlaces { get; set; }
            public string alignment { get; set; }
        }

        private class NavigationControlOptions
        {
            public string filter { get; set; }
            public int? definitionId { get; set; }
        }

        #endregion
    }
}
