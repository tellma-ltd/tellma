using BSharp.Controllers.DTO;
using BSharp.Controllers.Misc;
using BSharp.Services.ImportExport;
using BSharp.Services.OData;
using BSharp.Services.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace BSharp.Controllers
{
    public abstract class CrudControllerBase<TDtoForSave, TDto, TDtoForQuery, TKey> : ReadControllerBase<TDto, TDtoForQuery, TKey>
        where TDtoForQuery : DtoForSaveKeyBase<TKey>, new()
        where TDtoForSave : DtoForSaveKeyBase<TKey>, new()
        where TDto : DtoForSaveKeyBase<TKey>, new()
    {
        // Private Fields

        private readonly ILogger _logger;
        private readonly IStringLocalizer _localizer;

        // Constructor

        public CrudControllerBase(ILogger logger, IStringLocalizer localizer, IServiceProvider serviceProvider) : base(logger, localizer, serviceProvider)
        {
            _logger = logger;
            _localizer = localizer;
        }

        // HTTP Methods

        [HttpPost]
        public virtual async Task<ActionResult<EntitiesResponse<TDto>>> Save([FromBody] List<TDtoForSave> entities, [FromQuery] SaveArguments args)
        {
            // Note here we use lists https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1?view=netcore-2.1
            // since the order is symantically relevant for reporting validation errors on the entities

            return await ControllerUtilities.ExecuteAndHandleErrorsAsync(async () =>
            {
                var result = await SaveImplAsync(entities, args);
                return Ok(result);
            }, _logger);
        }

        [HttpDelete]
        public virtual async Task<ActionResult> Delete([FromBody] List<TKey> ids)
        {
            return await ControllerUtilities.ExecuteAndHandleErrorsAsync(async () =>
            {
                await DeleteAsync(ids);
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
            decimal parsingToDtosForSave = 0;
            decimal attributeValidationInCSharp = 0;
            decimal validatingAndSaving = 0;

            return await ControllerUtilities.ExecuteAndHandleErrorsAsync(async () =>
            {
                // Parse the file into DTOs + map back to row numbers (The way source code is compiled into machine code + symbols file)
                var (dtos, rowNumberFromErrorKeyMap) = await ParseImplAsync(args); // This should check for primary code consistency!
                parsingToDtosForSave = Math.Round(((decimal)watch2.ElapsedMilliseconds) / 1000, 1);
                watch2.Restart();

                // Validation
                ObjectValidator.Validate(ControllerContext, null, null, dtos);
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
                    await SaveImplAsync(dtos, new SaveArguments { ReturnEntities = false });
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
                    Inserted = dtos.Count(e => e.EntityState == "Inserted"),
                    Updated = dtos.Count(e => e.EntityState == "Updated"),
                };

                // Record the time
                watch.Stop();
                var elapsed = Math.Round(((decimal)watch.ElapsedMilliseconds) / 1000, 1);
                result.Seconds = elapsed;
                result.ParsingToDtosForSave = parsingToDtosForSave;
                result.AttributeValidationInCSharp = attributeValidationInCSharp;
                result.ValidatingAndSaving = validatingAndSaving;

                return Ok(result);
            }, _logger);
        }

        [HttpPost("parse"), RequestSizeLimit(5 * 1024 * 1024)] // 5MB
        public virtual async Task<ActionResult<List<TDtoForSave>>> Parse([FromQuery] ParseArguments args)
        {
            // This method doesn't import the file in the DB, it simply parses it to 
            // DTOs that are ripe for saving, and returns those DTOs to the requester
            // This supports scenarios where only part of the required fields are present
            // in the imported file, or to support previewing the import before committing it
            try
            {
                var file = Request.Form.Files.FirstOrDefault();
                var dtos = await ParseImplAsync(args);
                return Ok(dtos);
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

        protected virtual async Task<(List<TDtoForSave>, Func<string, int?>)> ParseImplAsync(ParseArguments args)
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

            // Change the abstract grid to DTOs for save, and make sure no errors resulted that weren't thrown
            var (dtosForSave, keyMap) = await ToDtosForSave(abstractGrid, args);
            if (!ModelState.IsValid)
            {
                throw new UnprocessableEntityException(ModelState);
            }

            return (dtosForSave, keyMap);
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

        protected abstract Task<(List<TDtoForSave>, Func<string, int?>)> ToDtosForSave(AbstractDataGrid grid, ParseArguments args);

        protected abstract AbstractDataGrid GetImportTemplate();

        private async Task<List<TDtoForSave>> ApplyUpdatePermissionsMask(List<TDtoForSave> entities)
        {
            var permissions = await UserPermissions(PermissionLevel.Update);
            /* 
             * Step 1: Get complete mask for TDtoForSave
             * 
             * 
If there are no permissions: throw forbidden exception
else if ((1) at least one permission is criteria free and mask free, AND (2) all update entities (including nav entities) have a full mask in EntityMetadata) return safely
else {
  Do the magic to determine the mask that each entity is based on 

we need to do 2 things:

for each updated DTO (including nav DTOs), construct the flat Mask in Entity Metadata (only relevant if (2) is false)
for each DTO (including nav DTO), determine the flat permission Mask applicable, (only relevant if (1) is false) <- throw forbidden exception if any permission has no mask

For every DTO intersect the two masks into a MegaMask

Load entities by Ids from the DB,  <- need to know the DtoForSave for every Dto ---> or do I??
  For every Update DTO, any property that is missing from its MegaMask, copy that property value from the corresponding DB entity
  For every Insert DTO, any property that is missing from its MegaMask, set that property to NULL
}

return the entities
             * 
             * 
             */

            return await Task.FromResult(entities);
        }

        ///// <summary>
        ///// For each saved entity, determines the applicable mask
        ///// Verifies that the user has sufficient permissions to update he list of entities provided, this implementation 
        ///// assumes that the view has permission levels Read and Update only, which most entities do
        ///// </summary>
        //protected virtual async Task<List<(TDtoForSave Entity, MaskTree Mask)>> GetMasksForSavedEntities(List<TDtoForSave> entities, SaveArguments args)
        //{
        //    var unrestrictedMask = new MaskTree();
        //    var permissions = await UserPermissions(PermissionLevel.Update);
        //    if (!permissions.Any())
        //    {
        //        // User has no permissions on this table whatsoever, forbid
        //        throw new ForbiddenException();
        //    }
        //    else if (permissions.Any(e => string.IsNullOrWhiteSpace(e.Criteria) && string.IsNullOrWhiteSpace(e.Mask)))
        //    {
        //        // User has unfiltered update permission on the table => proceed
        //        return entities.Select(e => (e, unrestrictedMask)).ToList();
        //    }
        //    else
        //    {
        //        if(entities.Any(e => e.EntityState != EntityStates.Inserted && e.EntityState != EntityStates.Updated && e.EntityState != EntityStates.Deleted))
        //        {
        //            // Developer mistake
        //            throw new BadRequestException($"Some saved entities do not have the one of the 3 allowed states: {EntityStates.Inserted}, {EntityStates.Updated} or {EntityStates.Deleted}");
        //        }

        //        var resultDic = new Dictionary<TDtoForSave, MaskTree>();

        //        // an array of every criteria and every mask
        //        var maskAndCriteriaArray = permissions
        //            .Where(e => !string.IsNullOrWhiteSpace(e.Criteria)) // Optimization: a null criteria is satisfied by the entire list of DTOs
        //            .GroupBy(e => e.Criteria)
        //            .Select(g => new
        //            {
        //                Criteria = g.Key,
        //                Mask = g.Select(e => string.IsNullOrWhiteSpace(e.Mask) ? unrestrictedMask : MaskTree.GetMaskTree(MaskTree.Split(e.Mask)))
        //                .Aggregate((t1, t2) => t1.UnionWith(t2)) // takes the union of all the mask trees
        //            }).ToArray();

        //        var universalPermissions = permissions
        //            .Where(e => string.IsNullOrWhiteSpace(e.Criteria));

        //        bool hasUniversalPermissions = universalPermissions.Count() > 0;

        //        // This mask (if exists) applies to every single DTO since the criteria is null
        //        var universalMask = hasUniversalPermissions ? universalPermissions
        //            .Distinct()
        //            .Select(e => string.IsNullOrWhiteSpace(e.Mask) ? unrestrictedMask : MaskTree.GetMaskTree(MaskTree.Split(e.Mask)))
        //            .Aggregate((t1, t2) => t1.UnionWith(t2)) : null; // we use a seed here since if the collection is empty this will throw an error

        //        // Every criteria to every index of maskAndCriteriaArray
        //        var criteriaWithIndexes = maskAndCriteriaArray
        //            .Select((e, index) => new IndexAndCriteria { Criteria = e.Criteria, Index = index });

        //        /////// Part (1) Permissions must allow manipulating the original data before the update

        //        var existingEntities = entities.Where(e => e.EntityState == EntityStates.Updated ||
        //            e.EntityState == EntityStates.Deleted);
        //        var existingIds = existingEntities.Select(e => e.Id);

        //        if (existingIds.Any())
        //        {
        //            var query = CreateODataQuery()
        //                .FilterByIds(existingIds.ToArray());

        //            // id => index in maskAndCriteriaArray
        //            var criteriaMapList = await query
        //                .GetIndexToIdMap(criteriaWithIndexes);

        //            var criteriaMapDictionary = criteriaMapList
        //                .GroupBy(e => e.Id)
        //                .ToDictionary(e => e.Key, e => e.Select(r => r.Index));

        //            foreach (var dto in existingEntities)
        //            {
        //                var id = dto.Id;
        //                MaskTree mask;

        //                if (criteriaMapDictionary.ContainsKey(id))
        //                {
        //                    // Those are DTOs that satisfy one or more non-null Criteria
        //                    mask = criteriaMapDictionary[id]
        //                        .Select(i => maskAndCriteriaArray[i].Mask)
        //                        .Aggregate((t1, t2) => t1.UnionWith(t2))
        //                        .UnionWith(universalMask);
        //                }
        //                else
        //                {
        //                    if (hasUniversalPermissions)
        //                    {
        //                        // Those are DTOs that belong to the universal mask of null criteria
        //                        mask = universalMask;
        //                    }
        //                    else
        //                    {
        //                        // Cannot update or delete this record, it doesn't satisfy any criteria
        //                        throw new ForbiddenException();
        //                    }
        //                }

        //                resultDic.Add(dto, mask);
        //            }
        //        }


        //        /////// Part (2) Permissions must work for the new data after the update, only for the modified properties

        //        var newEntities = entities.Where(e => e.EntityState == EntityStates.Inserted ||
        //            e.EntityState == EntityStates.Updated).ToList();

        //        if (newEntities.Any())
        //        {
        //            var (preamble, sql, ps) = GetAsSql(newEntities);

        //            var query = CreateODataQuery()
        //                .FromSql(sql, preamble, ps.ToArray());

        //            // index in newItems => index in maskAndCriteriaArray
        //            var criteriaMapList = await query
        //                .GetIndexToIndexMap(criteriaWithIndexes);

        //            var criteriaMapDictionary = criteriaMapList
        //                .GroupBy(e => e.Id)
        //                .ToDictionary(e => e.Key, e => e.Select(r => r.Index));

        //            foreach (var (dto, index) in newEntities.Select((dto, i) => (dto, i)))
        //            {
        //                MaskTree mask;

        //                if (criteriaMapDictionary.ContainsKey(index))
        //                {
        //                    // Those are DTOs that satisfy one or more non-null Criteria
        //                    mask = criteriaMapDictionary[index]
        //                        .Select(i => maskAndCriteriaArray[i].Mask)
        //                        .Aggregate((t1, t2) => t1.UnionWith(t2))
        //                        .UnionWith(universalMask);
        //                }
        //                else
        //                {
        //                    if (hasUniversalPermissions)
        //                    {
        //                        // Those are DTOs that belong to the universal mask of null criteria
        //                        mask = universalMask;
        //                    }
        //                    else
        //                    {
        //                        // Cannot insert or update this record, it doesn't satisfy any criteria
        //                        throw new ForbiddenException();
        //                    }
        //                }

        //                if (resultDic.ContainsKey(dto))
        //                {
        //                    var dtoMask = resultDic[dto];
        //                    resultDic[dto] = resultDic[dto].IntersectionWith(mask);

        //                }
        //                else
        //                {
        //                    resultDic.Add(dto, mask);
        //                }
        //            }
        //        }

        //        return entities.Select(e => (e, resultDic[e])).ToList(); // preserve the original order
        //    }
        //}

        /// <summary>
        /// Implementation should prepare a select statement that returns the provided entities 
        /// as an SQL result from a user-defined table type variable or a temporary table, using
        /// the index of the entities as the Id (even if the Id of the entity is not integer).
        /// This SQL result will be used to determine which of these entities earn which permission
        /// masks
        /// </summary>
        protected abstract (string PreambleSql, string ComposableSql, List<SqlParameter> Parameters) GetAsSql(IEnumerable<TDtoForSave> entities);

        /// <summary>
        /// Saves the entities (Insert or Update) into the database after authorization and validation
        /// </summary>
        /// <returns>Optionally returns the same entities in their persisted READ form</returns>
        protected virtual async Task<EntitiesResponse<TDto>> SaveImplAsync(List<TDtoForSave> entities, SaveArguments args)
        {
            // Trim all strings as a preprocessing step
            entities.ForEach(e => TrimStringProperties(e));

            // This implements field level security
            entities = await ApplyUpdatePermissionsMask(entities);
            
            var dbFacade = GetDbContext().Database;
            using (var trx = dbFacade.GetDbConnection().BeginTransaction())
            {
                try
                {
                    // Enlist the current
                    dbFacade.UseTransaction(trx);

                    // Validate
                    await ValidateAsync(entities);
                    if (!ModelState.IsValid)
                    {
                        throw new UnprocessableEntityException(ModelState);
                    }

                    // Save
                    var ids = await PersistAsync(entities, args);

                    EntitiesResponse<TDto> result = null;
                    if((args.ReturnEntities ?? false) && ids != null)
                    {
                        // Prepare a query of the resultm, and clone it
                        var query = CreateODataQuery();
                        query.UseTransaction(trx);
                        query.FilterByIds(ids.ToArray());
                        var qClone = query.Clone();

                        // Expand the result as specified in the OData agruments and load into memory
                        query.Expand(args.Expand);
                        var memoryList = await query.ToListAsync(); // this is potentially unordered, should that be a concern?

                        // Apply the permissions on the result
                        var permissions = await UserPermissions(PermissionLevel.Read);
                        var defaultMask = GetDefaultMask();
                        await ApplyReadPermissionsMask(memoryList, qClone, permissions, defaultMask);

                        // Flatten related entities and map each to its respective DTO 
                        var relatedEntities = FlattenRelatedEntitiesAndTrim(memoryList, args.Expand);

                        // Map the primary result to DTOs as well
                        var resultData = Mapper.Map<List<TDto>>(memoryList);

                        // Prepare the result in a response object
                        result = new EntitiesResponse<TDto>
                        {
                            Data = resultData,
                            RelatedEntities = relatedEntities,
                            CollectionName = GetCollectionName(typeof(TDto))
                        };
                    }

                    // Commit and return
                    trx.Commit();
                    return result;
                }
                catch (Exception ex)
                {
                    // Roll back the transaction
                    trx.Rollback();
                    throw ex;
                }
            }
        }

        /// <summary>
        /// Performs server side validation on the entities, this method is expected to 
        /// call AddModelError on the controller's ModelState if there is a validation problem,
        /// the method should NOT do validation that is already handled by validation attributes
        /// </summary>
        protected abstract Task ValidateAsync(List<TDtoForSave> entities);

        /// <summary>
        /// Persists the entities in the database, either creating them or updating them depending on the EntityState
        /// </summary>
        protected abstract Task<List<TKey>> PersistAsync(List<TDtoForSave> entitiesAndMasks, SaveArguments args);

        /// <summary>
        /// Begins the transaction that wraps validation and persistence of data inside the save API 
        /// implementation, each controller determines its suitable transaction isolation level
        /// </summary>
        protected virtual IsolationLevel GetSaveTransactionIsolationLevel()
        {
            return IsolationLevel.ReadCommitted;
        }

        /// <summary>
        /// Assumes that the view does not allow 'Create' permission level, if it does
        /// need to override it
        /// </summary>
        protected virtual async Task DeleteImplAsync(List<TKey> ids)
        {
            if(ids == null || !ids.Any())
            {
                return;
            }

            await CheckActionPermissions(ids);
            await ValidateDeleteAsync(ids);
            if (!ModelState.IsValid)
            {
                throw new UnprocessableEntityException(ModelState);
            }

            await DeleteAsync(ids);
        }

        /// <summary>
        /// Deletes the entities specified by the list of Ids
        /// Assumes that the view does not allow 'Create' permission level, if it does
        /// ignore this method and override <see cref="DeleteImplAsync(List{TKey})"/> instead
        /// </summary>
        protected abstract Task DeleteAsync(List<TKey> ids);

        protected virtual Task ValidateDeleteAsync(List<TKey> ids)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Trims all string properties of the entity
        /// </summary>
        protected virtual void TrimStringProperties(TDtoForSave entity)
        {
            entity.TrimStringProperties();
        }

        /// <summary>
        /// Constructs a SQL data table containing all the entities in all the collections
        /// and adds an index and a header index, this is useful for child collections that
        /// are passed to SQL alongside their headers, the include predicate optionally filters
        /// the items but keeps the original indexing
        /// </summary>
        protected DataTable DataTableWithHeaderIndex<T>(IEnumerable<(List<T> Items, int HeaderIndex)> collections, Predicate<T> include = null)
        {
            include = include ?? (e => true);
            DataTable table = new DataTable();

            // The column order MUST match the column order in the user-defined table type
            table.Columns.Add(new DataColumn("Index", typeof(int)));
            table.Columns.Add(new DataColumn("HeaderIndex", typeof(int)));

            var props = ControllerUtilities.GetPropertiesBaseFirst(typeof(T)).Where(e => !e.PropertyType.IsList());
            foreach (var prop in props)
            {
                var propType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                var column = new DataColumn(prop.Name, propType);
                if (propType == typeof(string))
                {
                    // For string columns, it is more performant to explicitly specify the maximum column size
                    // According to this article: http://www.dbdelta.com/sql-server-tvp-performance-gotchas/
                    var stringLengthAttribute = prop.GetCustomAttribute<StringLengthAttribute>(inherit: true);
                    if (stringLengthAttribute != null)
                    {
                        column.MaxLength = stringLengthAttribute.MaximumLength;
                    }
                }

                table.Columns.Add(column);
            }

            foreach (var (items, headerIndex) in collections)
            {
                int index = 0;
                foreach (var item in items)
                {
                    if (include(item))
                    {
                        DataRow row = table.NewRow();

                        // We add index and header index properties since SQL works with un-ordered sets
                        row["Index"] = index;
                        row["HeaderIndex"] = headerIndex;

                        // Add the remaining properties
                        foreach (var prop in props)
                        {
                            var propValue = prop.GetValue(item);
                            row[prop.Name] = propValue ?? DBNull.Value;
                        }

                        table.Rows.Add(row);
                    }

                    index++;
                }
            }

            return table;
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

        /// <summary>
        /// Determines whether the given exception is a foreign key violation on delete
        /// </summary>
        protected bool IsForeignKeyViolation(SqlException ex)
        {
            return ex.Number == 547;
        }

        // Private methods

        private ModelStateDictionary MapModelState(ModelStateDictionary modelState, Func<string, int?> rowNumberFromErrorKeyMap)
        {
            // Inline function for mapping a model state on DTOs to a model state on Excel rows
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
