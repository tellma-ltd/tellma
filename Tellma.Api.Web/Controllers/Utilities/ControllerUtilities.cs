using GeoJSON.Net;
using GeoJSON.Net.Contrib.Wkb;
using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Tellma.Api.Dto;
using Tellma.Controllers.ImportExport;
using Tellma.Data;
using Tellma.Model.Application;
using Tellma.Model.Common;
using Tellma.Services.Utilities;

namespace Tellma.Controllers.Utilities
{
    public static class ControllerUtilities
    {
        /// <summary>
        /// Calls the provided function and handles the special exceptions by turning them into <see cref="ActionResult"/>s.
        /// Action implementations can then throw these exceptions when there is an error, making the implementation easier
        /// </summary>
        public static async Task<ActionResult<T>> InvokeActionImpl<T>(Func<Task<ActionResult<T>>> func, ILogger logger)
        {
            try
            {
                return await func();
            }
            catch (TaskCanceledException)
            {
                return new OkResult();
            }
            catch (ForbiddenException)
            {
                return new StatusCodeResult(403);
            }
            catch (NotFoundException<int?> ex)
            {
                return new NotFoundObjectResult(ex.Ids);
            }
            catch (NotFoundException<int> ex)
            {
                return new NotFoundObjectResult(ex.Ids);
            }
            catch (NotFoundException<string> ex)
            {
                return new NotFoundObjectResult(ex.Ids);
            }
            catch (MethodNotAllowedException)
            {
                return new StatusCodeResult(405);
            }
            catch (UnprocessableEntityException ex)
            {
                return new UnprocessableEntityObjectResult(ToModelState(ex.ModelState));
            }
            catch (BadRequestException ex)
            {
                return new BadRequestObjectResult(ex.Message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error caught in {nameof(InvokeActionImpl)}<{nameof(T)}>: {ex.Message}");
                return new BadRequestObjectResult(ex.Message);
            }
        }

        /// <summary>
        /// Calls the provided function and handles the special exceptions by turning them into <see cref="ActionResult"/>s.
        /// Action implementations can then throw these exceptions when there is an error, making the implementation easier
        /// </summary>
        public static async Task<ActionResult> InvokeActionImpl(Func<Task<ActionResult>> func, ILogger logger)
        {
            try
            {
                return await func();
            }
            catch (TaskCanceledException)
            {
                return new OkResult();
            }
            catch (ForbiddenException)
            {
                return new StatusCodeResult(403);
            }
            catch (NotFoundException<int?> ex)
            {
                return new NotFoundObjectResult(ex.Ids);
            }
            catch (NotFoundException<int> ex)
            {
                return new NotFoundObjectResult(ex.Ids);
            }
            catch (NotFoundException<string> ex)
            {
                return new NotFoundObjectResult(ex.Ids);
            }
            catch (MethodNotAllowedException)
            {
                return new StatusCodeResult(405);
            }
            catch (UnprocessableEntityException ex)
            {
                return new UnprocessableEntityObjectResult(ToModelState(ex.ModelState));
            }
            catch (BadRequestException ex)
            {
                return new BadRequestObjectResult(ex.Message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error caught in {nameof(InvokeActionImpl)}: {ex.Message}");
                return new BadRequestObjectResult(ex.Message);
            }
        }

        /// <summary>
        /// Transforms a business logic layer <see cref="ValidationErrorsDictionary"/> to the web controller's <see cref="ModelStateDictionary"/>
        /// </summary>
        public static ModelStateDictionary ToModelState(ValidationErrorsDictionary validationErrors)
        {
            var result = new ModelStateDictionary();
            foreach (var (key, errors) in validationErrors.AllErrors)
            {
                foreach (var error in errors)
                {
                    result.AddModelError(key, error);
                }
            }

            return result;
        }

        /// <summary>
        /// If some 2 or more entities have the same Id that isn't 0, an appropriate error is added to the <see cref="ValidationErrorsDictionary"/>
        /// </summary>
        public static void ValidateUniqueIds<TEntity>(List<TEntity> entities, ValidationErrorsDictionary modelState, IStringLocalizer localizer) where TEntity : EntityWithKey
        {
            if (entities is null)
            {
                throw new ArgumentNullException(nameof(entities));
            }

            if (modelState is null)
            {
                throw new ArgumentNullException(nameof(modelState));
            }

            if (localizer is null)
            {
                throw new ArgumentNullException(nameof(localizer));
            }

            // Check that Ids are unique
            var duplicateIds = entities.Where(e => !(e.GetId()?.Equals(0) ?? true)) // takes away the nulls too
                .GroupBy(e => e.GetId())
                .Where(g => g.Count() > 1);

            if (duplicateIds.Any())
            {
                // Hash the entities' indices for performance
                Dictionary<TEntity, int> indices = entities.ToIndexDictionary();

                foreach (var groupWithDuplicateIds in duplicateIds)
                {
                    foreach (var entity in groupWithDuplicateIds)
                    {
                        // This error indicates a bug
                        var index = indices[entity];
                        modelState.AddModelError($"[{index}].Id", localizer["Error_TheEntityWithId0IsSpecifiedMoreThanOnce", entity.GetId()]);
                    }
                }
            }
        }

        /// <summary>
        /// SQL validation may return error message names (for localization) as well as some arguments 
        /// this method parses those arguments into objects based on their prefix for example date:2019-01-13
        /// will be parsed to datetime object suitable for formatting in C# into the error message
        /// </summary>
        public static object[] ToFormatArguments(this ValidationError @this, IStringLocalizer localizer)
        {
            static object Parse(string str, IStringLocalizer localizer)
            {
                // Null returns null
                if (string.IsNullOrWhiteSpace(str))
                {
                    return str;
                }

                // Anything with this prefix is translated
                var translateKey = "localize:";
                if (str.StartsWith(translateKey))
                {
                    str = str.Remove(0, translateKey.Length);
                    return localizer[str];
                }

                return str;
            }

            object[] formatArguments = {
                    Parse(@this.Argument1, localizer),
                    Parse(@this.Argument2, localizer),
                    Parse(@this.Argument3, localizer),
                    Parse(@this.Argument4, localizer),
                    Parse(@this.Argument5, localizer)
                };

            return formatArguments;
        }

        /// <summary>
        /// The method localizes every error in the collection and adds it to the <see cref="ValidationErrorsDictionary"/>
        /// </summary>
        public static void AddLocalizedErrors(this ValidationErrorsDictionary modelState, IEnumerable<ValidationError> errors, IStringLocalizer localizer)
        {
            foreach (var error in errors)
            {
                var formatArguments = error.ToFormatArguments(localizer);

                string key = error.Key;
                string errorMessage = localizer[error.ErrorName, formatArguments];

                modelState.AddModelError(key: key, errorMessage: errorMessage);
            }
        }

        /// <summary>
        /// Returns a very common transaction scope which uses <see cref="TransactionScopeOption.Required"/> by default, 
        /// an isolation level of <see cref="System.Data.IsolationLevel.ReadCommitted"/> by default and
        /// <see cref="TransactionScopeAsyncFlowOption.Enabled"/>. Defaults can be overridden with arguments
        /// </summary>
        public static TransactionScope CreateTransaction(TransactionScopeOption? scopeOption = null, TransactionOptions? options = null)
        {
            return new TransactionScope(
                        scopeOption: scopeOption ?? TransactionScopeOption.Required,
                        transactionOptions: options ?? new TransactionOptions
                        {
                            IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted,
                            Timeout = DefaultTransactionTimeout()
                        },
                        asyncFlowOption: TransactionScopeAsyncFlowOption.Enabled);
        }

        /// <summary>
        /// Returns the universal default timeout of 5 minutes, if every transaction used this method
        /// it makes it easier to change the timeout when necessary
        /// </summary>
        public static TimeSpan DefaultTransactionTimeout()
        {
            return TimeSpan.FromMinutes(5);
        }

        /// <summary>
        /// From an <see cref="Expression"/> that accesses the property (e.g. "e => e.Name"),
        /// it computes another <see cref="Expression"/> that sets the property, (e.g. "(e, v) => e.Name = v")
        /// </summary>
        public static Expression<Action<TClass, TProp>> GetAssigner<TClass, TProp>(Expression<Func<TClass, TProp>> propAccessor)
        {
            var prop = ((MemberExpression)propAccessor.Body).Member;
            var typeParam = Expression.Parameter(typeof(TClass));
            var valueParam = Expression.Parameter(typeof(TProp));

            return Expression.Lambda<Action<TClass, TProp>>(
                Expression.Assign(
                    Expression.MakeMemberAccess(typeParam, prop),
                    valueParam), typeParam, valueParam);
        }

        /// <summary>
        /// Takes a list of <see cref="Entity"/>'s, and for every entity it inspects the navigation properties, if a navigation property
        /// contains an <see cref="Entity"/> with a strong type, it sets that property to null, and moves the strong entity into a separate
        /// "relatedEntities" hash set, this has several advantages:
        /// 1 - JSON.NET will not have to deal with circular references
        /// 2 - Every strong entity is mentioned once in the JSON response (smaller response size)
        /// 3 - It makes it easier for clients to store and track entities in a central workspace
        /// </summary>
        /// <returns>A dictionary mapping every type name to an <see cref="IEnumerable"/> of related entities of that type (excluding the result entities)</returns>
        public static Dictionary<string, IEnumerable<Entity>> FlattenAndTrim<TEntity>(IEnumerable<TEntity> resultEntities, CancellationToken cancellation)
            where TEntity : Entity
        {
            // If the result is empty, nothing to do
            if (resultEntities == null || !resultEntities.Any())
            {
                return new Dictionary<string, IEnumerable<Entity>>();
            }

            var relatedEntities = new HashSet<Entity>();
            var resultHash = resultEntities.ToHashSet();

            void FlattenAndTrimInner(Entity entity, TypeDescriptor typeDesc)
            {
                if (entity.EntityMetadata.FlattenedAndTrimmed)
                {
                    // This has already been flattened and trimed before
                    return;
                }

                // Mark the entity as flattened and trimmed
                entity.EntityMetadata.FlattenedAndTrimmed = true;

                // Recursively go over the nav properties
                foreach (var prop in typeDesc.NavigationProperties)
                {
                    if (prop.GetValue(entity) is Entity relatedEntity)
                    {
                        prop.SetValue(entity, null);

                        if (!resultHash.Contains(relatedEntity))
                        {
                            // Unless it is part of the main result, add it to relatedEntities
                            relatedEntities.Add(relatedEntity);
                        }

                        FlattenAndTrimInner(relatedEntity, prop.TypeDescriptor);
                    }
                }

                // Recursively go over every entity in the nav collection properties
                foreach (var prop in typeDesc.CollectionProperties)
                {
                    var collectionType = prop.CollectionTypeDescriptor;
                    if (prop.GetValue(entity) is IList collection)
                    {
                        foreach (var obj in collection)
                        {
                            if (obj is Entity relatedEntity)
                            {
                                FlattenAndTrimInner(relatedEntity, collectionType);
                            }
                        }
                    }
                }
            }

            // Flatten every entity in the main list
            var typeDesc = TypeDescriptor.Get<TEntity>();
            foreach (var entity in resultEntities)
            {
                if (entity != null)
                {
                    FlattenAndTrimInner(entity, typeDesc);
                    cancellation.ThrowIfCancellationRequested();
                }
            }

            // Return the result
            return relatedEntities
                .GroupBy(e => e.GetType().GetRootType().Name)
                .ToDictionary(g => g.Key, g => g.AsEnumerable());
        }

        /// <summary>
        /// Sets the value of <see cref="ILocationEntityForSave.LocationWkb"/> according to to the value of <see cref="ILocationEntityForSave.LocationJson"/>
        /// </summary>
        public static void SynchronizeWkbWithJson<T>(T entity) where T : EntityWithKey, ILocationEntityForSave
        {
            // Here we convert the GeoJson to Well-Known Binary
            var json = entity.LocationJson;
            if (string.IsNullOrWhiteSpace(json))
            {
                entity.LocationWkb = null;
                return;
            }

            try
            {
                var spy = JsonConvert.DeserializeObject<GeoJsonSpy>(json);
                if (spy.Type == GeoJSONObjectType.Feature)
                {
                    // A simple feature can be turned in to a simple WKB
                    var feature = JsonConvert.DeserializeObject<Feature>(json);

                    var geometry = feature?.Geometry;
                    entity.LocationWkb = geometry?.ToWkb();
                }
                else if (spy.Type == GeoJSONObjectType.FeatureCollection)
                {
                    // A feature collection must be converted to a geometry collection and then turned to WKB
                    var coll = JsonConvert.DeserializeObject<FeatureCollection>(json);
                    var geometries = coll?.Features?.Select(feat => feat.Geometry)?.Where(e => e != null) ?? new List<IGeometryObject>();

                    if (geometries.Count() == 1)
                    {
                        // If it's just a single geometry, no need to wrap it in a geometry collection
                        var geometry = geometries.Single();
                        entity.LocationWkb = geometry?.ToWkb();
                    }
                    else
                    {
                        // If it's zero or multiple geometries, wrap in a geometry collection
                        var geomCollection = new GeometryCollection(geometries);
                        entity.LocationWkb = geomCollection?.ToWkb();
                    }
                }
                else
                {
                    // I don't know what'd be the point of localizing this message
                    throw new InvalidOperationException("Root GeoJSON element must be a feature or a feature collection");
                }
            }
            catch (Exception ex)
            {
                entity.EntityMetadata.LocationJsonParseError = ex.Message;
                return;
            }
        }

        /// <summary>
        /// Takes an XLSX or a CSV stream and unpackages its content into a 2-D table of strings
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="fileName"></param>
        /// <param name="contentType"></param>
        /// <param name="localizer"></param>
        /// <returns></returns>
        public static IEnumerable<string[]> ExtractStringsFromFile(Stream stream, string fileName, string contentType, IStringLocalizer localizer)
        {
            IDataExtractor extracter;
            if (contentType == MimeTypes.Csv || (fileName?.ToLower()?.EndsWith(".csv") ?? false))
            {
                extracter = new CsvExtractor();
            }
            else if (contentType == MimeTypes.Excel || (fileName?.ToLower()?.EndsWith(".xlsx") ?? false))
            {
                extracter = new ExcelExtractor();
            }
            else
            {
                throw new FormatException(localizer["Error_OnlyCsvOrExcelAreSupported"]);
            }

            // Extrat and return
            try
            {
                return extracter.Extract(stream).ToList();
            }
            catch (Exception ex)
            {
                // Report any errors during extraction
                string msg = localizer["Error_FailedToParseFileError0", ex.Message];
                throw new BadRequestException(msg);
            }
        }

        /// <summary>
        /// Returns the E.164 representation of the phone number. Which starts with an optional '+' sign followed by digits only
        /// </summary>
        public static string ToE164(string phoneNumber)
        {
            if (string.IsNullOrEmpty(phoneNumber))
            {
                return null;
            }

            // Normalize the phone number to E.164 format (Optional + sign, followed only by digits)
            var e164 = new System.Text.StringBuilder();

            // Start with a '+' sign if there is one
            if (phoneNumber.StartsWith('+'))
            {
                e164.Append('+');
            }

            // Then append only the digits
            foreach (var digit in phoneNumber.Where(char.IsDigit))
            {
                e164.Append(digit);
            }

            return e164.ToString();
        }

        /// <summary>
        /// Attempts to intelligently guess the content mime type from the file name
        /// </summary>
        public static string ContentType(string fileName)
        {
            var provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(fileName, out string contentType))
            {
                contentType = "application/octet-stream";
            }

            return contentType;
        }

        /// <summary>
        /// Retrieves the collection name from the Entity type
        /// </summary>
        public static string GetCollectionName(Type entityType)
        {
            return entityType.GetRootType().Name;
        }

        #region Control Options

        private static readonly JsonSerializerSettings _serializerSettings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };

        /// <summary>
        /// Normalizes and standardizes the control options JSON, removing any unknown properties.
        /// </summary>
        /// <param name="control"></param>
        /// <param name="controlOptions"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public static string PreprocessControlOptions(string control, string controlOptions, SettingsForClient settings)
        {
            if (string.IsNullOrWhiteSpace(controlOptions) || controlOptions == "{}")
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
                            var options = JsonConvert.DeserializeObject<SerialControlOptions>(controlOptions);
                            result = JsonConvert.SerializeObject(options, _serializerSettings);
                            break;
                        }
                    case "choice":
                        {
                            var options = JsonConvert.DeserializeObject<ChoiceControlOptions>(controlOptions);
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

                            result = JsonConvert.SerializeObject(options, _serializerSettings);
                            break;
                        }
                    case "number":
                        {
                            var options = JsonConvert.DeserializeObject<NumberControlOptions>(controlOptions);
                            result = JsonConvert.SerializeObject(options, _serializerSettings);
                            break;
                        }
                    case "percent":
                        {
                            var options = JsonConvert.DeserializeObject<PercentControlOptions>(controlOptions);
                            result = JsonConvert.SerializeObject(options, _serializerSettings);
                            break;
                        }
                    default:
                        {
                            var options = JsonConvert.DeserializeObject<NavigationControlOptions>(controlOptions);
                            result = JsonConvert.SerializeObject(options, _serializerSettings);
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

        public static IEnumerable<string> ValidateControlOptions(string control, string controlOptions, IStringLocalizer localizer, SettingsForClient settings, DefinitionsForClient defs)
        {
            List<string> errors = new List<string>();
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
                            var options = JsonConvert.DeserializeObject<SerialControlOptions>(controlOptions);

                            if ((options.codeWidth ?? 4) < 0)
                            {
                                string label = localizer["ControlOptions_codeWidth"];
                                errors.Add(localizer[Constants.Error_0MustBeGreaterOrEqualZero, label]);
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
                            var options = JsonConvert.DeserializeObject<ChoiceControlOptions>(controlOptions);
                            if (options.choices != null)
                            {
                                if (options.choices.Any(e => string.IsNullOrWhiteSpace(e.value)))
                                {
                                    errors.Add(localizer[Constants.Error_Field0IsRequired, localizer["ControlOptions_value"]]);
                                }

                                if (options.choices.Any(e => string.IsNullOrWhiteSpace(e.name)))
                                {
                                    string label = localizer["Name"];
                                    if (settings.SecondaryLanguageId != null || settings.TernaryLanguageId != null)
                                    {
                                        label += $" ({settings.PrimaryLanguageSymbol})";
                                    }

                                    errors.Add(localizer[Constants.Error_Field0IsRequired, label]);
                                }
                            }
                        }
                        break;
                    case "number":
                        {
                            var options = JsonConvert.DeserializeObject<NumberControlOptions>(controlOptions);
                            if ((options.minDecimalPlaces ?? 0) < 0)
                            {
                                string label = localizer["ControlOptions_minDecimalPlaces"];
                                errors.Add(localizer[Constants.Error_0MustBeGreaterOrEqualZero, label]);
                            }
                            if ((options.maxDecimalPlaces ?? 0) < 0)
                            {
                                string label = localizer["ControlOptions_maxDecimalPlaces"];
                                errors.Add(localizer[Constants.Error_0MustBeGreaterOrEqualZero, label]);
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
                            var options = JsonConvert.DeserializeObject<PercentControlOptions>(controlOptions);
                            if ((options.minDecimalPlaces ?? 0) < 0)
                            {
                                string label = localizer["ControlOptions_minDecimalPlaces"];
                                errors.Add(localizer[Constants.Error_0MustBeGreaterOrEqualZero, label]);
                            }
                            if ((options.maxDecimalPlaces ?? 0) < 0)
                            {
                                string label = localizer["ControlOptions_maxDecimalPlaces"];
                                errors.Add(localizer[Constants.Error_0MustBeGreaterOrEqualZero, label]);
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
                            var options = JsonConvert.DeserializeObject<NavigationControlOptions>(controlOptions);
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
                                    case nameof(Relation):
                                        if (!defs.Relations.ContainsKey(definitionId))
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

    /// <summary>
    /// Used to peek at the root element of a GeoJson string using JSON.NET
    /// </summary>
    public class GeoJsonSpy : IGeometryObject
    {
        [JsonProperty(PropertyName = "type")]
        public GeoJSONObjectType Type { get; set; }
    }
}
