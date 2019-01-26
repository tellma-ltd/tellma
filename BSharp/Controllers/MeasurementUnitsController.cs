using AutoMapper;
using BSharp.Controllers.DTO;
using BSharp.Controllers.Misc;
using BSharp.Data;
using BSharp.Services.Identity;
using BSharp.Services.ImportExport;
using BSharp.Services.Utilities;
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
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using M = BSharp.Data.Model;

namespace BSharp.Controllers
{
    [Route("api/measurement-units")]
    public class MeasurementUnitsController : CrudControllerBase<M.MeasurementUnit, MeasurementUnit, MeasurementUnitForSave, int?>
    {
        private readonly ApplicationContext _db;
        private readonly IModelMetadataProvider _metadataProvider;
        private readonly ILogger<MeasurementUnitsController> _logger;
        private readonly IStringLocalizer<MeasurementUnitsController> _localizer;
        private readonly IMapper _mapper;

        public MeasurementUnitsController(ApplicationContext db, IModelMetadataProvider metadataProvider, ILogger<MeasurementUnitsController> logger,
            IStringLocalizer<MeasurementUnitsController> localizer, IMapper mapper, IUserService userService) : base(logger, localizer, mapper, userService)
        {
            _db = db;
            _metadataProvider = metadataProvider;
            _logger = logger;
            _localizer = localizer;
            _mapper = mapper;
        }

        [HttpPut("activate")]
        public async Task<ActionResult<EntitiesResponse<MeasurementUnit>>> Activate([FromBody] List<int> ids, [FromQuery] ActivateArguments<int> args)
        {
            return await ActivateDeactivate(ids, args.ReturnEntities ?? false, args.Expand, isActive: true);
        }

        [HttpPut("deactivate")]
        public async Task<ActionResult<EntitiesResponse<MeasurementUnit>>> Deactivate([FromBody] List<int> ids, [FromQuery] DeactivateArguments<int> args)
        {
            return await ActivateDeactivate(ids, args.ReturnEntities ?? false, args.Expand, isActive: false);
        }

        private async Task<ActionResult<EntitiesResponse<MeasurementUnit>>> ActivateDeactivate([FromBody] List<int> ids, bool returnEntities, string expand, bool isActive)
        {
            using (var trx = await _db.Database.BeginTransactionAsync())
            {
                try
                {
                    // TODO Authorize Activate

                    // TODO Validate (No used units, no duplicate Ids, no missing Ids?)

                    var isActiveParam = new SqlParameter("@IsActive", isActive);

                    DataTable idsTable = DataTable(ids.Select(id => new { Id = id }));
                    var idsTvp = new SqlParameter("@Ids", idsTable)
                    {
                        TypeName = $"dbo.IdList",
                        SqlDbType = SqlDbType.Structured
                    };

                    string sql = @"
DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();
DECLARE @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));

MERGE INTO [dbo].MeasurementUnits AS t
	USING (
		SELECT [Id]
		FROM @Ids
	) AS s ON (t.Id = s.Id)
	WHEN MATCHED AND (t.IsActive <> @IsActive)
	THEN
		UPDATE SET 
			t.[IsActive]	= @IsActive,
			t.[ModifiedAt]	= @Now,
			t.[ModifiedById]= @UserId;
";

                    // Update the entities
                    await _db.Database.ExecuteSqlCommandAsync(sql, idsTvp, isActiveParam);

                    // Determine whether entities should be returned
                    if (!returnEntities)
                    {
                        // IF no returned items are expected, simply return 200 OK
                        return Ok();
                    }
                    else
                    {
                        // Load the entities using their Ids
                        var affectedDbEntitiesQ = _db.MeasurementUnits.FromSql("SELECT * FROM [dbo].[MeasurementUnits] WHERE Id IN (SELECT Id FROM @Ids)", idsTvp);
                        var affectedDbEntitiesExpandedQ = Expand(affectedDbEntitiesQ, expand);
                        var affectedDbEntities = await affectedDbEntitiesExpandedQ.ToListAsync();
                        var affectedEntities = _mapper.Map<List<MeasurementUnit>>(affectedDbEntities);                        

                        // sort the entities the way their Ids came, as a good practice
                        MeasurementUnit[] sortedAffectedEntities = new MeasurementUnit[ids.Count];
                        Dictionary<int, MeasurementUnit> affectedEntitiesDic = affectedEntities.ToDictionary(e => e.Id.Value);
                        for (int i = 0; i < ids.Count; i++)
                        {
                            var id = ids[i];
                            MeasurementUnit entity = null;
                            if (affectedEntitiesDic.ContainsKey(id))
                            {
                                entity = affectedEntitiesDic[id];
                            }

                            sortedAffectedEntities[i] = entity;
                        }

                        // Prepare a proper response
                        var response = new EntitiesResponse<MeasurementUnit> {
                            Data = sortedAffectedEntities,
                            CollectionName = GetCollectionName(typeof(MeasurementUnit))
                        };

                        // Commit and return
                        trx.Commit();
                        return Ok(response);
                    }
                }
                catch (Exception ex)
                {
                    trx.Rollback();
                    _logger.LogError($"Error: {ex.Message} {ex.StackTrace}");
                    return BadRequest(ex.Message);
                }
            }
        }

        protected override async Task<IDbContextTransaction> BeginSaveTransaction()
        {
            return await _db.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);
        }

        protected override IQueryable<M.MeasurementUnit> GetBaseQuery()
        {
            return _db.MeasurementUnits.Where(e => e.UnitType != "Money");
        }

        protected override IQueryable<M.MeasurementUnit> Search(IQueryable<M.MeasurementUnit> query, string search)
        {
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(e => e.Name.Contains(search) || e.Name2.Contains(search) || e.Code.Contains(search));
            }

            return query;
        }

        protected override IQueryable<M.MeasurementUnit> SingletonQuery(IQueryable<M.MeasurementUnit> query, int? id)
        {
            return query.Where(e => e.Id == id);
        }

        protected override IQueryable<M.MeasurementUnit> IncludeInactive(IQueryable<M.MeasurementUnit> query, bool inactive)
        {
            if (!inactive)
            {
                query = query.Where(e => e.IsActive);
            }

            return query;
        }

        protected override async Task ValidateAsync(List<MeasurementUnitForSave> entities)
        {
            // Hash the indices for performance
            var indices = entities.ToIndexDictionary();

            // Check that Ids make sense in relation to EntityState, and that no entity is DELETED
            // All these errors indicate a bug
            foreach (var entity in entities)
            {
                if (entity.EntityState == EntityStates.Deleted)
                {
                    // Won't be supported for this API
                    var index = indices[entity];
                    ModelState.AddModelError($"[{index}].{nameof(entity.EntityState)}", _localizer["Error_Deleting0IsNotSupportedFromThisAPI", _localizer["MeasurementUnits"]]);
                }

                if (entity.Id != null && entity.EntityState != EntityStates.Updated)
                {
                    // This error indicates a bug
                    var index = indices[entity];
                    ModelState.AddModelError($"[{index}].{nameof(entity.Id)}", _localizer["Error_CannotInsert0WhileSpecifyId", _localizer["MeasurementUnit"]]);
                }

                if (entity.Id == null && entity.EntityState == EntityStates.Updated)
                {
                    // This error indicates a bug
                    var index = indices[entity];
                    ModelState.AddModelError($"[{index}].{nameof(entity.Id)}", _localizer["Error_CannotUpdate0WithoutId", _localizer["MeasurementUnit"]]);
                }
            }

            // Check that Ids are unique
            var duplicateIds = entities.Where(e => e.Id != null).GroupBy(e => e.Id.Value).Where(g => g.Count() > 1);
            foreach (var groupWithDuplicateIds in duplicateIds)
            {
                foreach (var entity in groupWithDuplicateIds)
                {
                    // This error indicates a bug
                    var index = indices[entity];
                    ModelState.AddModelError($"[{index}].{nameof(entity.Id)}", _localizer["Error_TheEntityWithId0IsSpecifiedMoreThanOnce", entity.Id]);
                }
            }

            // No need to invoke SQL if the model state is full of errors
            if (ModelState.HasReachedMaxErrors)
            {
                return;
            }

            // Perform SQL-side validation
            DataTable entitiesTable = DataTable(entities, addIndex: true);
            var entitiesTvp = new SqlParameter("Entities", entitiesTable) { TypeName = $"dbo.{nameof(MeasurementUnitForSave)}List", SqlDbType = SqlDbType.Structured };
            int remainingErrorCount = ModelState.MaxAllowedErrors - ModelState.ErrorCount;

            // Code, Name and Name2 must be unique
            var sqlErrors = await _db.Validation.FromSql($@"
SET NOCOUNT ON;
DECLARE @ValidationErrors dbo.ValidationErrorList;

    -- Non Null Ids must exist
    INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument1])
    SELECT '[' + CAST([Id] AS NVARCHAR(255)) + '].Id' As [Key], N'Error_TheId0WasNotFound' As [ErrorName], CAST([Id] As NVARCHAR(255)) As [Argument1]
    FROM @Entities
    WHERE Id Is NOT NULL AND Id NOT IN (SELECT Id from [dbo].[MeasurementUnits])
    
	-- Code must be unique
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument1], [Argument2], [Argument3], [Argument4], [Argument5]) 
	SELECT '[' + CAST(FE.[Index] AS NVARCHAR(255)) + '].Code' As [Key], N'Error_TheCode0IsUsed' As [ErrorName],
		FE.Code AS Argument1, NULL AS Argument2, NULL AS Argument3, NULL AS Argument4, NULL AS Argument5
	FROM @Entities FE 
	JOIN [dbo].MeasurementUnits BE ON FE.Code = BE.Code
	WHERE FE.[Code] IS NOT NULL
	AND (FE.[EntityState] = N'Inserted') OR (FE.Id <> BE.Id) OPTION(HASH JOIN);

	-- Code must not be duplicated in the uploaded list
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument1], [Argument2], [Argument3], [Argument4], [Argument5]) 
	SELECT '[' + CAST([Index] AS NVARCHAR(255)) + '].Code' As [Key], N'Error_TheCode0IsDuplicated' As [ErrorName],
		[Code] AS Argument1, NULL AS Argument2, NULL AS Argument3, NULL AS Argument4, NULL AS Argument5
	FROM @Entities
	WHERE [Code] IN (
		SELECT [Code]
		FROM @Entities
		WHERE [Code] IS NOT NULL
		GROUP BY [Code]
		HAVING COUNT(*) > 1
	) OPTION(HASH JOIN);

	-- Name must not exist already
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument1], [Argument2], [Argument3], [Argument4], [Argument5]) 
	SELECT '[' + CAST(FE.[Index] AS NVARCHAR(255)) + '].Name' As [Key], N'Error_TheName0IsUsed' As [ErrorName],
		FE.[Name] AS Argument1, NULL AS Argument2, NULL AS Argument3, NULL AS Argument4, NULL AS Argument5
	FROM @Entities FE 
	JOIN [dbo].MeasurementUnits BE ON FE.[Name] = BE.[Name]
	WHERE (FE.[EntityState] = N'Inserted') OR (FE.Id <> BE.Id) OPTION(HASH JOIN);

	-- Name must be unique in the uploaded list
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument1], [Argument2], [Argument3], [Argument4], [Argument5]) 
	SELECT '[' + CAST([Index] AS NVARCHAR(255)) + '].Name' As [Key], N'Error_TheName0IsDuplicated' As [ErrorName],
		[Name] AS Argument1, NULL AS Argument2, NULL AS Argument3, NULL AS Argument4, NULL AS Argument5
	FROM @Entities
	WHERE [Name] IN (
		SELECT [Name]
		FROM @Entities
		GROUP BY [Name]
		HAVING COUNT(*) > 1
	) OPTION(HASH JOIN);

	-- Name2 must not exist already
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument1], [Argument2], [Argument3], [Argument4], [Argument5]) 
	SELECT '[' + CAST(FE.[Index] AS NVARCHAR(255)) + '].Name2' As [Key], N'Error_TheName0IsUsed' As [ErrorName],
		FE.[Name2] AS Argument1, NULL AS Argument2, NULL AS Argument3, NULL AS Argument4, NULL AS Argument5
	FROM @Entities FE 
	JOIN [dbo].MeasurementUnits BE ON FE.[Name2] = BE.[Name2]
	WHERE (FE.[EntityState] = N'Inserted') OR (FE.Id <> BE.Id) OPTION(HASH JOIN);

	-- Name2 must be unique in the uploaded list
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument1], [Argument2], [Argument3], [Argument4], [Argument5]) 
	SELECT '[' + CAST([Index] AS NVARCHAR(255)) + '].Name2' As [Key], N'Error_TheName0IsDuplicated' As [ErrorName],
		[Name2] AS Argument1, NULL AS Argument2, NULL AS Argument3, NULL AS Argument4, NULL AS Argument5
	FROM @Entities
	WHERE [Name2] IN (
		SELECT [Name2]
		FROM @Entities
		GROUP BY [Name2]
		HAVING COUNT(*) > 1
	) OPTION(HASH JOIN);
    -- Add further logic

SELECT TOP {remainingErrorCount} * FROM @ValidationErrors;
", entitiesTvp).ToListAsync();

            // Loop over the errors returned from SQL and add them to ModelState
            foreach (var sqlError in sqlErrors)
            {
                var formatArguments = sqlError.ToFormatArguments();

                string key = sqlError.Key;
                string errorMessage = _localizer[sqlError.ErrorName, formatArguments];

                ModelState.AddModelError(key: key, errorMessage: errorMessage);
            }
        }

        protected override async Task<List<M.MeasurementUnit>> PersistAsync(List<MeasurementUnitForSave> entities, SaveArguments args)
        {
            // Add created entities
            DataTable entitiesTable = DataTable(entities, addIndex: true);
            var entitiesTvp = new SqlParameter("Entities", entitiesTable)
            {
                TypeName = $"dbo.{nameof(MeasurementUnitForSave)}List",
                SqlDbType = SqlDbType.Structured
            };

            string saveSql = $@"
-- Procedure: MeasurementUnitsSave
SET NOCOUNT ON;
	DECLARE @IndexedIds [dbo].[IndexedIdList];
	DECLARE @TenantId int = CONVERT(INT, SESSION_CONTEXT(N'TenantId'));
	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();
	DECLARE @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));

	INSERT INTO @IndexedIds([Index], [Id])
	SELECT x.[Index], x.[Id]
	FROM
	(
		MERGE INTO [dbo].MeasurementUnits AS t
		USING (
			SELECT [Index], [Id], [Code], [UnitType], [Name], [Name2], [UnitAmount], [BaseAmount]
			FROM @Entities 
			WHERE [EntityState] IN (N'Inserted', N'Updated')
		) AS s ON (t.Id = s.Id)
		WHEN MATCHED 
		THEN
			UPDATE SET 
				t.[UnitType]	= s.[UnitType],
				t.[Name]		= s.[Name],
				t.[Name2]		= s.[Name2],
				t.[UnitAmount]	= s.[UnitAmount],
				t.[BaseAmount]	= s.[BaseAmount],
				t.[Code]		= s.[Code],
				t.[ModifiedAt]	= @Now,
				t.[ModifiedById]	= @UserId
		WHEN NOT MATCHED THEN
				INSERT ([TenantId], [UnitType], [Name], [Name2], [UnitAmount], [BaseAmount], [Code], [CreatedAt], [CreatedById], [ModifiedAt], [ModifiedById])
				VALUES (@TenantId, s.[UnitType], s.[Name], s.[Name2], s.[UnitAmount], s.[BaseAmount], s.[Code], @Now, @UserId, @Now, @UserId)
			OUTPUT s.[Index], inserted.[Id] 
	) As x
    OPTION(RECOMPILE)
";
            // Optimization
            if (!(args.ReturnEntities ?? false))
            {
                // IF no returned items are expected, simply execute a non-Query and return an empty list;
                await _db.Database.ExecuteSqlCommandAsync(saveSql, entitiesTvp);
                return new List<M.MeasurementUnit>();
            }
            else
            {
                // If returned items are expected, append a select statement to the SQL command
                saveSql = saveSql += "SELECT * FROM @IndexedIds;";

                // Retrieve the map from Indexes to Ids
                var indexedIds = await _db.Saving.FromSql(saveSql, entitiesTvp).ToListAsync();

                // Load the entities using their Ids
                DataTable idsTable = DataTable(indexedIds.Select(e => new { e.Id }), addIndex: false);
                var idsTvp = new SqlParameter("Ids", idsTable)
                {
                    TypeName = $"dbo.IdList",
                    SqlDbType = SqlDbType.Structured
                };

                var q = _db.MeasurementUnits.FromSql("SELECT * FROM dbo.[MeasurementUnits] WHERE Id IN (SELECT Id FROM @Ids)", idsTvp);
                q = Expand(q, args.Expand);
                var savedEntities = await q.ToListAsync();


                // SQL Server does not guarantee order, so make sure the result is sorted according to the initial index
                Dictionary<int, int> indices = indexedIds.ToDictionary(e => e.Id, e => e.Index);
                var sortedSavedEntities = new M.MeasurementUnit[savedEntities.Count];
                foreach (var item in savedEntities)
                {
                    int index = indices[item.Id];
                    sortedSavedEntities[index] = item;
                }

                // Return the sorted collection
                return sortedSavedEntities.ToList();
            }
        }

        protected override async Task DeleteImplAsync(List<int?> ids)
        {
            // Prepare a list of Ids to delete
            DataTable idsTable = DataTable(ids.Select(e => new { Id = e }), addIndex: false);
            var idsTvp = new SqlParameter("Ids", idsTable)
            {
                TypeName = $"dbo.IdList",
                SqlDbType = SqlDbType.Structured
            };

            using (var trx = await _db.Database.BeginTransactionAsync())
            {
                try
                {
                    // Delete efficiently with a SQL query
                    await _db.Database.ExecuteSqlCommandAsync("DELETE FROM dbo.[MeasurementUnits] WHERE Id IN (SELECT Id FROM @Ids)", idsTvp);

                    // Commit and return
                    trx.Commit();
                    return;
                }
                catch (SqlException ex) when (IsForeignKeyViolation(ex))
                {
                    throw new BadRequestException(_localizer["Error_CannotDelete0AlreadyInUse", _localizer["MeasurementUnit"]]);
                }
                catch (Exception ex)
                {
                    trx.Rollback();
                    throw ex;
                }
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
            for (int i = 0; i < props.Length; i++)
            {
                var prop = props[i];
                var display = _metadataProvider.GetMetadataForProperty(type, prop.Name)?.DisplayName ?? prop.Name;
                // var display = _localizer["MeasurementUnit_Code"].Value;
                header[i] = AbstractDataCell.Cell(display);
            }

            return result;
        }

        protected override AbstractDataGrid DtosToAbstractGrid(GetResponse<MeasurementUnit> response, ExportArguments args)
        {
            // Get all the properties without Id and EntityState
            var type = typeof(MeasurementUnit);
            var readProps = typeof(MeasurementUnit).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            var saveProps = typeof(MeasurementUnitForSave).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            var props = saveProps.Union(readProps).ToArray();

            // The result that will be returned
            var result = new AbstractDataGrid(props.Length, response.Data.Count() + 1);

            // Add the header
            var header = result[result.AddRow()];
            for (int i = 0; i < props.Length; i++)
            {
                var prop = props[i];
                var display = _metadataProvider.GetMetadataForProperty(type, prop.Name)?.DisplayName ?? prop.Name;

                header[i] = AbstractDataCell.Cell(display);

                // Add the proper styling for DateTime and DateTimeOffset
                if (prop.PropertyType.IsDateOrTime())
                {
                    var att = prop.GetCustomAttribute<DataTypeAttribute>();
                    var isDateOnly = att != null && att.DataType == DataType.Date;
                    header[i].NumberFormat = ExportDateTimeFormat(dateOnly: isDateOnly);
                }
            }


            // Add the rows
            foreach (var entity in response.Data)
            {
                var row = result[result.AddRow()];
                for (int i = 0; i < props.Length; i++)
                {
                    var prop = props[i];
                    var content = prop.GetValue(entity);

                    // Special handling for choice lists
                    var choiceListAttr = prop.GetCustomAttribute<ChoiceListAttribute>();
                    if (choiceListAttr != null)
                    {
                        var choiceIndex = Array.FindIndex(choiceListAttr.Choices, e => e.Equals(content));
                        if (choiceIndex != -1)
                        {
                            string displayName = choiceListAttr.DisplayNames[choiceIndex];
                            content = _localizer[displayName];
                        }
                    }

                    // Special handling for DateTimeOffset
                    if (prop.PropertyType.IsDateTimeOffset() && content != null)
                    {
                        content = ToExportDateTime((DateTimeOffset)content);
                    }

                    row[i] = AbstractDataCell.Cell(content);
                }
            }

            return result;
        }

        protected override async Task<(List<MeasurementUnitForSave>, Func<string, int?>)> ToDtosForSave(AbstractDataGrid grid, ParseArguments args)
        {
            // Get the properties of the DTO for Save, excluding Id or EntityState
            string mode = args.Mode;
            var readType = typeof(MeasurementUnit);
            var saveType = typeof(MeasurementUnitForSave);

            var readProps = readType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .ToDictionary(prop => _metadataProvider.GetMetadataForProperty(readType, prop.Name)?.DisplayName ?? prop.Name, StringComparer.InvariantCultureIgnoreCase);

            var saveProps = saveType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .ToDictionary(prop => _metadataProvider.GetMetadataForProperty(saveType, prop.Name)?.DisplayName ?? prop.Name, StringComparer.InvariantCultureIgnoreCase);

            // Maps the index of the grid column to a property on the DtoForSave
            var saveColumnMap = new List<(int Index, PropertyInfo Property)>(grid.RowSize);

            // Make sure all column header labels are recognizable
            // and construct the save column map
            var firstRow = grid[0];
            for (int c = 0; c < firstRow.Length; c++)
            {
                var column = firstRow[c];
                string headerLabel = column.Content?.ToString();

                // So any thing after an empty column is ignored
                if (string.IsNullOrWhiteSpace(headerLabel))
                    break;

                if (saveProps.ContainsKey(headerLabel))
                {
                    var prop = saveProps[headerLabel];
                    saveColumnMap.Add((c, prop));
                }
                else if (readProps.ContainsKey(headerLabel))
                {
                    // All good, just ignore
                }
                else
                {
                    AddRowError(1, _localizer["Error_Column0NotRecognizable", headerLabel]);
                }
            }

            // Milestone 1: columns in the abstract grid mapped
            if (!ModelState.IsValid)
            {
                throw new UnprocessableEntityException(ModelState);
            }

            // Construct the result using the map generated earlier
            List<MeasurementUnitForSave> result = new List<MeasurementUnitForSave>(grid.Count - 1);
            for (int i = 1; i < grid.Count; i++) // Skip the header
            {
                var row = grid[i];

                // Anything after an empty row is ignored
                if (saveColumnMap.All((p) => string.IsNullOrWhiteSpace(row[p.Index].Content?.ToString())))
                {
                    break;
                }

                var entity = new MeasurementUnitForSave();
                foreach (var (index, prop) in saveColumnMap)
                {
                    var content = row[index].Content;
                    var propName = _metadataProvider.GetMetadataForProperty(readType, prop.Name).DisplayName;

                    // Special handling for choice lists
                    if (content != null)
                    {
                        var choiceListAttr = prop.GetCustomAttribute<ChoiceListAttribute>();
                        if (choiceListAttr != null)
                        {
                            List<string> displayNames = choiceListAttr.DisplayNames.Select(e => _localizer[e].Value).ToList();
                            string stringContent = content.ToString();
                            var displayNameIndex = displayNames.IndexOf(stringContent);
                            if (displayNameIndex == -1)
                            {
                                string seperator = _localizer[", "];
                                AddRowError(i + 1, _localizer["Error_Value0IsNotValidFor1AcceptableValuesAre2", stringContent, propName, string.Join(seperator, displayNames)]);
                            }
                            else
                            {
                                content = choiceListAttr.Choices[displayNameIndex];
                            }
                        }
                    }

                    // Special handling for DateTime and DateTimeOffset
                    if (prop.PropertyType.IsDateOrTime())
                    {
                        try
                        {
                            var date = ParseImportedDateTime(content);
                            content = date;

                            if (prop.PropertyType.IsDateTimeOffset())
                            {
                                content = AddUserTimeZone(date);
                            }
                        }
                        catch (Exception)
                        {
                            AddRowError(i + 1, _localizer["Error_TheValue0IsNotValidFor1Field", content?.ToString(), propName]);
                        }
                    }

                    try
                    {
                        prop.SetValue(entity, content); // TODO casting here to be done
                    }
                    catch (ArgumentException)
                    {
                        AddRowError(i + 1, _localizer["Error_TheValue0IsNotValidFor1Field", content?.ToString(), propName]);
                    }
                }

                result.Add(entity);
            }

            // Milestone 2: DTOs created
            if (!ModelState.IsValid)
            {
                throw new UnprocessableEntityException(ModelState);
            }

            // Prepare a dictionary of indices in order to construct any validation errors performantly
            // "IndexOf" is O(n), this brings it down to O(1)
            Dictionary<MeasurementUnitForSave, int> indicesDic = result.ToIndexDictionary();

            // For each entity, set the Id and EntityState depending on import mode
            if (mode == "Insert")
            {
                // For Insert mode, all are marked inserted and all Ids are null
                // Any duplicate codes will be handled later in the validation
                result.ForEach(e => e.Id = null);
                result.ForEach(e => e.EntityState = EntityStates.Inserted);
            }
            else
            {
                // For all other modes besides Insert, we need to match the entity codes to Ids by querying the DB
                // Load the code Ids from the database
                var nonNullCodes = result.Where(e => !string.IsNullOrWhiteSpace(e.Code));
                var codesDataTable = DataTable(nonNullCodes.Select(e => new { e.Code }));
                var entitiesTvp = new SqlParameter("@Codes", codesDataTable)
                {
                    TypeName = $"dbo.CodeList",
                    SqlDbType = SqlDbType.Structured
                };

                string sql = $@"SELECT c.Code, e.Id FROM @Codes c JOIN [dbo].[MeasurementUnits] e ON c.Code = e.Code WHERE e.UnitType <> 'Money';";
                var idCodesDic = await _db.CodeIds.FromSql(sql, entitiesTvp).ToDictionaryAsync(e => e.Code, e => e.Id);

                result.ForEach(e =>
                {
                    if (!string.IsNullOrWhiteSpace(e.Code) && idCodesDic.ContainsKey(e.Code))
                    {
                        e.Id = idCodesDic[e.Code];
                    }
                    else
                    {
                        e.Id = null;
                    }
                });

                // Make sure no codes are mentioned twice, if we don't do it here, the save validation later will complain
                // about duplicated Id, but the error will not be clear since user deals with code while importing from Excel              
                var duplicateIdGroups = result.Where(e => e.Id != null).GroupBy(e => e.Id.Value).Where(g => g.Count() > 1);
                foreach (var duplicateIdGroup in duplicateIdGroups)
                {
                    foreach (var entity in duplicateIdGroup)
                    {
                        int index = indicesDic[entity];
                        AddRowError(index + 2, _localizer["Error_TheCode0IsDuplicated", entity.Code]);
                    }
                }

                if (mode == "Merge")
                {
                    // Merge simply inserts codes that are not found, and updates codes that are found
                    result.ForEach(e =>
                    {
                        if (e.Id != null)
                        {
                            e.EntityState = EntityStates.Updated;
                        }
                        else
                        {
                            e.EntityState = EntityStates.Inserted;
                        }
                    });
                }
                else
                {
                    // In the case of update: codes are required, and MUST match database Ids
                    if (mode == "Update")
                    {
                        for (int index = 0; index < result.Count; index++)
                        {
                            var entity = result[index];
                            if (string.IsNullOrWhiteSpace(entity.Code))
                            {
                                AddRowError(index + 2, _localizer["Error_CodeIsRequiredForImportModeUpdate"]);
                            }
                            else if (entity.Id == null)
                            {
                                AddRowError(index + 2, _localizer["Error_TheCode0DoesNotExist", entity.Code]);
                            }
                        }

                        result.ForEach(e => e.EntityState = EntityStates.Updated);
                    }
                    else
                    {
                        throw new InvalidOperationException("Unknown save mode"); // Developer bug
                    }
                }
            }

            // Milestone 3: Id and EntityState are set
            if (!ModelState.IsValid)
            {
                throw new UnprocessableEntityException(ModelState);
            }

            // Function maps any future validation errors back to specific rows
            int? errorKeyMap(string key)
            {
                int? rowNumber = null;
                if (key != null && key.StartsWith("["))
                {
                    var indexStr = key.TrimStart('[').Split(']')[0];
                    if (int.TryParse(indexStr, out int index))
                    {
                        // Add 2:
                        // 1 for the header in the abstract grid
                        // 1 for the difference between index and number
                        rowNumber = index + 2;
                    }
                }
                return rowNumber;
            }

            return (result, errorKeyMap);
        }
    }
}
