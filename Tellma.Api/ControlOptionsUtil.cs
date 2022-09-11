using Microsoft.Extensions.Localization;
using System.Text.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Tellma.Api.Dto;
using Tellma.Api.Metadata;
using Tellma.Model.Application;
using System.Text.Json.Serialization;

namespace Tellma.Api
{
    public static class ControlOptionsUtil
    {
        private static readonly JsonSerializerOptions _serializerOptions = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        /// <summary>
        /// Normalizes and standardizes the control options JSON, removing any unknown properties.
        /// </summary>
        public static string PreprocessControlOptions(string control, string optionsJson, SettingsForClient settings)
        {
            // Deserialize
            var opt = Deserialize(control, optionsJson);

            // Preprocess
            if (opt is ChoiceControlOptions options)
            {

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
            }

            // Serialize
            return Serialize(opt);
        }

        /// <summary>
        /// Validates the control options JSON, returning any errors as localized messages.
        /// </summary>
        public static IEnumerable<string> ValidateControlOptions(string control, string controlOptions, IStringLocalizer localizer, SettingsForClient settings, DefinitionsForClient defs)
        {
            var errors = new List<string>();
            switch (Deserialize(control, controlOptions))
            {
                case null:
                    break;
                case SerialControlOptions options:
                    {
                        // var options = JsonSerializer.Deserialize<SerialControlOptions>(controlOptions, _serializerOptions);

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
                case ChoiceControlOptions options:
                    {
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
                case NumberControlOptions options:
                    {
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
                case PercentControlOptions options:
                    {
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
                case NavigationControlOptions options:
                    {
                        const string invalidDefError = "Invalid definition.";
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

            return errors;
        }

        /// <summary>
        /// Deserializes the options json into a <see cref="ControlOptions"/> object.
        /// </summary>
        public static ControlOptions Deserialize(string control, string optionsJson)
        {
            if (string.IsNullOrWhiteSpace(optionsJson) || optionsJson.Trim() == "{}")
            {
                return null; // Nothing to preprocess
            }

            try
            {
                return control switch
                {
                    null => null,
                    "text" => null,
                    "date" => null,
                    "datetime" => null,
                    "boolean" => null,
                    "serial" => JsonSerializer.Deserialize<SerialControlOptions>(optionsJson, _serializerOptions),
                    "choice" => JsonSerializer.Deserialize<ChoiceControlOptions>(optionsJson, _serializerOptions),
                    "number" => JsonSerializer.Deserialize<NumberControlOptions>(optionsJson, _serializerOptions),
                    "percent" => JsonSerializer.Deserialize<PercentControlOptions>(optionsJson, _serializerOptions),
                    _ => JsonSerializer.Deserialize<NavigationControlOptions>(optionsJson, _serializerOptions),
                };
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Serializes the <see cref="ControlOptions"/> object back into JSON.
        /// </summary>
        public static string Serialize(ControlOptions options)
        {
            if (options == null)
            {
                return null;
            }

            string result = JsonSerializer.Serialize(options, _serializerOptions);
            if (string.IsNullOrWhiteSpace(result) || result.Trim() == "{}")
            {
                return null; // Nothing to preprocess
            }

            return result;
        }
    }

    #region Types

    public abstract class ControlOptions
    {
    }

    public class SerialControlOptions : ControlOptions
    {
        public string prefix { get; set; }
        public int? codeWidth { get; set; }
    }

    public class ChoiceControlOptions : ControlOptions
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

    public class NumberControlOptions : ControlOptions
    {
        public int? minDecimalPlaces { get; set; }
        public int? maxDecimalPlaces { get; set; }
        public string alignment { get; set; }
    }

    public class PercentControlOptions : ControlOptions
    {
        public int? minDecimalPlaces { get; set; }
        public int? maxDecimalPlaces { get; set; }
        public string alignment { get; set; }
    }

    public class NavigationControlOptions : ControlOptions
    {
        public string filter { get; set; }
        public int? definitionId { get; set; }
    }

    #endregion

}
