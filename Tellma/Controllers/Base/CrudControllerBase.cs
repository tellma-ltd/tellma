using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Tellma.Controllers.Dto;
using Tellma.Controllers.ImportExport;
using Tellma.Controllers.Utilities;
using Tellma.Data;
using Tellma.Data.Queries;
using Tellma.Entities;
using Tellma.Services.Utilities;

namespace Tellma.Controllers
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

        // Constructor

        public CrudControllerBase(ILogger logger) : base(logger)
        {
            _logger = logger;
        }

        // HTTP Methods

        [HttpPost]
        public virtual async Task<ActionResult<EntitiesResponse<TEntity>>> Save([FromBody] List<TEntityForSave> entities, [FromQuery] SaveArguments args)
        {
            // Note here we use lists https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1?view=netcore-2.1
            // since the order is semantically relevant for reporting validation errors

            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                // Calculate server time at the very beginning for consistency
                var serverTime = DateTimeOffset.UtcNow;

                // Load the data
                var service = GetCrudService();
                var (data, extras) = await service.Save(entities, args);

                await OnSuccessfulSave(data, extras);

                // Transform it and return the result
                var returnEntities = args?.ReturnEntities ?? false;
                if (returnEntities)
                {
                    // Transform the entities as an EntitiesResponse
                    var response = TransformToEntitiesResponse(data, extras, serverTime, cancellation: default);

                    // Return the response
                    return Ok(response);
                }
                else
                {
                    // Return 200
                    return Ok();
                }
            }, _logger);
        }

        [HttpDelete]
        public virtual async Task<ActionResult> Delete([FromQuery] List<TKey> i)
        {
            // "i" parameter is given a short name to allow a large number of
            // ids to be passed in the query string before the url size limit
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                var service = GetCrudService();
                await service.Delete(ids: i);

                await OnSuccessfulDelete(ids: i);

                return Ok();
            }, _logger);
        }

        [HttpDelete("{id}")]
        public virtual async Task<ActionResult> DeleteId(TKey id)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                var ids = new List<TKey> { id };
                var service = GetCrudService();
                await service.Delete(ids);

                return Ok();
            }, _logger);
        }

        // Helpers

        protected override FactGetByIdServiceBase<TEntity, TKey> GetFactGetByIdService()
        {
            return GetCrudService();
        }

        protected abstract CrudServiceBase<TEntityForSave, TEntity, TKey> GetCrudService();

        /// <summary>
        /// Gives an opportunity for implementations to add headers to the response if a save was successful,
        /// useful to set x-version headers for controllers that cause changes that invalidate the cache
        /// </summary>
        protected virtual Task OnSuccessfulSave(List<TEntity> data, Extras extras)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Gives an opportunity for implementations to add headers to the response if a delete was successful,
        /// useful to set x-version headers for controllers that cause changes that invalidate the cache
        /// </summary>
        protected virtual Task OnSuccessfulDelete(List<TKey> ids)
        {
            return Task.CompletedTask;
        }


        //[HttpPost("import"), RequestSizeLimit(5 * 1024 * 1024)] // 5MB
        //public virtual async Task<ActionResult<ImportResult>> Import2([FromQuery] ImportArguments args)
        //{
        //    return await ControllerUtilities.InvokeActionImpl(async () =>
        //    {
        //        IFormFile formFile = Request.Form.Files.FirstOrDefault();
        //        var contentType = formFile?.ContentType;
        //        var fileName = formFile?.FileName;
        //        using var fileStream = formFile?.OpenReadStream();

        //        var service = GetCrudService();
        //        var result = await service.Import(fileStream, fileName, contentType, args);

        //        return Ok(result);
        //    }, _logger);
        //}


        //#region Import Stuff

        //private readonly IStringLocalizer<Strings> _localizer;

        //[HttpGet("template")]
        //public virtual ActionResult Template([FromQuery] TemplateArguments args)
        //{
        //    try
        //    {
        //        var abstractFile = GetImportTemplate();
        //        return ToFileResult(abstractFile, args.Format);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError($"Error: {ex.Message} {ex.StackTrace}");
        //        return BadRequest(ex.Message);
        //    }
        //}

        //[HttpPost("import"), RequestSizeLimit(5 * 1024 * 1024)] // 5MB
        //public virtual async Task<ActionResult<ImportResult>> Import([FromQuery] ImportArguments args)
        //{
        //    return await ControllerUtilities.InvokeActionImpl(async () =>
        //    {
        //        // Parse the file into Entities + map back to row numbers (The way source code is compiled into machine code + symbols file)
        //        var (entities, rowNumberFromErrorKeyMap) = await ParseImplAsync(args); // This should check for primary code consistency!

        //        // Validation
        //        ObjectValidator.Validate(ControllerContext, null, null, entities);

        //        if (!ModelState.IsValid)
        //        {
        //            var mappedModelState = MapModelState(ModelState, rowNumberFromErrorKeyMap);
        //            throw new UnprocessableEntityException(mappedModelState);
        //        }

        //        // Saving
        //        try
        //        {
        //            await SaveImplAsync(entities, new SaveArguments { ReturnEntities = false });
        //        }
        //        catch (UnprocessableEntityException ex)
        //        {
        //            var mappedModelState = MapModelState(ex.ModelState, rowNumberFromErrorKeyMap);
        //            throw new UnprocessableEntityException(mappedModelState);
        //        }

        //        var result = new ImportResult
        //        {
        //            Inserted = entities.Count(e => e.Id?.Equals(default(TKey)) ?? false),
        //            Updated = entities.Count(e => !(e.Id?.Equals(default(TKey)) ?? false)),
        //        };

        //        // Record the time
        //        result.Seconds = elapsed;
        //        result.ParsingToDtosForSave = parsingToEntitiesForSave;
        //        result.AttributeValidationInCSharp = attributeValidationInCSharp;
        //        result.ValidatingAndSaving = validatingAndSaving;

        //        return Ok(result);
        //    }, _logger);
        //}

        //[HttpPost("parse"), RequestSizeLimit(5 * 1024 * 1024)] // 5MB
        //public virtual async Task<ActionResult<List<TEntityForSave>>> Parse([FromQuery] ParseArguments args)
        //{
        //    // This method doesn't import the file in the DB, it simply parses it to 
        //    // Entities that are ripe for saving, and returns those Entities to the requester
        //    // This supports scenarios where only part of the required fields are present
        //    // in the imported file, or to support previewing the import before committing it
        //    try
        //    {
        //        var file = Request.Form.Files.FirstOrDefault();
        //        var entities = await ParseImplAsync(args);
        //        return Ok(entities);
        //    }
        //    catch (UnprocessableEntityException ex)
        //    {
        //        return UnprocessableEntity(ex.ModelState);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError($"Error: {ex.Message} {ex.StackTrace}");
        //        return BadRequest(ex.Message);
        //    }
        //}

        //// Abstract and virtual members


        //protected virtual async Task<(List<TEntityForSave>, Func<string, int?>)> ParseImplAsync(ParseArguments args)
        //{
        //    var file = Request.Form.Files.FirstOrDefault();
        //    if (file == null)
        //    {
        //        throw new BadRequestException(_localizer["Error_NoFileWasUploaded"]);
        //    }

        //    var abstractGrid = FileToAbstractGrid(file, args);
        //    if (abstractGrid.Count < 2)
        //    {
        //        ModelState.AddModelError("", _localizer["Error_EmptyImportFile"]);
        //        throw new UnprocessableEntityException(ModelState);
        //    }

        //    // Change the abstract grid to entities for save, and make sure no errors resulted that weren't thrown
        //    var (entitiesForSave, keyMap) = await ToEntitiesForSave(abstractGrid, args);
        //    if (!ModelState.IsValid)
        //    {
        //        throw new UnprocessableEntityException(ModelState);
        //    }

        //    return (entitiesForSave, keyMap);
        //}

        //protected virtual AbstractDataGrid FileToAbstractGrid(IFormFile file, ParseArguments args)
        //{
        //    // Determine an appropriate file handler based on the file metadata
        //    FileHandlerBase handler;
        //    if (file.ContentType == "text/csv" || (file.FileName?.ToLower()?.EndsWith(".csv") ?? false))
        //    {
        //        handler = new Services.ImportExport.CsvHandler(_localizer);
        //    }
        //    else if (file.ContentType == "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" || (file.FileName?.ToLower()?.EndsWith(".xlsx") ?? false))
        //    {
        //        handler = new ExcelHandler(_localizer);
        //    }
        //    else
        //    {
        //        throw new FormatException(_localizer["Error_UnknownFileFormat"]);
        //    }

        //    using var fileStream = file.OpenReadStream();
        //    // Use the handler to unpack the file into an abstract grid and return it
        //    AbstractDataGrid abstractGrid = handler.ToAbstractGrid(fileStream);
        //    return abstractGrid;
        //}

        //protected Task<(List<TEntityForSave>, Func<string, int?>)> ToEntitiesForSave(AbstractDataGrid _1, ParseArguments _2)
        //{
        //    throw new NotImplementedException();
        //}

        //protected AbstractDataGrid GetImportTemplate()
        //{
        //    throw new NotImplementedException();
        //}

        ///// <summary>
        ///// Syntactic sugar for localizing an error, prefixing it with "Row N: " and adding it to ModelState with an appropriate key
        ///// </summary>
        ///// <returns>False if the maximum errors was reached</returns>
        //protected bool AddRowError(int rowNumber, string errorMessage, ModelStateDictionary modelState = null)
        //{
        //    var ms = modelState ?? ModelState;
        //    ms.AddModelError($"Row{rowNumber}", _localizer["Row{0}", rowNumber] + ": " + errorMessage);
        //    return !ms.HasReachedMaxErrors;
        //}

        //private ModelStateDictionary MapModelState(ModelStateDictionary modelState, Func<string, int?> rowNumberFromErrorKeyMap)
        //{
        //    // Inline function for mapping a model state on entities to a model state on Excel rows
        //    // Copy the errors to another collection
        //    var mappedModelState = new ModelStateDictionary();

        //    // Transform the errors to the current collection
        //    foreach (var error in modelState)
        //    {
        //        int? rowNumber = rowNumberFromErrorKeyMap(error.Key);
        //        foreach (var errorMessage in error.Value.Errors)
        //        {
        //            if (rowNumber != null)
        //            {
        //                // Error is specific to a row
        //                AddRowError(rowNumber.Value, errorMessage.ErrorMessage, mappedModelState);
        //            }
        //            else
        //            {
        //                // Error is general to the imported file
        //                mappedModelState.AddModelError(error.Key, errorMessage.ErrorMessage);
        //            }
        //        }
        //    }

        //    return mappedModelState;
        //}

        //private FileResult ToFileResult(AbstractDataGrid abstractFile, string format)
        //{
        //    // Get abstract grid

        //    FileHandlerBase handler;
        //    string contentType;
        //    if (format == FileFormats.Xlsx)
        //    {
        //        handler = new ExcelHandler(_localizer);
        //        contentType = MimeTypes.Excel;
        //    }
        //    else if (format == FileFormats.Csv)
        //    {
        //        handler = new Services.ImportExport.CsvHandler(_localizer);
        //        contentType = MimeTypes.Csv;
        //    }
        //    else
        //    {
        //        throw new FormatException(_localizer["Error_UnknownFileFormat"]);
        //    }

        //    var fileStream = handler.ToFileStream(abstractFile);
        //    return File(((MemoryStream)fileStream).ToArray(), contentType);
        //}

        //#endregion





    }

    public abstract class CrudServiceBase<TEntityForSave, TEntity, TKey> : FactGetByIdServiceBase<TEntity, TKey>
        where TEntityForSave : EntityWithKey<TKey>, new()
        where TEntity : EntityWithKey<TKey>, new()
    {
        private readonly IStringLocalizer _localizer;
        // private readonly MetadataProvider _metadata;

        public CrudServiceBase(IStringLocalizer localizer) : base(localizer)
        {
            _localizer = localizer;
            // _metadata = metadata;
        }

        #region Save

        /// <summary>
        /// Saves the entities (Insert or Update) into the database after authorization and validation.
        /// </summary>
        /// <returns>Optionally returns the same entities in their persisted READ form.</returns>
        public virtual async Task<(List<TEntity>, Extras)> Save(List<TEntityForSave> entities, SaveArguments args)
        {
            try
            {
                // Parse arguments
                var returnEntities = args?.ReturnEntities ?? false;

                // Trim all strings as a preprocessing step
                entities.ForEach(e => TrimStringProperties(e));

                // This implements field level security
                entities = await ApplyUpdatePermissionsMask(entities);

                // Start a transaction scope for save since it causes data modifications
                using var trx = ControllerUtilities.CreateTransaction(null, GetSaveTransactionOptions());

                // Optional preprocessing
                await SavePreprocessAsync(entities);

                // Validate
                // Basic validation that applies to all entities
                ControllerUtilities.ValidateUniqueIds(entities, ModelState, _localizer);

                // Actual Validation
                await SaveValidateAsync(entities);
                if (!ModelState.IsValid)
                {
                    throw new UnprocessableEntityException(ModelState);
                }

                // Save and retrieve Ids
                var ids = await SaveExecuteAsync(entities, returnEntities);

                List<TEntity> data = null;
                Extras extras = null;
                if (returnEntities)
                {
                    (data, extras) = await GetByIds(ids, args, cancellation: default);
                }

                // Commit and return
                await OnSaveCompleted();
                trx.Complete();

                return (data, extras);
            }
            catch (Exception ex)
            {
                await OnSaveError(ex);
                throw ex;
            }
        }

        /// <summary>
        /// Optional preprocessing of entities before they are validated and saved
        /// </summary>
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
        protected abstract Task<List<TKey>> SaveExecuteAsync(List<TEntityForSave> entities, bool returnIds);

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
        /// Trims all string properties of the entity
        /// </summary>
        protected virtual void TrimStringProperties(TEntityForSave entity)
        {
            entity.TrimStringProperties();
        }

        private async Task<List<TEntityForSave>> ApplyUpdatePermissionsMask(List<TEntityForSave> entities)
        {
            //  var entityMasks = GetMasksForSavedEntities(entities);
            // var permissions = await UserPermissions(Constants.Update);

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
            if (entities == null || !entities.Any())
            {
                return new Dictionary<TEntityForSave, MaskTree>();
            }

            var unrestrictedMask = new MaskTree();
            var permissions = await UserPermissions(Constants.Update, cancellation: default); // non-cancellable
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
                        .GetIndexToIdMap<TKey>(criteriaWithIndexes, cancellation: default);

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
                        .GetIndexToIndexMap(criteriaWithIndexes, cancellation: default);

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
        protected virtual Query<TEntity> GetAsQuery(List<TEntityForSave> entities)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Delete

        /// <summary>
        /// Assumes that the view does not allow 'Create' permission level, if it does
        /// need to override it
        /// </summary>
        public virtual async Task Delete(List<TKey> ids)
        {
            if (ids == null || !ids.Any())
            {
                return;
            }

            // Permissions
            await CheckActionPermissions("Delete", ids);

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

        #endregion

        //public async Task<ImportResult> Import(Stream fileStream, string fileName, string contentType, ImportArguments args)
        //{
        //    if (fileStream == null)
        //    {
        //        throw new BadRequestException(_localizer["Error_NoFileWasUploaded"]);
        //    }

        //    // Determine an appropriate file handler based on the file metadata
        //    IDataExtracter extractor = GetSuitableExtracter(fileName, contentType);

        //    // Extract the data
        //    IEnumerable<string[]> data = extractor.Extract(fileStream);
        //    if (!data.Any())
        //    {
        //        throw new BadRequestException(_localizer["Error_UploadedFileWasEmpty"]);
        //    }

        //    // Map the columns
        //    var headers = data.First();
        //    var mappingInfo = MapColumns(headers);


        //    // Load related entities (including principal entities if update or merge)


        //    // Parse the data 

        //    try
        //    {
        //        // Save the data

        //        // Return import result
        //    }
        //    catch (UnprocessableEntityException ex)
        //    {
        //        // Map errors to row numbers
        //    }

        //    throw new NotImplementedException();
        //}

        protected MappingInfo MapColumns(string[] headers)
        {
            throw new NotImplementedException();
            //var result = new MappingInfo();
            //foreach (var header in headers)
            //{
            //    var steps = SplitHeader(header);
            //    var currentType = typeof(TEntityForSave);

            //    foreach (var step in steps)
            //    {
            //        var match = _metadata.GetMetadataForProperties(currentType).FirstOrDefault(p => p.DisplayName == step);
            //        match.
            //    }
            //}
        }

        private IEnumerable<string> SplitHeader(string header)
        {
            var builder = new StringBuilder();
            for (int i = 0; i < header.Length; i++)
            {
                char c = header[i];
                if (c == '/')
                {
                    if (i + 1 < header.Length && header[i + 1] == '/') // Escaped
                    {
                        builder.Append(c);
                        i++; // Ignore the second forward slash
                    }
                    else
                    {
                        yield return builder.ToString();
                        builder = new StringBuilder();
                    }
                }
                else
                {
                    builder.Append(c);
                }
            }
        }

        private IDataExtracter GetSuitableExtracter(string fileName, string contentType)
        {
            IDataExtracter handler;
            if (contentType == MimeTypes.Csv || (fileName?.ToLower()?.EndsWith(".csv") ?? false))
            {
                handler = new CsvHandler();
            }
            else if (contentType == MimeTypes.Excel || (fileName?.ToLower()?.EndsWith(".xlsx") ?? false))
            {
                handler = new ExcelHandler();
            }
            else
            {
                throw new FormatException(_localizer["Error_UnknownFileFormat"]);
            }

            return handler;
        }
    }
}
