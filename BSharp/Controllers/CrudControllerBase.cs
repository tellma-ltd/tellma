using AutoMapper;
using BSharp.Controllers.DTO;
using BSharp.Controllers.Misc;
using BSharp.Services.ImportExport;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using M = BSharp.Data.Model;

namespace BSharp.Controllers
{
    [ApiController]
    public abstract class CrudControllerBase<TModel, TDto, TDtoForSave, TKey> : ControllerBase
        where TModel : M.ModelForSaveBase
        where TDtoForSave : DtoForSaveKeyBase<TKey>
        where TDto : TDtoForSave
    {
        // Constants
        private const int DEFAULT_MAX_PAGE_SIZE = 5000;

        // Private Fields
        private readonly ILogger _logger;
        private readonly IStringLocalizer _localizer;
        private readonly IMapper _mapper;

        // Constructor
        public CrudControllerBase(ILogger logger, IStringLocalizer localizer, IMapper mapper)
        {
            _logger = logger;
            _localizer = localizer;
            _mapper = mapper;
        }

        // HTTP Methods
        [HttpGet]
        public virtual async Task<ActionResult<GetResponse<TDto>>> Get([FromQuery] GetArguments args)
        {
            try
            {
                var result = await GetImplAsync(args);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message} {ex.StackTrace}");
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public virtual async Task<ActionResult<GetByIdResponse<TDto>>> GetById(TKey id, [FromQuery] GetByIdArguments args)
        {
            try
            {
                // TODO Authorize GET by Id

                // Expand
                var query = SingletonQuery(GetBaseQuery(), id);
                query = Expand(query, args.Expand);

                // Load
                var dbEntity = await query.FirstOrDefaultAsync();
                if (dbEntity == null)
                {
                    throw new NotFoundException<TKey>(id);
                }

                // Flatten Related Entities
                var relatedEntities = FlattenRelatedEntities(dbEntity);

                // Map the primary result to DTO too
                var entity = Map(dbEntity);

                // TODO apply the SELECT

                // Return
                var result = new GetByIdResponse<TDto>
                {
                    Entity = entity,
                    RelatedEntities = relatedEntities
                };

                return Ok(result);
            }
            catch (NotFoundException<TKey> ex)
            {
                return NotFound(ex.Ids);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message} {ex.StackTrace}");
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public virtual async Task<ActionResult<List<TDto>>> Save([FromBody] List<TDtoForSave> entities, [FromQuery] SaveArguments args)
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
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message} {ex.StackTrace}");
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("export")]
        public virtual async Task<ActionResult> Export([FromQuery] ExportArguments args)
        {
            try
            {
                // Get abstract grid
                var response = await GetImplAsync(args);
                var abstractFile = ToAbstractGrid(response, args);
                return ToFileResult(abstractFile, args.Format);
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

        [HttpPost("import")]
        public virtual async Task<ActionResult<ImportResult>> Import(IFormFile file, [FromQuery] ImportArguments args)
        {
            try
            {
                // Parse the file into DTOs + map back to row numbers (The way source code is compiled into machine code + symbol file)
                var (dtos, rowNumberFromErrorKeyMap) = await ParseImplAsync(file, args); // This should check for primary code consistency!

                // Validation
                ObjectValidator.Validate(ControllerContext, null, null, dtos);
                if (!ModelState.IsValid)
                {
                    var mappedModelState = MapModelState(ModelState, rowNumberFromErrorKeyMap);
                    throw new UnprocessableEntityException(mappedModelState);
                }

                // Saving
                try
                {
                    await SaveImplAsync(dtos, new SaveArguments { ReturnEntities = false });
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
                    Deleted = dtos.Count(e => e.EntityState == "Deleted")
                };

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

        [HttpPost("parse")]
        public virtual async Task<ActionResult<List<TDtoForSave>>> Parse(IFormFile file, ParseArguments args)
        {
            // This method doesn't import the file in the DB, it simply parses it to 
            // DTOs that are ripe for saving, and returns those DTOs to the requester
            // This supports scenarios where only part of the required fields are present
            // in the imported file, or to support previewing the import before committing it
            try
            {
                var dtos = await ParseImplAsync(file, args);
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

        protected virtual async Task<(List<TDtoForSave>, Func<string, int?>)> ParseImplAsync(IFormFile file, ParseArguments args)
        {
            var abstractGrid = ToAbstractGrid(file, args);
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

        protected virtual AbstractDataGrid ToAbstractGrid(IFormFile file, ParseArguments args)
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

        // Not Done
        protected abstract Task<(List<TDtoForSave>, Func<string, int?>)> ToDtosForSave(AbstractDataGrid grid, ParseArguments args);

        // Not Done
        protected abstract AbstractDataGrid GetImportTemplate();

        // Not Done
        protected abstract AbstractDataGrid ToAbstractGrid(GetResponse<TDto> response, ExportArguments args);


        // Abstract and virtual members

        /// <summary>
        /// Returns the query from which the GET endpoint retrieves the results
        /// </summary>
        protected abstract IQueryable<TModel> GetBaseQuery();

        /// <summary>
        /// Returns the query from which the GET by Id endpoint retrieves the result
        /// </summary>
        protected abstract IQueryable<TModel> SingletonQuery(IQueryable<TModel> query, TKey id);

        /// <summary>
        /// Applies the search argument, which is handled differently in every controller
        /// </summary>
        protected abstract IQueryable<TModel> Search(IQueryable<TModel> query, string search);

        /// <summary>
        /// Includes or excludes inactive items from the query depending on the boolean switch supplied
        /// </summary>
        protected abstract IQueryable<TModel> IncludeInactive(IQueryable<TModel> query, bool inactive);

        /// <summary>
        /// Persists the entities in the database, either creating them or updating them depending on the EntityState
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        protected abstract Task<List<TModel>> PersistAsync(List<TDtoForSave> entities, bool returnEntities);

        /// <summary>
        /// Returns the entities as per the specifications in the get request
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        protected virtual async Task<GetResponse<TDto>> GetImplAsync(GetArguments args)
        {
            // TODO Authorize for GET

            // Get a readonly query
            IQueryable<TModel> query = GetBaseQuery().AsNoTracking();

            // Include inactive
            query = IncludeInactive(query, inactive: args.Inactive);

            // Search
            query = Search(query, args.Search);

            // Filter
            query = Filter(query, args.Filter);

            // Before ordering or paging, retrieve the total count
            int totalCount = query.Count();

            // OrderBy
            query = OrderBy(query, args.OrderBy, args.Desc);

            // Apply the paging (Protect against DOS attacks by enforcing a maximum page size)
            var top = args.Top;
            var skip = args.Skip;
            top = Math.Min(top, MaximumPageSize());
            query = query.Skip(skip).Take(top);

            // Apply the expand, which has the general format 'Expand=A,B/C,D'
            query = Expand(query, args.Expand);

            // Load the data, transform it and wrap it in some metadata
            var memoryList = await query.ToListAsync();

            // Flatten related entities and map each to its respective DTO 
            var relatedEntities = FlattenRelatedEntities(memoryList);

            // Map the primary result to DTOs as well
            var resultData = Map(memoryList);

            // TODO apply the SELECT

            // Prepare the result in a response object
            var result = new GetResponse<TDto>
            {
                Skip = skip,
                Top = resultData.Count(),
                OrderBy = args.OrderBy,
                Desc = args.Desc,
                TotalCount = totalCount,
                Data = resultData,
                RelatedEntities = relatedEntities
            };

            // Finally return the result
            return result;
        }

        /// <summary>
        /// Saves the entities (Insert or Update) into the database after authorization and validation
        /// </summary>
        /// <returns>Optionally returns the same entities in their persisted READ form</returns>
        protected virtual async Task<List<TDto>> SaveImplAsync(List<TDtoForSave> entities, SaveArguments args)
        {
            // TODO Authorize POST

            // Validate
            await ValidateAsync(entities);
            if (!ModelState.IsValid)
            {
                throw new UnprocessableEntityException(ModelState);
            }

            // Save
            var dbEntities = await PersistAsync(entities, args.ReturnEntities);

            // Map and return
            var result = Map(dbEntities);
            return result;
        }

        /// <summary>
        /// Deletes the entities specified by the list of Ids
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        protected abstract Task DeleteImplAsync(List<TKey> ids);

        /// <summary>
        /// Performs server side validation on the entities, this method is expected to 
        /// call AddModelError on the controller's ModelState if there is a validation problem,
        /// the method should NOT do validation that is already handled by validation attributes
        /// </summary>
        protected abstract Task ValidateAsync(List<TDtoForSave> entities);

        /// <summary>
        /// Maps a list of the controller models to a list of concrete controller DTOs
        /// </summary>
        protected virtual List<TDto> Map(List<TModel> models)
        {
            return _mapper.Map<List<TDto>>(models);
        }

        /// <summary>
        /// Maps a list of any models to their corresponding DTO types, the default implementation
        /// assumes that the default DTO has been mapped to every model type in AutoMapper by mapping 
        /// it to type <see cref="DtoForSaveBase"/>
        /// </summary>
        protected virtual IEnumerable<DtoForSaveBase> MapRelatedEntities(IEnumerable<M.ModelForSaveBase> relatedEntities)
        {
            return relatedEntities.Select(e => _mapper.Map<DtoForSaveBase>(e));
        }

        /// <summary>
        /// Specifies the maximum page size to be returned by GET, defaults to <see cref="DEFAULT_MAX_PAGE_SIZE"/>
        /// </summary>
        protected virtual int MaximumPageSize()
        {
            return DEFAULT_MAX_PAGE_SIZE;
        }

        /// <summary>
        /// Filters the query based on the filter argument, the default implementation 
        /// assumes OData-like syntax
        /// </summary>
        protected virtual IQueryable<TModel> Filter(IQueryable<TModel> query, string filter)
        {
            // TODO Implement filter

            if (!string.IsNullOrWhiteSpace(filter))
            {
                // (1) Lex
                // (2) Parse
                // (3) Apply
            }

            return query;
        }

        /// <summary>
        /// Orders the query as per the orderby and desc arguments
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="query">The base query to order</param>
        /// <param name="orderby">The orderby parameter which has the format 'A/B/C'</param>
        /// <param name="desc">True for a descending order</param>
        /// <returns>Ordered query</returns>
        protected virtual IQueryable<TModel> OrderBy(IQueryable<TModel> query, string orderby, bool desc)
        {
            Type modelType = typeof(TModel);
            if (!string.IsNullOrWhiteSpace(orderby))
            {
                var steps = orderby.Split('/');

                // Validate that the steps represent a valid train of navigation properties
                {
                    PropertyInfo prop = null;
                    Type propType = modelType;
                    foreach (var step in steps)
                    {
                        prop = propType.GetProperty(step);
                        if (prop == null)
                        {
                            throw new InvalidOperationException(
                                $"The property '{step}' is not a navigation property of entity type '{propType.Name}'. " +
                                $"The orderby parameter should have the general format: 'orderby=A/B'");
                        }

                        var isCollection = prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(ICollection<>);
                        if (isCollection)
                        {
                            throw new InvalidOperationException(
                                $"The property '{step}' is a collection navigation property of type '{propType.Name}'. " +
                                $"Collection properties cannot be used in the orderby argument");
                        }

                        propType = prop.PropertyType;
                    }
                }

                // Create the key selector dynamically using LINQ expressions and apply it on the query
                {
                    var param = Expression.Parameter(modelType);
                    Expression exp = param;
                    Type propType = modelType;
                    foreach (var step in steps)
                    {
                        exp = Expression.Property(exp, propType.GetProperty(step));
                        exp = Expression.Convert(exp, typeof(object)); // To handle unboxing of e.g. int members
                    }

                    var keySelector = Expression.Lambda<Func<TModel, object>>(exp, param);

                    // Order the query taking into account the "isDescending" parameter
                    query = desc ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);
                }
            }

            return query;
        }

        /// <summary>
        /// Includes in the query all navigation properties specified in the expand parameter
        /// </summary>
        /// <param name="query">The base query on which to include related properties</param>
        /// <param name="expand">The expand parameter which has the format 'A,B/C,D''</param>
        /// <returns>Expanded query</returns>
        protected virtual IQueryable<TModel> Expand(IQueryable<TModel> query, string expand)
        {
            // Apply the expand, which has the general format 'Expand=A,B/C,D'
            if (!string.IsNullOrWhiteSpace(expand))
            {
                var paths = expand.Split(',').Select(e => e.Trim()).Where(e => !string.IsNullOrWhiteSpace(e));
                Type modelType = typeof(TModel);
                foreach (var path in paths)
                {
                    // Validate each step in the path
                    {
                        var steps = path.Split('/');
                        PropertyInfo prop = null;
                        Type propType = modelType;
                        foreach (var step in steps)
                        {
                            prop = propType.GetProperty(step);
                            if (prop == null)
                            {
                                throw new InvalidOperationException(
                                    $"The property '{step}' is not a navigation property of entity type '{propType.Name}'. " +
                                    $"The expand argument should have the general format: 'Expand=A,B/C,D'");
                            }

                            var isCollection = prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(ICollection<>);
                            propType = isCollection ? prop.PropertyType.GenericTypeArguments[0] : prop.PropertyType;
                        }
                    }

                    // Include
                    {
                        var includePath = path.Replace("/", ".");
                        query = query.Include(includePath);
                    }
                }
            }

            return query;
        }

        /// <summary>
        /// For every model in the list, the method will traverse the object graph and group all related
        /// models it can find (navigation properties) into a dictionary, after mapping them to their DTOs
        /// </summary>
        /// <param name="models"></param>
        /// <returns></returns>
        protected virtual Dictionary<string, IEnumerable<DtoForSaveBase>> FlattenRelatedEntities(List<TModel> models)
        {
            // An inline function that recursively traverses the model tree and adds all entities
            // that have a base type of Model.ModelForSaveBase to the provided HashSet
            void Flatten(M.ModelForSaveBase model, HashSet<M.ModelForSaveBase> accRelatedModels)
            {

                foreach (var prop in model.GetType().GetProperties())
                {
                    if (prop.PropertyType.IsSubclassOf(typeof(M.ModelForSaveBase)))
                    {
                        // Navigation property
                        if (prop.GetValue(model) is M.ModelForSaveBase relatedModel && !accRelatedModels.Contains(relatedModel))
                        {
                            Flatten(model, accRelatedModels);
                            accRelatedModels.Add(relatedModel);
                        }

                        // Implemtnations would have to handle navigation collections
                    }
                }
            }

            var relatedModels = new HashSet<M.ModelForSaveBase>();
            foreach (var model in models)
            {
                Flatten(model, relatedModels);
            }

            // This groups the related entities by type name, and maps them to DTO using the mapper
            var relatedEntities = relatedModels.GroupBy(e => e.GetType().Name)
                .ToDictionary(g => g.Key, g => g.Select(e => _mapper.Map<DtoForSaveBase>(e)));

            return relatedEntities;
        }

        /// <summary>
        /// Syntactic sugar that maps a collection based on the implementation of its 'list' overload
        /// </summary>
        /// <param name="models"></param>
        /// <returns></returns>
        protected TDto Map(TModel model)
        {
            return Map(new List<TModel>() { model }).Single();
        }

        /// <summary>
        /// Syntactic sugar that flattens a single model, based on the implementation of its 'list' overload
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        protected Dictionary<string, IEnumerable<DtoForSaveBase>> FlattenRelatedEntities(TModel model)
        {
            return FlattenRelatedEntities(new List<TModel> { model });
        }

        private Dictionary<(object, bool), DataTable> _dataTableCache = new Dictionary<(object, bool), DataTable>();
        /// <summary>
        /// Constructs a SQL data table containing all the public properties of the 
        /// entities' type and populates the data table with the provided entities
        /// </summary>
        protected DataTable DataTable<T>(IEnumerable<T> entities, bool addIndex = false)
        {
            if (!_dataTableCache.ContainsKey((entities, addIndex)))
            {
                DataTable table = new DataTable();
                var props = typeof(T).GetProperties();
                var columns = props.Select(p => new DataColumn(p.Name, p.PropertyType)).ToList();
                if (addIndex)
                {
                    columns.Add(new DataColumn("Index", typeof(int)));
                }
                table.Columns.AddRange(columns.ToArray());

                int index = 0;
                foreach (var entity in entities)
                {
                    DataRow row = table.NewRow();
                    foreach (var prop in props)
                    {
                        row[prop.Name] = prop.GetValue(entity);
                    }

                    // We add an index property since SQL works with un-ordered sets
                    if (addIndex)
                    {
                        row["Index"] = index++;
                    }

                    table.Rows.Add(row);
                }

                _dataTableCache[(entities, addIndex)] = table;
            }

            return _dataTableCache[(entities, addIndex)];
        }

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
            //return File(fileStream: fileStream, contentType: mimeType);

            return File(((MemoryStream)fileStream).ToArray(), contentType);
        }

        /// <summary>
        /// Syntactic sugar for localizing an error, prefixing it with "Row N: " and adding it to ModelState with an appropriate key
        /// </summary>
        protected void AddRowError(int rowNumber, string errorMessage, ModelStateDictionary modelState = null) =>
            (modelState ?? ModelState).AddModelError($"Row{rowNumber}", _localizer["Row{0}", rowNumber] + ": " + errorMessage);
        
    }
}
