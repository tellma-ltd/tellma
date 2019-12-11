using BSharp.Controllers.Dto;
using BSharp.Controllers.Utilities;
using BSharp.Data;
using BSharp.Data.Queries;
using BSharp.Entities;
using BSharp.Services.ImportExport;
using BSharp.Services.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

namespace BSharp.Controllers
{
    /// <summary>
    /// Controllers inheriting from this class allow searching, aggregating and exporting a certain
    /// entity type that inherits from <see cref="EntityWithKey{TKey}"/> using OData-like parameters
    /// and allow selecting a certain record by Id, as well as updating, deleting and importing lists
    /// of that entity
    /// </summary>
    public abstract class CrudControllerBase<TEntityForSave, TEntity, TKey> : FactGetByIdControllerBase<TEntity, TKey>
        where TEntityForSave : EntityWithKey<TKey>, new()
        where TEntity : EntityWithKey<TKey>, new()
    {
        // Private Fields

        private readonly ILogger _logger;
        private readonly IStringLocalizer _localizer;

        // Constructor

        public CrudControllerBase(ILogger logger, IStringLocalizer localizer) : base(logger, localizer)
        {
            _logger = logger;
            _localizer = localizer;
        }

        // HTTP Methods

        [HttpPost]
        public virtual async Task<ActionResult<EntitiesResponse<TEntity>>> Save([FromBody] List<TEntityForSave> entities, [FromQuery] SaveArguments args)
        {
            // Note here we use lists https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1?view=netcore-2.1
            // since the order is semantically relevant for reporting validation errors

            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                var result = await SaveImplAsync(entities, args);
                return Ok(result);
            }, _logger);
        }

        [HttpDelete]
        public virtual async Task<ActionResult> Delete([FromBody] List<TKey> ids)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                await DeleteImplAsync(ids);
                return Ok();
            }, _logger);
        }

        [HttpGet("template")]
        public virtual ActionResult Template([FromQuery] TemplateArguments args)
        {
            try
            {
                var abstractFile = GetImportTemplate();
                return ToFileResult(abstractFile, args.Format);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message} {ex.StackTrace}");
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("import"), RequestSizeLimit(5 * 1024 * 1024)] // 5MB
        public virtual async Task<ActionResult<ImportResult>> Import([FromQuery] ImportArguments args)
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();

            Stopwatch watch2 = new Stopwatch();
            watch2.Start();
            decimal parsingToEntitiesForSave = 0;
            decimal attributeValidationInCSharp = 0;
            decimal validatingAndSaving = 0;

            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                // Parse the file into Entities + map back to row numbers (The way source code is compiled into machine code + symbols file)
                var (entities, rowNumberFromErrorKeyMap) = await ParseImplAsync(args); // This should check for primary code consistency!
                parsingToEntitiesForSave = Math.Round(((decimal)watch2.ElapsedMilliseconds) / 1000, 1);
                watch2.Restart();

                // Validation
                ObjectValidator.Validate(ControllerContext, null, null, entities);
                attributeValidationInCSharp = Math.Round(((decimal)watch2.ElapsedMilliseconds) / 1000, 1);
                watch2.Restart();

                if (!ModelState.IsValid)
                {
                    var mappedModelState = MapModelState(ModelState, rowNumberFromErrorKeyMap);
                    throw new UnprocessableEntityException(mappedModelState);
                }

                // Saving
                try
                {
                    await SaveImplAsync(entities, new SaveArguments { ReturnEntities = false });
                    validatingAndSaving = Math.Round(((decimal)watch2.ElapsedMilliseconds) / 1000, 1);
                    watch2.Stop();
                }
                catch (UnprocessableEntityException ex)
                {
                    var mappedModelState = MapModelState(ex.ModelState, rowNumberFromErrorKeyMap);
                    throw new UnprocessableEntityException(mappedModelState);
                }

                var result = new ImportResult
                {
                    Inserted = entities.Count(e => e.Id?.Equals(default(TKey)) ?? false),
                    Updated = entities.Count(e => !(e.Id?.Equals(default(TKey)) ?? false)),
                };

                // Record the time
                watch.Stop();
                var elapsed = Math.Round(((decimal)watch.ElapsedMilliseconds) / 1000, 1);
                result.Seconds = elapsed;
                result.ParsingToDtosForSave = parsingToEntitiesForSave;
                result.AttributeValidationInCSharp = attributeValidationInCSharp;
                result.ValidatingAndSaving = validatingAndSaving;

                return Ok(result);
            }, _logger);
        }

        [HttpPost("parse"), RequestSizeLimit(5 * 1024 * 1024)] // 5MB
        public virtual async Task<ActionResult<List<TEntityForSave>>> Parse([FromQuery] ParseArguments args)
        {
            // This method doesn't import the file in the DB, it simply parses it to 
            // Entities that are ripe for saving, and returns those Entities to the requester
            // This supports scenarios where only part of the required fields are present
            // in the imported file, or to support previewing the import before committing it
            try
            {
                var file = Request.Form.Files.FirstOrDefault();
                var entities = await ParseImplAsync(args);
                return Ok(entities);
            }
            catch (UnprocessableEntityException ex)
            {
                return UnprocessableEntity(ex.ModelState);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message} {ex.StackTrace}");
                return BadRequest(ex.Message);
            }
        }

        // Abstract and virtual members

        protected virtual async Task<(List<TEntityForSave>, Func<string, int?>)> ParseImplAsync(ParseArguments args)
        {
            var file = Request.Form.Files.FirstOrDefault();
            if (file == null)
            {
                throw new BadRequestException(_localizer["Error_NoFileWasUploaded"]);
            }

            var abstractGrid = FileToAbstractGrid(file, args);
            if (abstractGrid.Count < 2)
            {
                ModelState.AddModelError("", _localizer["Error_EmptyImportFile"]);
                throw new UnprocessableEntityException(ModelState);
            }

            // Change the abstract grid to entities for save, and make sure no errors resulted that weren't thrown
            var (entitiesForSave, keyMap) = await ToEntitiesForSave(abstractGrid, args);
            if (!ModelState.IsValid)
            {
                throw new UnprocessableEntityException(ModelState);
            }

            return (entitiesForSave, keyMap);
        }

        protected virtual AbstractDataGrid FileToAbstractGrid(IFormFile file, ParseArguments args)
        {
            // Determine an appropriate file handler based on the file metadata
            FileHandlerBase handler;
            if (file.ContentType == "text/csv" || file.FileName.EndsWith(".csv"))
            {
                handler = new CsvHandler(_localizer);
            }
            else if (file.ContentType == "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" || file.FileName.EndsWith(".xlsx"))
            {
                handler = new ExcelHandler(_localizer);
            }
            else
            {
                throw new FormatException(_localizer["Error_UnknownFileFormat"]);
            }

            using (var fileStream = file.OpenReadStream())
            {
                // Use the handler to unpack the file into an abstract grid and return it
                AbstractDataGrid abstractGrid = handler.ToAbstractGrid(fileStream);
                return abstractGrid;
            }
        }

        protected Task<(List<TEntityForSave>, Func<string, int?>)> ToEntitiesForSave(AbstractDataGrid grid, ParseArguments args)
        {
            throw new NotImplementedException();
        }

        protected AbstractDataGrid GetImportTemplate()
        {
            throw new NotImplementedException();
        }

        private async Task<List<TEntityForSave>> ApplyUpdatePermissionsMask(List<TEntityForSave> entities)
        {
          //  var entityMasks = GetMasksForSavedEntities(entities);
            var permissions = await UserPermissions(Constants.Update);

            // TODO

            /* 
             * Step 1: Get complete mask for TEntityForSave
             * 
             * 
If there are no permissions: throw forbidden exception
else if ((1) at least one permission is criteria free and mask free, AND (2) all update entities (including nav entities) have a full mask in EntityMetadata) return safely
else {
  Do the magic to determine the mask that each entity is based on 

we need to do 2 things:

for each updated entity (including nav entities), construct the flat Mask in Entity Metadata (only relevant if (2) is false)
for each entity (including nav entity), determine the flat permission Mask applicable, (only relevant if (1) is false) <- throw forbidden exception if any permission has no mask

For every entity intersect the two masks into a MegaMask

Load entities by Ids from the DB,  <- need to know the EntityForSave for every entity ---> or do I??
  For every Update entity, any property that is missing from its MegaMask, copy that property value from the corresponding DB entity
  For every Insert entity, any property that is missing from its MegaMask, set that property to NULL
}

return the entities
             * 
             */

            return await Task.FromResult(entities);
        }

        /// <summary>
        /// For each saved entity, determines the applicable mask.
        /// Verifies that the user has sufficient permissions to update the list of entities provided.
        /// </summary>
        protected virtual async Task<Dictionary<TEntityForSave, MaskTree>> GetMasksForSavedEntities(List<TEntityForSave> entities)
        {
            if(entities == null || !entities.Any())
            {
                return new Dictionary<TEntityForSave, MaskTree>();
            }

            var unrestrictedMask = new MaskTree();
            var permissions = await UserPermissions(Constants.Update);
            if (!permissions.Any())
            {
                // User has no permissions on this table whatsoever; forbid
                throw new ForbiddenException();
            }
            else if (permissions.Any(e => string.IsNullOrWhiteSpace(e.Criteria) && string.IsNullOrWhiteSpace(e.Mask)))
            {
                // User has unfiltered update permission on the table => proceed
                return entities.ToDictionary(e => e, e => unrestrictedMask);
            }
            else
            {
                var resultDic = new Dictionary<TEntityForSave, MaskTree>();

                // An array of every criteria and every mask
                var maskAndCriteriaArray = permissions
                    .Where(e => !string.IsNullOrWhiteSpace(e.Criteria)) // Optimization: a null criteria is satisfied by the entire list of entities
                    .GroupBy(e => e.Criteria)
                    .Select(g => new
                    {
                        Criteria = g.Key,
                        Mask = g.Select(e => string.IsNullOrWhiteSpace(e.Mask) ? unrestrictedMask : MaskTree.Parse(e.Mask))
                        .Aggregate((t1, t2) => t1.UnionWith(t2)) // Takes the union of all the mask trees
                    }).ToArray();

                var universalPermissions = permissions
                    .Where(e => string.IsNullOrWhiteSpace(e.Criteria));

                bool hasUniversalPermissions = universalPermissions.Count() > 0;

                // This mask (if exists) applies to every single entity since the criteria is null
                var universalMask = hasUniversalPermissions ? universalPermissions
                    .Distinct()
                    .Select(e => MaskTree.Parse(e.Mask))
                    .Aggregate((t1, t2) => t1.UnionWith(t2)) : null;

                // Every criteria to every index of maskAndCriteriaArray
                var criteriaWithIndexes = maskAndCriteriaArray
                    .Select((e, index) => new IndexAndCriteria { Criteria = e.Criteria, Index = index });

                /////// Part (1) Permissions must allow manipulating the original data before the update

                var existingEntities = entities.Where(e => !0.Equals(e.Id));
                if (existingEntities.Any())
                {
                    // Get the Ids
                    TKey[] existingIds = existingEntities
                        .Select(e => e.Id).ToArray();

                    // Prepare the query
                    var query = GetRepository()
                        .Query<TEntity>()
                        .FilterByIds(existingIds);

                    // id => index in maskAndCriteriaArray
                    var criteriaMapList = await query
                        .GetIndexToIdMap<TKey>(criteriaWithIndexes);

                    // id => indices in maskAndCriteriaArray
                    var criteriaMapDictionary = criteriaMapList
                        .GroupBy(e => e.Id)
                        .ToDictionary(e => e.Key, e => e.Select(r => r.Index));

                    foreach (var entity in existingEntities)
                    {
                        var id = entity.Id;
                        MaskTree mask;

                        if (criteriaMapDictionary.ContainsKey(id))
                        {
                            // Those are entities that satisfy one or more non-null Criteria
                            mask = criteriaMapDictionary[id]
                                .Select(i => maskAndCriteriaArray[i].Mask)
                                .Aggregate((t1, t2) => t1.UnionWith(t2))
                                .UnionWith(universalMask);
                        }
                        else
                        {
                            if (hasUniversalPermissions)
                            {
                                // Those are entities that belong to the universal mask of null criteria
                                mask = universalMask;
                            }
                            else
                            {
                                // Cannot update or delete this record, it doesn't satisfy any criteria
                                throw new ForbiddenException();
                            }
                        }

                        resultDic.Add(entity, mask);
                    }
                }


                /////// Part (2) Permissions must work for the new data after the update, only for the modified properties
                {
                    // index in newItems => index in maskAndCriteriaArray
                    var criteriaMapList = await GetAsQuery(entities)
                        .GetIndexToIndexMap(criteriaWithIndexes);

                    var criteriaMapDictionary = criteriaMapList
                        .GroupBy(e => e.Id)
                        .ToDictionary(e => e.Key, e => e.Select(r => r.Index));

                    foreach (var (entity, index) in entities.Select((entity, i) => (entity, i)))
                    {
                        MaskTree mask;

                        if (criteriaMapDictionary.ContainsKey(index))
                        {
                            // Those are entities that satisfy one or more non-null Criteria
                            mask = criteriaMapDictionary[index]
                                .Select(i => maskAndCriteriaArray[i].Mask)
                                .Aggregate((t1, t2) => t1.UnionWith(t2))
                                .UnionWith(universalMask);
                        }
                        else
                        {
                            if (hasUniversalPermissions)
                            {
                                // Those are entities that belong to the universal mask of null criteria
                                mask = universalMask;
                            }
                            else
                            {
                                // Cannot insert or update this record, it doesn't satisfy any criteria
                                throw new ForbiddenException();
                            }
                        }

                        if (resultDic.ContainsKey(entity))
                        {
                            var entityMask = resultDic[entity];
                            resultDic[entity] = resultDic[entity].IntersectionWith(mask);

                        }
                        else
                        {
                            resultDic.Add(entity, mask);
                        }
                    }
                }

                return resultDic; // preserve the original order
            }
        }

        /// <summary>
        /// Implementation should prepare a select statement that returns the provided entities 
        /// as an SQL result from a user-defined table type variable or a temporary table, using
        /// the index of the entities as the Id (even if the Id of the entity is not integer).
        /// This SQL result will be used to determine which of these entities earn which permission
        /// masks.
        /// </summary>
        protected abstract Query<TEntity> GetAsQuery(List<TEntityForSave> entities);

        /// <summary>
        /// Saves the entities (Insert or Update) into the database after authorization and validation.
        /// </summary>
        /// <returns>Optionally returns the same entities in their persisted READ form.</returns>
        protected virtual async Task<EntitiesResponse<TEntity>> SaveImplAsync(List<TEntityForSave> entities, SaveArguments args)
        {
            try
            {
                // Parse arguments
                var expand = ExpandExpression.Parse(args.Expand);
                var returnEntities = args.ReturnEntities ?? false;

                // Trim all strings as a preprocessing step
                entities.ForEach(e => TrimStringProperties(e));

                // This implements field level security
                entities = await ApplyUpdatePermissionsMask(entities);

                // Start a transaction scope for save since it causes data modifications
                using (var trx = ControllerUtilities.CreateTransaction(null, GetSaveTransactionOptions()))
                {
                    // Validate
                    // Optional preprocessing
                    await SavePreprocessAsync(entities);

                    // Basic validation that applies to all entities
                    ControllerUtilities.ValidateUniqueIds(entities, ModelState, _localizer);

                    // Actual Validation
                    await SaveValidateAsync(entities);
                    if (!ModelState.IsValid)
                    {
                        throw new UnprocessableEntityException(ModelState);
                    }

                    // Save and retrieve Ids
                    var ids = await SaveExecuteAsync(entities, expand, returnEntities);

                    // Use the Ids to retrieve the items
                    EntitiesResponse<TEntity> result = null;
                    if (returnEntities && ids != null)
                    {
                        result = await GetByIdListAsync(ids.ToArray(), expand);
                    }

                    await PostProcess(result);

                    // Commit and return
                    await OnSaveCompleted();
                    trx.Complete();
                    return result;
                }
            }
            catch (Exception ex)
            {
                await OnSaveError(ex);
                throw ex;
            }
        }

        // Optional preprocessing of entities before they are validated and saved
        protected virtual Task<List<TEntityForSave>> SavePreprocessAsync(List<TEntityForSave> entities)
        {
            return Task.FromResult(entities);
        }

        /// <summary>
        /// Performs server side validation on the entities, this method is expected to 
        /// call AddModelError on the controller's ModelState if there is a validation problem,
        /// the method should NOT do validation that is already handled by validation attributes.
        /// Note: Don't check for unique Ids as this is already taken care of
        /// </summary>
        protected abstract Task SaveValidateAsync(List<TEntityForSave> entities);

        /// <summary>
        /// Persists the entities in the database, either creating them or updating, the call to this method is already wrapped inside a transaction
        /// </summary>
        protected abstract Task<List<TKey>> SaveExecuteAsync(List<TEntityForSave> entities, ExpandExpression expand, bool returnIds);

        /// <summary>
        /// Retrieves the <see cref="TransactionOptions"/> that are used in <see cref="Save(List{TEntityForSave}, SaveArguments)"/>,
        /// there is a default implementation that uses <see cref="IsolationLevel.ReadCommitted"/> and a timeout of 5 minutes
        /// </summary>
        protected virtual TransactionOptions GetSaveTransactionOptions()
        {
            return new TransactionOptions
            {
                IsolationLevel = IsolationLevel.ReadCommitted,
                Timeout = ControllerUtilities.DefaultTransactionTimeout()
            };
        }

        /// <summary>
        /// Gives an opportunity for inheriting controllers to post process the result before it is served
        /// </summary>
        protected virtual Task PostProcess(EntitiesResponse<TEntity> result)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Invoked just before committing the Save transaction and returning the result, errors thrown here will roll back the save transactions
        /// </summary>
        protected virtual Task OnSaveCompleted()
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Invoked when an error occurs in the save pipleline, good place to perform cleaning up of resources
        /// </summary>
        protected virtual Task OnSaveError(Exception ex)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Assumes that the view does not allow 'Create' permission level, if it does
        /// need to override it
        /// </summary>
        protected virtual async Task DeleteImplAsync(List<TKey> ids)
        {
            if (ids == null || !ids.Any())
            {
                return;
            }

            // Permissions
            await CheckActionPermissions("Delete", ids.ToArray());

            // Validate
            await DeleteValidateAsync(ids);
            if (!ModelState.IsValid)
            {
                throw new UnprocessableEntityException(ModelState);
            }

            // Execute
            await DeleteExecuteAsync(ids);
        }

        /// <summary>
        /// Deletes the entities specified by the list of Ids, call to this method is NOT wrapped inside a transaction
        /// </summary>
        protected abstract Task DeleteExecuteAsync(List<TKey> ids);

        /// <summary>
        /// Validates the delete operation before it happens
        /// </summary>
        protected abstract Task DeleteValidateAsync(List<TKey> ids);

        /// <summary>
        /// Trims all string properties of the entity
        /// </summary>
        protected virtual void TrimStringProperties(TEntityForSave entity)
        {
            entity.TrimStringProperties();
        }

        /// <summary>
        /// Syntactic sugar for localizing an error, prefixing it with "Row N: " and adding it to ModelState with an appropriate key
        /// </summary>
        /// <returns>False if the maximum errors was reached</returns>
        protected bool AddRowError(int rowNumber, string errorMessage, ModelStateDictionary modelState = null)
        {
            var ms = modelState ?? ModelState;
            ms.AddModelError($"Row{rowNumber}", _localizer["Row{0}", rowNumber] + ": " + errorMessage);
            return !ms.HasReachedMaxErrors;
        }

        // Private methods

        private ModelStateDictionary MapModelState(ModelStateDictionary modelState, Func<string, int?> rowNumberFromErrorKeyMap)
        {
            // Inline function for mapping a model state on entities to a model state on Excel rows
            // Copy the errors to another collection
            var mappedModelState = new ModelStateDictionary();

            // Transform the errors to the current collection
            foreach (var error in modelState)
            {
                int? rowNumber = rowNumberFromErrorKeyMap(error.Key);
                foreach (var errorMessage in error.Value.Errors)
                {
                    if (rowNumber != null)
                    {
                        // Error is specific to a row
                        AddRowError(rowNumber.Value, errorMessage.ErrorMessage, mappedModelState);
                    }
                    else
                    {
                        // Error is general to the imported file
                        mappedModelState.AddModelError(error.Key, errorMessage.ErrorMessage);
                    }
                }
            }

            return mappedModelState;
        }

        private FileResult ToFileResult(AbstractDataGrid abstractFile, string format)
        {
            // Get abstract grid

            FileHandlerBase handler;
            string contentType;
            if (format == FileFormats.Xlsx)
            {
                handler = new ExcelHandler(_localizer);
                contentType = MimeTypes.Xlsx;
            }
            else if (format == FileFormats.Csv)
            {
                handler = new CsvHandler(_localizer);
                contentType = MimeTypes.Csv;
            }
            else
            {
                throw new FormatException(_localizer["Error_UnknownFileFormat"]);
            }

            var fileStream = handler.ToFileStream(abstractFile);
            return File(((MemoryStream)fileStream).ToArray(), contentType);
        }
    }

    // public class 
}
