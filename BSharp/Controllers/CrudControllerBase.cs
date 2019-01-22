using AutoMapper;
using BSharp.Controllers.DTO;
using BSharp.Controllers.Misc;
using BSharp.Services.ImportExport;
using BSharp.Services.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
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
using System.Reflection;
using System.Threading.Tasks;
using M = BSharp.Data.Model;

namespace BSharp.Controllers
{
    [ApiController]
    public abstract class CrudControllerBase<TModel, TDto, TDtoForSave, TKey> : ReadControllerBase<TModel, TDto, TKey>
        where TModel : M.ModelBase
        where TDtoForSave : DtoForSaveKeyBase<TKey>
        where TDto : DtoForSaveKeyBase<TKey>
    {
        // Private Fields

        private readonly ILogger _logger;
        private readonly IStringLocalizer _localizer;
        private readonly IMapper _mapper;

        // Constructor

        public CrudControllerBase(ILogger logger, IStringLocalizer localizer, IMapper mapper) : base(logger, localizer, mapper)
        {
            _logger = logger;
            _localizer = localizer;
            _mapper = mapper;
        }

        // HTTP Methods

        [HttpPost]
        public virtual async Task<ActionResult<EntitiesResponse<TDto>>> Save([FromBody] List<TDtoForSave> entities, [FromQuery] SaveArguments args)
        {
            // Note here we use lists https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1?view=netcore-2.1
            // since the order is symantically relevant for reporting validation errors on the entities
            try
            {
                var result = await SaveImplAsync(entities, args);
                return Ok(result);
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

        [HttpDelete]
        public virtual async Task<ActionResult> Delete([FromBody] List<TKey> ids)
        {
            try
            {
                // TODO: Authorize DELETE

                await DeleteImplAsync(ids);
                return Ok();
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

        [HttpGet("template")]
        public virtual ActionResult Template([FromQuery] TemplateArguments args)
        {
            try
            {
                var abstractFile = GetImportTemplate();
                return ToFileResult(abstractFile, args.Format);
                // return Ok(abstractFile);
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

            try
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

        /// <summary>
        /// Saves the entities (Insert or Update) into the database after authorization and validation
        /// </summary>
        /// <returns>Optionally returns the same entities in their persisted READ form</returns>
        protected virtual async Task<EntitiesResponse<TDto>> SaveImplAsync(List<TDtoForSave> entities, SaveArguments args)
        {
            // TODO Authorize POST

            // Trim all strings as a preprocessing step
            entities.ForEach(e => TrimStringProperties(e));

            using (var trx = await BeginSaveTransaction())
            {
                try
                {
                    // Validate
                    await ValidateAsync(entities);
                    if (!ModelState.IsValid)
                    {
                        throw new UnprocessableEntityException(ModelState);
                    }

                    // Save
                    var memoryList = await PersistAsync(entities, args);

                    // Flatten related entities and map each to its respective DTO 
                    var relatedEntities = FlattenRelatedEntities(memoryList, args.Expand);

                    // Map the primary result to DTOs as well
                    var resultData = Map(memoryList);

                    // Prepare the result in a response object
                    var result = new EntitiesResponse<TDto>
                    {
                        Data = resultData,
                        RelatedEntities = relatedEntities,
                        CollectionName = GetCollectionName(typeof(TDto))
                    };

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
        protected abstract Task<List<TModel>> PersistAsync(List<TDtoForSave> entities, SaveArguments args);

        /// <summary>
        /// Begins the transaction that wraps validation and persistence of data inside the save API 
        /// implementation, each controller determines its suitable transaction isolation level
        /// </summary>
        protected abstract Task<IDbContextTransaction> BeginSaveTransaction();

        /// <summary>
        /// Deletes the entities specified by the list of Ids
        /// </summary>
        protected abstract Task DeleteImplAsync(List<TKey> ids);

        /// <summary>
        /// Trims all string properties of the entity
        /// </summary>
        protected virtual void TrimStringProperties(TDtoForSave entity)
        {
            entity.TrimStringProperties();
        }

        /// <summary>
        /// Constructs a SQL data table containing all the public properties of the 
        /// entities' type and populates the data table with the provided entities
        /// </summary>
        protected DataTable DataTable<T>(IEnumerable<T> entities, bool addIndex = false)
        {

            DataTable table = new DataTable();
            if (addIndex)
            {
                // The column order MUST match the column order in the user-defined table type
                table.Columns.Add(new DataColumn("Index", typeof(int)));
            }

            var props = GetPropertiesBaseFirst(typeof(T)).Where(e => !e.PropertyType.IsList());
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

            int index = 0;
            foreach (var entity in entities)
            {
                DataRow row = table.NewRow();

                // We add an index property since SQL works with un-ordered sets
                if (addIndex)
                {
                    row["Index"] = index++;
                }

                // Add the remaining properties
                foreach (var prop in props)
                {
                    var propValue = prop.GetValue(entity);
                    row[prop.Name] = propValue ?? DBNull.Value;
                }

                table.Rows.Add(row);
            }

            return table;
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

            var props = GetPropertiesBaseFirst(typeof(T)).Where(e => !e.PropertyType.IsList());
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
        /// This is alternative for <see cref="Type.GetProperties"/>
        /// that returns base class properties before inherited class properties
        /// Credit: https://bit.ly/2UGAkKj
        /// </summary>
        protected PropertyInfo[] GetPropertiesBaseFirst(Type type)
        {
            var orderList = new List<Type>();
            var iteratingType = type;
            do
            {
                orderList.Insert(0, iteratingType);
                iteratingType = iteratingType.BaseType;
            } while (iteratingType != null);

            var props = type.GetProperties()
                .OrderBy(x => orderList.IndexOf(x.DeclaringType))
                .ToArray();

            return props;
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

        /// <summary>
        /// Attempts to intelligently parse an object (that comes from an imported file) to a DateTime
        /// </summary>
        protected DateTime? ParseImportedDateTime(object value)
        {
            if (value == null)
            {
                return null;
            }

            DateTime dateTime;

            if (value.GetType() == typeof(double))
            {
                // Double indicates the OLE Automation date typically represented in excel
                dateTime = DateTime.FromOADate((double)value);
            }
            else
            {
                // Parse the import value into a DateTime
                var valueString = value.ToString();
                dateTime = DateTime.Parse(valueString);
            }


            return dateTime;
        }

        /// <summary>
        /// Changes the DateTime into a DateTimeOffset by adding the user's local timezone, this effectively
        /// acts as the reverse of <see cref="ToExportDateTime(DateTimeOffset?)"/>
        /// </summary>
        protected DateTimeOffset? AddUserTimeZone(DateTime? value)
        {
            if (value == null)
            {
                return null;
            }

            // The date time supplied in the import does not the contain time zone offset
            // The code below adds the current user time zone to the date time supplied
            var timeZone = TimeZoneInfo.Local;  // TODO: Use the user time zone   
            var offset = timeZone.GetUtcOffset(DateTimeOffset.Now);
            var dtOffset = new DateTimeOffset(value.Value, offset);

            return dtOffset;
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
}
