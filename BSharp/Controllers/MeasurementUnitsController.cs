using BSharp.Controllers.Dto;
using BSharp.Controllers.Misc;
using BSharp.Data;
using BSharp.Data.Queries;
using BSharp.EntityModel;
using BSharp.Services.ImportExport;
using BSharp.Services.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Transactions;

namespace BSharp.Controllers
{
    [Route("api/measurement-units")]
    [ApplicationApi]
    public class MeasurementUnitsController : CrudControllerBase<MeasurementUnitForSave, MeasurementUnit, int>
    {
        private readonly IModelMetadataProvider _metadataProvider;
        private readonly ILogger<MeasurementUnitsController> _logger;
        private readonly IStringLocalizer<MeasurementUnitsController> _localizer;
        private readonly ApplicationRepository _repo;

        private string VIEW => "measurement-units";

        public MeasurementUnitsController(
            ILogger<MeasurementUnitsController> logger,
            IStringLocalizer<MeasurementUnitsController> localizer,
            ApplicationRepository repo,
            IModelMetadataProvider metadataProvider,
            IServiceProvider serviceProvider) : base(logger, localizer)
        {
            _logger = logger;
            _localizer = localizer;
            _repo = repo;
            _metadataProvider = metadataProvider;
        }

        [HttpPut("activate")]
        public async Task<ActionResult<EntitiesResponse<MeasurementUnit>>> Activate([FromBody] List<int> ids, [FromQuery] ActivateArguments args)
        {
            bool returnEntities = args.ReturnEntities ?? false;

            return await ControllerUtilities.InvokeActionImpl(() =>
                Activate(ids: ids,
                    returnEntities: returnEntities,
                    expand: args.Expand,
                    isActive: true)
            , _logger);
        }

        [HttpPut("deactivate")]
        public async Task<ActionResult<EntitiesResponse<MeasurementUnit>>> Deactivate([FromBody] List<int> ids, [FromQuery] DeactivateArguments args)
        {
            bool returnEntities = args.ReturnEntities ?? false;

            return await ControllerUtilities.InvokeActionImpl(() =>
                Activate(ids: ids,
                    returnEntities: returnEntities,
                    expand: args.Expand,
                    isActive: false)
            , _logger);
        }


        private async Task<ActionResult<EntitiesResponse<MeasurementUnit>>> Activate([FromBody] List<int> ids, bool returnEntities, string expand, bool isActive)
        {
            // Parse parameters
            var expandExp = ExpandExpression.Parse(expand);
            var idsArray = ids.ToArray();

            // Check user permissions
            await CheckActionPermissions("IsActive", idsArray);

            // Execute and return
            using (var trx = new TransactionScope(
                scopeOption: TransactionScopeOption.Required,
                transactionOptions: new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted, Timeout = GetTransactionTimeout() },
                asyncFlowOption: TransactionScopeAsyncFlowOption.Enabled))
            {
                await _repo.MeasurementUnits__Activate(ids, isActive);

                if (returnEntities)
                {
                    var response = await GetByIdListAsync(idsArray, expandExp);

                    trx.Complete();
                    return Ok(response);
                }
                else
                {
                    trx.Complete();
                    return Ok();
                }
            }
        }

        protected override async Task<IEnumerable<AbstractPermission>> UserPermissions(string action)
        {
            return await _repo.GetUserPermissions(action, VIEW);
        }

        protected override IRepository GetRepository()
        {
            return _repo;
        }

        protected override Query<MeasurementUnit> Search(Query<MeasurementUnit> query, GetArguments args, IEnumerable<AbstractPermission> filteredPermissions)
        {
            string search = args.Search;
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Replace("'", "''"); // escape quotes by repeating them

                var name = nameof(MeasurementUnit.Name);
                var name2 = nameof(MeasurementUnit.Name2);
                var name3 = nameof(MeasurementUnit.Name3);
                var code = nameof(MeasurementUnit.Code);

                var filterString = $"{name} {Ops.contains} '{search}' or {name2} {Ops.contains} '{search}' or {name3} {Ops.contains} '{search}' or {code} {Ops.contains} '{search}'";
                query.Filter(FilterExpression.Parse(filterString));
            }

            return query;
        }

        protected override async Task SaveValidateAsync(List<MeasurementUnitForSave> entities)
        {
            // C# validation
            ControllerUtilities.ValidateUniqueIds(entities, ModelState, _localizer);

            // No need to invoke SQL if the model state is full of errors
            if (ModelState.HasReachedMaxErrors)
            {
                return;
            }

            // SQL validation
            int remainingErrorCount = ModelState.MaxAllowedErrors - ModelState.ErrorCount;
            var sqlErrors = await _repo.MeasurementUnits_Validate__Save(entities, top: remainingErrorCount);

            // Add errors to model state
            ModelState.AddLocalizedErrors(sqlErrors, _localizer);
        }

        protected override async Task<List<int>> SaveExecuteAsync(List<MeasurementUnitForSave> entities, SaveArguments args)
        {
            bool returnEntities = args.ReturnEntities ?? false;
            return await _repo.MeasurementUnits__Save(entities, returnIds: returnEntities);
        }
        protected override async Task DeleteValidateAsync(List<int> ids)
        {
            // SQL validation
            int remainingErrorCount = ModelState.MaxAllowedErrors - ModelState.ErrorCount;
            var sqlErrors = await _repo.MeasurementUnits_Validate__Delete(ids, top: remainingErrorCount);

            // Add errors to model state
            ModelState.AddLocalizedErrors(sqlErrors, _localizer);
        }

        protected override async Task DeleteExecuteAsync(List<int> ids)
        {
            try
            {
                await _repo.MeasurementUnits__Delete(ids);
            }
            catch (ForeignKeyViolationException)
            {
                throw new BadRequestException(_localizer["Error_CannotDelete0AlreadyInUse", _localizer["MeasurementUnit"]]);
            }
        }

        protected override AbstractDataGrid GetImportTemplate()
        {
            // Get the properties of the DTO for Save, excluding Id or EntityState
            var type = typeof(MeasurementUnitForSave);
            var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            // The result that will be returned
            var result = new AbstractDataGrid(props.Length, 1);

            // Add the header
            var header = result[result.AddRow()];
            int i = 0;
            foreach (var prop in props)
            {
                var display = _metadataProvider.GetMetadataForProperty(type, prop.Name)?.DisplayName ?? prop.Name;
                if (display != Constants.Hidden)
                {
                    header[i++] = AbstractDataCell.Cell(display);
                }
            }

            return result;
        }

        protected override AbstractDataGrid EntitiesToAbstractGrid(GetResponse<MeasurementUnit> response, ExportArguments args)
        {
            // Get all the properties without Id and EntityState
            var type = typeof(MeasurementUnit);
            var readProps = typeof(MeasurementUnit).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            var saveProps = typeof(MeasurementUnitForSave).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            var props = saveProps.Union(readProps).ToArray();

            // The result that will be returned
            var result = new AbstractDataGrid(props.Length, response.Result.Count() + 1);

            // Add the header
            List<PropertyInfo> addedProps = new List<PropertyInfo>(props.Length);
            {
                var header = result[result.AddRow()];
                int i = 0;
                foreach (var prop in props)
                {
                    var display = _metadataProvider.GetMetadataForProperty(type, prop.Name)?.DisplayName ?? prop.Name;
                    if (display != Constants.Hidden)
                    {
                        header[i] = AbstractDataCell.Cell(display);

                        // Add the proper styling for DateTime and DateTimeOffset
                        if (prop.PropertyType.IsDateOrTime())
                        {
                            var att = prop.GetCustomAttribute<DataTypeAttribute>();
                            var isDateOnly = att != null && att.DataType == DataType.Date;
                            header[i].NumberFormat = ExportDateTimeFormat(dateOnly: isDateOnly);
                        }

                        addedProps.Add(prop);
                        i++;
                    }
                }
            }

            //// Add the rows
            //foreach (var entity in response.Ids)
            //{
            //    var metadata = entity.EntityMetadata;
            //    var row = result[result.AddRow()];
            //    int i = 0;
            //    foreach (var prop in addedProps)
            //    {
            //        metadata.TryGetValue(prop.Name, out FieldMetadata meta);
            //        if (meta == FieldMetadata.Loaded)
            //        {
            //            var content = prop.GetValue(entity);

            //            // Special handling for choice lists
            //            var choiceListAttr = prop.GetCustomAttribute<ChoiceListAttribute>();
            //            if (choiceListAttr != null)
            //            {
            //                var choiceIndex = Array.FindIndex(choiceListAttr.Choices, e => e.Equals(content));
            //                if (choiceIndex != -1)
            //                {
            //                    string displayName = choiceListAttr.DisplayNames[choiceIndex];
            //                    content = _localizer[displayName];
            //                }
            //            }

            //            // Special handling for DateTimeOffset
            //            if (prop.PropertyType.IsDateTimeOffset() && content != null)
            //            {
            //                content = ToExportDateTime((DateTimeOffset)content);
            //            }

            //            row[i] = AbstractDataCell.Cell(content);
            //        }
            //        else if (meta == FieldMetadata.Restricted)
            //        {
            //            row[i] = AbstractDataCell.Cell(Constants.Restricted);
            //        }
            //        else
            //        {
            //            row[i] = AbstractDataCell.Cell("-");
            //        }

            //        i++;
            //    }
            //}

            return result;
        }

        protected override Task<(List<MeasurementUnitForSave>, Func<string, int?>)> ToEntitiesForSave(AbstractDataGrid grid, ParseArguments args)
        {
            throw new NotImplementedException();
        }

        //protected override async Task<(List<MeasurementUnitForSave>, Func<string, int?>)> ToEntitiesForSave(AbstractDataGrid grid, ParseArguments args)
        //{
        //    // Get the properties of the DTO for Save, excluding Id or EntityState
        //    string mode = args.Mode;
        //    var readType = typeof(MeasurementUnit);
        //    var saveType = typeof(MeasurementUnitForSave);

        //    var readProps = readType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
        //        .ToDictionary(prop => _metadataProvider.GetMetadataForProperty(readType, prop.Name)?.DisplayName ?? prop.Name, StringComparer.InvariantCultureIgnoreCase);

        //    var saveProps = saveType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
        //        .ToDictionary(prop => _metadataProvider.GetMetadataForProperty(saveType, prop.Name)?.DisplayName ?? prop.Name, StringComparer.InvariantCultureIgnoreCase);

        //    // Maps the index of the grid column to a property on the DtoForSave
        //    var saveColumnMap = new List<(int Index, PropertyInfo Property)>(grid.RowSize);

        //    // Make sure all column header labels are recognizable
        //    // and construct the save column map
        //    var firstRow = grid[0];
        //    for (int c = 0; c < firstRow.Length; c++)
        //    {
        //        var column = firstRow[c];
        //        string headerLabel = column.Content?.ToString();

        //        // So any thing after an empty column is ignored
        //        if (string.IsNullOrWhiteSpace(headerLabel))
        //            break;

        //        if (saveProps.ContainsKey(headerLabel))
        //        {
        //            var prop = saveProps[headerLabel];
        //            saveColumnMap.Add((c, prop));
        //        }
        //        else if (readProps.ContainsKey(headerLabel))
        //        {
        //            // All good, just ignore
        //        }
        //        else
        //        {
        //            AddRowError(1, _localizer["Error_Column0NotRecognizable", headerLabel]);
        //        }
        //    }

        //    // Milestone 1: columns in the abstract grid mapped
        //    if (!ModelState.IsValid)
        //    {
        //        throw new UnprocessableEntityException(ModelState);
        //    }

        //    // Construct the result using the map generated earlier
        //    List<MeasurementUnitForSave> result = new List<MeasurementUnitForSave>(grid.Count - 1);
        //    for (int i = 1; i < grid.Count; i++) // Skip the header
        //    {
        //        var row = grid[i];

        //        // Anything after an empty row is ignored
        //        if (saveColumnMap.All((p) => string.IsNullOrWhiteSpace(row[p.Index].Content?.ToString())))
        //        {
        //            break;
        //        }

        //        var entity = new MeasurementUnitForSave();
        //        foreach (var (index, prop) in saveColumnMap)
        //        {
        //            var content = row[index].Content;
        //            var propName = _metadataProvider.GetMetadataForProperty(readType, prop.Name).DisplayName;

        //            // Special handling for choice lists
        //            if (content != null)
        //            {
        //                var choiceListAttr = prop.GetCustomAttribute<ChoiceListAttribute>();
        //                if (choiceListAttr != null)
        //                {
        //                    List<string> displayNames = choiceListAttr.DisplayNames.Select(e => _localizer[e].Value).ToList();
        //                    string stringContent = content.ToString();
        //                    var displayNameIndex = displayNames.IndexOf(stringContent);
        //                    if (displayNameIndex == -1)
        //                    {
        //                        string seperator = _localizer[", "];
        //                        AddRowError(i + 1, _localizer["Error_Value0IsNotValidFor1AcceptableValuesAre2", stringContent, propName, string.Join(seperator, displayNames)]);
        //                    }
        //                    else
        //                    {
        //                        content = choiceListAttr.Choices[displayNameIndex];
        //                    }
        //                }
        //            }

        //            // Special handling for DateTime and DateTimeOffset
        //            if (prop.PropertyType.IsDateOrTime())
        //            {
        //                try
        //                {
        //                    var date = ParseImportedDateTime(content);
        //                    content = date;

        //                    if (prop.PropertyType.IsDateTimeOffset())
        //                    {
        //                        content = AddUserTimeZone(date);
        //                    }
        //                }
        //                catch (Exception)
        //                {
        //                    AddRowError(i + 1, _localizer["Error_TheValue0IsNotValidFor1Field", content?.ToString(), propName]);
        //                }
        //            }

        //            try
        //            {
        //                prop.SetValue(entity, content); // TODO casting here to be done
        //            }
        //            catch (ArgumentException)
        //            {
        //                AddRowError(i + 1, _localizer["Error_TheValue0IsNotValidFor1Field", content?.ToString(), propName]);
        //            }
        //        }

        //        result.Add(entity);
        //    }

        //    // Milestone 2: DTOs created
        //    if (!ModelState.IsValid)
        //    {
        //        throw new UnprocessableEntityException(ModelState);
        //    }

        //    // Prepare a dictionary of indices in order to construct any validation errors performantly
        //    // "IndexOf" is O(n), this brings it down to O(1)
        //    Dictionary<MeasurementUnitForSave, int> indicesDic = result.ToIndexDictionary();

        //    // For each entity, set the Id and EntityState depending on import mode
        //    if (mode == "Insert")
        //    {
        //        // For Insert mode, all are marked inserted and all Ids are null
        //        // Any duplicate codes will be handled later in the validation
        //        result.ForEach(e => e.Id = null);
        //        result.ForEach(e => e.EntityState = EntityStates.Inserted);
        //    }
        //    else
        //    {
        //        // For all other modes besides Insert, we need to match the entity codes to Ids by querying the DB
        //        // Load the code Ids from the database
        //        var nonNullCodes = result.Where(e => !string.IsNullOrWhiteSpace(e.Code));
        //        var codesDataTable = ControllerUtilities.DataTable(nonNullCodes.Select(e => new { e.Code }));
        //        var entitiesTvp = new SqlParameter("@Codes", codesDataTable)
        //        {
        //            TypeName = $"dbo.CodeList",
        //            SqlDbType = SqlDbType.Structured
        //        };

        //        string sql = $@"SELECT c.Code, e.Id FROM @Codes c JOIN [dbo].[MeasurementUnits] e ON c.Code = e.Code WHERE e.UnitType <> 'Money';";
        //        var idCodesDic = await _db.CodeIds.FromSql(sql, entitiesTvp).ToDictionaryAsync(e => e.Code, e => e.Id);

        //        result.ForEach(e =>
        //        {
        //            if (!string.IsNullOrWhiteSpace(e.Code) && idCodesDic.ContainsKey(e.Code))
        //            {
        //                e.Id = idCodesDic[e.Code];
        //            }
        //            else
        //            {
        //                e.Id = null;
        //            }
        //        });

        //        // Make sure no codes are mentioned twice, if we don't do it here, the save validation later will complain
        //        // about duplicated Id, but the error will not be clear since user deals with code while importing from Excel              
        //        var duplicateIdGroups = result.Where(e => e.Id != null).GroupBy(e => e.Id.Value).Where(g => g.Count() > 1);
        //        foreach (var duplicateIdGroup in duplicateIdGroups)
        //        {
        //            foreach (var entity in duplicateIdGroup)
        //            {
        //                int index = indicesDic[entity];
        //                AddRowError(index + 2, _localizer["Error_TheCode0IsDuplicated", entity.Code]);
        //            }
        //        }

        //        if (mode == "Merge")
        //        {
        //            // Merge simply inserts codes that are not found, and updates codes that are found
        //            result.ForEach(e =>
        //            {
        //                if (e.Id != null)
        //                {
        //                    e.EntityState = EntityStates.Updated;
        //                }
        //                else
        //                {
        //                    e.EntityState = EntityStates.Inserted;
        //                }
        //            });
        //        }
        //        else
        //        {
        //            // In the case of update: codes are required, and MUST match database Ids
        //            if (mode == "Update")
        //            {
        //                for (int index = 0; index < result.Count; index++)
        //                {
        //                    var entity = result[index];
        //                    if (string.IsNullOrWhiteSpace(entity.Code))
        //                    {
        //                        AddRowError(index + 2, _localizer["Error_CodeIsRequiredForImportModeUpdate"]);
        //                    }
        //                    else if (entity.Id == null)
        //                    {
        //                        AddRowError(index + 2, _localizer["Error_TheCode0DoesNotExist", entity.Code]);
        //                    }
        //                }

        //                result.ForEach(e => e.EntityState = EntityStates.Updated);
        //            }
        //            else
        //            {
        //                throw new InvalidOperationException("Unknown save mode"); // Developer bug
        //            }
        //        }
        //    }

        //    // Milestone 3: Id and EntityState are set
        //    if (!ModelState.IsValid)
        //    {
        //        throw new UnprocessableEntityException(ModelState);
        //    }

        //    // Function maps any future validation errors back to specific rows
        //    int? errorKeyMap(string key)
        //    {
        //        int? rowNumber = null;
        //        if (key != null && key.StartsWith("["))
        //        {
        //            var indexStr = key.TrimStart('[').Split(']')[0];
        //            if (int.TryParse(indexStr, out int index))
        //            {
        //                // Add 2:
        //                // 1 for the header in the abstract grid
        //                // 1 for the difference between index and number
        //                rowNumber = index + 2;
        //            }
        //        }
        //        return rowNumber;
        //    }

        //    return (result, errorKeyMap);
        //}

        protected override (string PreambleSql, string ComposableSql, List<SqlParameter> Parameters) GetAsSql(IEnumerable<MeasurementUnitForSave> entities)
        {
            // Preamble SQL
            string preambleSql =
                $@"DECLARE @TenantId int = CONVERT(INT, SESSION_CONTEXT(N'TenantId'));
            	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();
            	DECLARE @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));
            	DECLARE @True BIT = 1;";

            // Composable SQL
            string sql =
                $@"SELECT  @TenantId AS TenantId, ISNULL(E.Id, 0) AS Id, E.Name, E.Name2, E.Name3, E.Code, E.UnitType, E.UnitAmount, E.BaseAmount,
                @True AS IsActive, @Now AS CreatedAt, @UserId AS CreatedById, @Now AS ModifiedAt, @UserId AS ModifiedById 
                FROM @Entities E";

            // Entities TVP put in a singleton
            DataTable entitiesTable = ControllerUtilities.DataTable(entities, addIndex: true);
            var entitiesTvp = new SqlParameter("Entities", entitiesTable)
            {
                TypeName = $"dbo.{nameof(MeasurementUnitForSave)}List",
                SqlDbType = SqlDbType.Structured
            };

            var ps = new List<SqlParameter>() { entitiesTvp };

            // Return the result
            return (preambleSql, sql, ps);
        }
    }
}
