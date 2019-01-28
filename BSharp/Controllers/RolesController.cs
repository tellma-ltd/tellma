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
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using M = BSharp.Data.Model;

namespace BSharp.Controllers
{
    [Route("api/roles")]
    public class RolesController : CrudControllerBase<M.Role, Role, RoleForSave, int?>
    {
        private readonly ApplicationContext _db;
        private readonly IModelMetadataProvider _metadataProvider;
        private readonly ILogger<RolesController> _logger;
        private readonly IStringLocalizer<RolesController> _localizer;
        private readonly IMapper _mapper;

        public RolesController(ApplicationContext db, IModelMetadataProvider metadataProvider, ILogger<RolesController> logger,
            IStringLocalizer<RolesController> localizer, IMapper mapper, IUserService userService) : base(logger, localizer, mapper, userService)
        {
            _db = db;
            _metadataProvider = metadataProvider;
            _logger = logger;
            _localizer = localizer;
            _mapper = mapper;
        }

        [HttpPut("activate")]
        public async Task<ActionResult<EntitiesResponse<Role>>> Activate([FromBody] List<int> ids, [FromQuery] ActivateArguments<int> args)
        {
            return await ActivateDeactivate(ids, args.ReturnEntities ?? false, args.Expand, isActive: true);
        }

        [HttpPut("deactivate")]
        public async Task<ActionResult<EntitiesResponse<Role>>> Deactivate([FromBody] List<int> ids, [FromQuery] DeactivateArguments<int> args)
        {
            return await ActivateDeactivate(ids, args.ReturnEntities ?? false, args.Expand, isActive: false);
        }

        private async Task<ActionResult<EntitiesResponse<Role>>> ActivateDeactivate([FromBody] List<int> ids, bool returnEntities, string expand, bool isActive)
        {
            using (var trx = await _db.Database.BeginTransactionAsync())
            {
                try
                {
                    // TODO Authorize Activate

                    // TODO Validate (No used units, no duplicate Ids, no missing Ids?)

                    var isActiveParam = new SqlParameter("@IsActive", isActive);

                    DataTable idsTable = DataTable(ids.Select(id => new { Id = id }), addIndex: false);
                    var idsTvp = new SqlParameter("@Ids", idsTable)
                    {
                        TypeName = $"dbo.IdList",
                        SqlDbType = SqlDbType.Structured
                    };

                    string sql = @"
DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();
DECLARE @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));

MERGE INTO [dbo].[Roles] AS t
	USING (
		SELECT [Id]
		FROM @Ids
	) AS s ON (t.Id = s.Id)
	WHEN MATCHED AND (t.IsActive <> @IsActive)
	THEN
		UPDATE SET 
			t.[IsActive]	= @IsActive,
			t.[ModifiedAt]	= @Now,
			t.[ModifiedById]	= @UserId;
";

                    // Update the entities
                    await _db.Database.ExecuteSqlCommandAsync(sql, idsTvp, isActiveParam);
                    trx.Commit();
                }
                catch (Exception ex)
                {
                    trx.Rollback();
                    _logger.LogError($"Error: {ex.Message} {ex.StackTrace}");
                    return BadRequest(ex.Message);
                }
            }

            // Determine whether entities should be returned
            if (!returnEntities)
            {
                // IF no returned items are expected, simply return 200 OK
                return Ok();
            }
            else
            {
                // Load the entities using their Ids
                var affectedDbEntitiesQ = _db.Roles.Where(e => ids.Contains(e.Id)); // _db.Roles.FromSql("SELECT * FROM [dbo].[Roles] WHERE Id IN (SELECT Id FROM @Ids)", idsTvp);
                var affectedDbEntitiesExpandedQ = Expand(affectedDbEntitiesQ, expand);
                var affectedDbEntities = await affectedDbEntitiesExpandedQ.ToListAsync();
                var affectedEntities = _mapper.Map<List<Role>>(affectedDbEntities);

                // sort the entities the way their Ids came, as a good practice
                Role[] sortedAffectedEntities = new Role[ids.Count];
                Dictionary<int, Role> affectedEntitiesDic = affectedEntities.ToDictionary(e => e.Id.Value);
                for (int i = 0; i < ids.Count; i++)
                {
                    var id = ids[i];
                    Role entity = null;
                    if (affectedEntitiesDic.ContainsKey(id))
                    {
                        entity = affectedEntitiesDic[id];
                    }

                    sortedAffectedEntities[i] = entity;
                }

                // Prepare a proper response
                var response = new EntitiesResponse<Role>
                {
                    Data = sortedAffectedEntities,
                    CollectionName = GetCollectionName(typeof(Role))
                };

                // Commit and return
                return Ok(response);
            }
        }

        protected override async Task<IDbContextTransaction> BeginSaveTransaction()
        {
            return await _db.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);
        }

        protected override string ViewId()
        {
            return "roles";
        }

        protected override IQueryable<M.Role> GetBaseQuery() => _db.Roles;

        protected override IQueryable<M.Role> SingletonQuery(IQueryable<M.Role> query, int? id)
        {
            return query.Where(e => e.Id == id);
        }

        protected override IQueryable<M.Role> Search(IQueryable<M.Role> query, string search)
        {
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(e => e.Name.Contains(search) || e.Name2.Contains(search) || e.Code.Contains(search));
            }

            return query;
        }

        protected override IQueryable<M.Role> IncludeInactive(IQueryable<M.Role> query, bool inactive)
        {
            if (!inactive)
            {
                query = query.Where(e => e.IsActive);
            }

            return query;
        }

        protected override IQueryable<M.Role> Expand(IQueryable<M.Role> query, string expand)
        {
            // TODO: Move it to a place where it can be universally applied

            // Here we make sure that a valid 'Permissions/View' expand term does not make it to the default implementation
            // since it would throw an error, 'View' is not a navigation property in the DB and has to be manually included
            // in FlattenRelatedEntities
            string nonDbExpand = "Permissions/View";
            if (expand != null && expand.Contains(nonDbExpand))
            {
                // Take out the non DB Expand term and call the default implementation on the rest
                var dbExpands = expand.Split(',').Select(e => e.Trim()).Where(e => e != nonDbExpand).ToList();
                dbExpands.Add("Permissions");
                expand = string.Join(",", dbExpands);
            }

            return base.Expand(query, expand);
        }

        protected override Dictionary<string, IEnumerable<DtoBase>> FlattenRelatedEntities(List<M.Role> models, string expand)
        {
            // TODO: Move it to a place where it can be universally applied

            // Here we artificially include Permissions/Views since it is not a navigation
            // property in the DB and therefore will throw an error in the default implementation
            string nonDbExpand = "Permissions/View";
            if (expand != null && expand.Contains(nonDbExpand))
            {
                // Take out the non DB Expand term and call the default implementation on the rest
                var dbExpands = expand.Split(',').Select(e => e.Trim()).Where(e => e != nonDbExpand).ToList();
                dbExpands.Add("Permissions");
                Dictionary<string, IEnumerable<DtoBase>> result = base.FlattenRelatedEntities(models, string.Join(",", dbExpands));

                // Manually include the views by invoking the Views repository
                var viewIds = models.SelectMany(e => e.Permissions).Select(e => e.ViewId).Where(e => e != null).Distinct();
                var repo = new ViewsRepository(_db, _localizer);
                var allViews = repo.GetAllViews().ToDictionary(e => e.Id);

                var viewList = new List<View>(viewIds.Count());

                foreach (var viewId in viewIds)
                {
                    if (allViews.ContainsKey(viewId))
                    {
                        var viewDef = allViews[viewId];
                        viewList.Add(_mapper.Map<View>(viewDef));
                    }
                }

                result["Views"] = viewList;
                return result;
            }
            else
            {
                return base.FlattenRelatedEntities(models, expand);
            }
        }

        protected override async Task ValidateAsync(List<RoleForSave> entities)
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
                    ModelState.AddModelError($"[{index}].{nameof(entity.EntityState)}", _localizer["Error_Deleting0IsNotSupportedFromThisAPI", _localizer["Roles"]]);
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
                    ModelState.AddModelError($"[{index}].{nameof(entity.Id)}",
                        _localizer["Error_TheEntityWithId0IsSpecifiedMoreThanOnce", entity.Id]);
                }
            }

            // Check that line ids are unique
            var duplicateLineIds = entities.SelectMany(e => e.Permissions) // All lines
                .Where(e => e.Id != null).GroupBy(e => e.Id.Value).Where(g => g.Count() > 1) // Duplicate Ids
                .SelectMany(g => g).ToDictionary(e => e, e => e.Id.Value); // to dictionary

            foreach (var entity in entities)
            {
                var lineIndices = entity.Permissions.ToIndexDictionary();
                foreach (var line in entity.Permissions)
                {
                    if (duplicateLineIds.ContainsKey(line))
                    {
                        // This error indicates a bug
                        var index = indices[entity];
                        var lineIndex = lineIndices[line];
                        var id = duplicateLineIds[line];
                        ModelState.AddModelError($"[{index}].{nameof(entity.Permissions)}[{lineIndex}].{nameof(entity.Id)}",
                            _localizer["Error_TheEntityWithId0IsSpecifiedMoreThanOnce", id]);
                    }
                }
            }

            // TODO Validate Criteria

            // No need to invoke SQL if the model state is full of errors
            if (ModelState.HasReachedMaxErrors)
            {
                return;
            }

            // Perform SQL-side validation
            DataTable rolesTable = DataTable(entities, addIndex: true);
            var rolesTvp = new SqlParameter("Roles", rolesTable) { TypeName = $"dbo.{nameof(RoleForSave)}List", SqlDbType = SqlDbType.Structured };

            var permissionHeaderIndices = indices.Keys.Select(role => (role.Permissions, indices[role]));
            DataTable permissionsTable = DataTableWithHeaderIndex(permissionHeaderIndices, e => e.EntityState != null);
            var permissionsTvp = new SqlParameter("Permissions", permissionsTable) { TypeName = $"dbo.{nameof(PermissionForSave)}List", SqlDbType = SqlDbType.Structured };

            int remainingErrorCount = ModelState.MaxAllowedErrors - ModelState.ErrorCount;

            // (1) Code must be unique
            var sqlErrors = await _db.Validation.FromSql($@"
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];
	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();

    INSERT INTO @ValidationErrors([Key], [ErrorName])
    SELECT '[' + CAST([Index] AS NVARCHAR(255)) + '].Id' As [Key], N'Error_CannotModifyInactiveItem' As [ErrorName]
    FROM @Roles
    WHERE Id IN (SELECT Id from [dbo].[Roles] WHERE IsActive = 0)
	OPTION(HASH JOIN);

    INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument1])
    SELECT '[' + CAST([Index] AS NVARCHAR(255)) + '].Id' As [Key], N'Error_TheId0WasNotFound' As [ErrorName], CAST([Id] As NVARCHAR(255)) As [Argument1]
    FROM @Roles
    WHERE Id Is NOT NULL
	AND Id NOT IN (SELECT Id from [dbo].[Roles])
	OPTION(HASH JOIN);
		
	-- Code must not be already in the back end
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument1], [Argument2], [Argument3], [Argument4], [Argument5]) 
	SELECT '[' + CAST(FE.[Index] AS NVARCHAR(255)) + '].Code' As [Key], N'Error_TheCode0IsUsed' As [ErrorName],
		FE.Code AS Argument1, NULL AS Argument2, NULL AS Argument3, NULL AS Argument4, NULL AS Argument5
	FROM @Roles FE 
	JOIN [dbo].Roles BE ON FE.Code = BE.Code
	WHERE FE.[Code] IS NOT NULL
	AND ((FE.[EntityState] = N'Inserted') OR (FE.Id <> BE.Id))
	OPTION(HASH JOIN);

	-- Code must not be duplicated in the uploaded list
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument1], [Argument2], [Argument3], [Argument4], [Argument5]) 
	SELECT '[' + CAST([Index] AS NVARCHAR(255)) + '].Code' As [Key], N'Error_TheCode0IsDuplicated' As [ErrorName],
		[Code] AS Argument1, NULL AS Argument2, NULL AS Argument3, NULL AS Argument4, NULL AS Argument5
	FROM @Roles
	WHERE [Code] IN (
		SELECT [Code]
		FROM @Roles
		WHERE [Code] IS NOT NULL
		GROUP BY [Code]
		HAVING COUNT(*) > 1
	) OPTION(HASH JOIN);

	-- Name must not exist in the db
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument1], [Argument2], [Argument3], [Argument4], [Argument5]) 
	SELECT '[' + CAST(FE.[Index] AS NVARCHAR(255)) + '].Name' As [Key], N'Error_TheName0IsUsed' As [ErrorName],
		FE.[Name] AS Argument1, NULL AS Argument2, NULL AS Argument3, NULL AS Argument4, NULL AS Argument5
	FROM @Roles FE 
	JOIN [dbo].Roles BE ON FE.[Name] = BE.[Name]
	WHERE (FE.[EntityState] = N'Inserted') OR (FE.Id <> BE.Id)
	OPTION(HASH JOIN);

	-- Name2 must not exist in the db
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument1], [Argument2], [Argument3], [Argument4], [Argument5]) 
	SELECT '[' + CAST(FE.[Index] AS NVARCHAR(255)) + '].Name2' As [Key], N'Error_TheName0IsUsed' As [ErrorName],
		FE.[Name2] AS Argument1, NULL AS Argument2, NULL AS Argument3, NULL AS Argument4, NULL AS Argument5
	FROM @Roles FE 
	JOIN [dbo].Roles BE ON FE.[Name2] = BE.[Name2]
	WHERE (FE.[EntityState] = N'Inserted') OR (FE.Id <> BE.Id)
	OPTION(HASH JOIN);

	-- Name must be unique in the uploaded list
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument1], [Argument2], [Argument3], [Argument4], [Argument5]) 
	SELECT '[' + CAST([Index] AS NVARCHAR(255)) + '].Name' As [Key], N'Error_TheName0IsDuplicated' As [ErrorName],
		[Name] AS Argument1, NULL AS Argument2, NULL AS Argument3, NULL AS Argument4, NULL AS Argument5
	FROM @Roles
	WHERE [Name] IN (
		SELECT [Name]
		FROM @Roles
		GROUP BY [Name]
		HAVING COUNT(*) > 1
	) OPTION(HASH JOIN);

	-- Name2 must be unique in the uploaded list
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument1], [Argument2], [Argument3], [Argument4], [Argument5]) 
	SELECT '[' + CAST([Index] AS NVARCHAR(255)) + '].Name2' As [Key], N'Error_TheName0IsDuplicated' As [ErrorName],
		[Name2] AS Argument1, NULL AS Argument2, NULL AS Argument3, NULL AS Argument4, NULL AS Argument5
	FROM @Roles
	WHERE [Name2] IN (
		SELECT [Name2]
		FROM @Roles
		WHERE [Name2] IS NOT NULL
		GROUP BY [Name2]
		HAVING COUNT(*) > 1
	) OPTION(HASH JOIN);

	-- No inactive view
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument1], [Argument2], [Argument3], [Argument4], [Argument5]) 
	SELECT '[' + CAST(P.[HeaderIndex] AS NVARCHAR(255)) + '].Permissions[' + 
				CAST(P.[Index] AS NVARCHAR(255)) + '].ViewId' As [Key], N'Error_TheView0IsInactive' As [ErrorName],
				P.[ViewId] AS Argument1, NULL AS Argument2, NULL AS Argument3, NULL AS Argument4, NULL AS Argument5
	FROM @Permissions P
	WHERE P.ViewId NOT IN (
		SELECT [Id] FROM dbo.[Views] WHERE IsActive = 1
		) AND P.ViewId <> 'all'
	AND (P.[EntityState] IN (N'Inserted', N'Updated'));
SELECT TOP {remainingErrorCount} * FROM @ValidationErrors;
", rolesTvp, permissionsTvp).ToListAsync();

            // Loop over the errors returned from SQL and add them to ModelState
            foreach (var sqlError in sqlErrors)
            {
                var formatArguments = sqlError.ToFormatArguments();

                string key = sqlError.Key;
                string errorMessage = _localizer[sqlError.ErrorName, formatArguments];

                ModelState.AddModelError(key: key, errorMessage: errorMessage);
            }
        }

        protected override async Task<List<M.Role>> PersistAsync(List<RoleForSave> entities, SaveArguments args)
        {
            // Add created entities
            var roleIndices = entities.ToIndexDictionary();
            DataTable rolesTable = DataTable(entities, addIndex: true);
            var rolesTvp = new SqlParameter("Roles", rolesTable)
            {
                TypeName = $"dbo.{nameof(RoleForSave)}List",
                SqlDbType = SqlDbType.Structured
            };

            // Filter out permissions that haven't changed for performance
            var permissionHeaderIndices = roleIndices.Keys.Select(role => (role.Permissions.Where(e => e.EntityState != null).ToList(), roleIndices[role]));
            DataTable permissionsTable = DataTableWithHeaderIndex(permissionHeaderIndices, e => e.EntityState != null);
            var permissionsTvp = new SqlParameter("Permissions", permissionsTable)
            {
                TypeName = $"dbo.{nameof(PermissionForSave)}List",
                SqlDbType = SqlDbType.Structured
            };

            string saveSql = $@"
-- Procedure: Roles__Save
    DECLARE @IndexedIds [dbo].[IndexedIdList], @PermissionsIndexedIds [dbo].[IndexedIdList];
	DECLARE @TenantId int = CONVERT(INT, SESSION_CONTEXT(N'TenantId'));
	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();
	DECLARE @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));

	DELETE FROM [dbo].[Permissions]
	WHERE [Id] IN (SELECT [Id] FROM @Permissions WHERE [EntityState] = N'Deleted');

	INSERT INTO @IndexedIds([Index], [Id])
	SELECT x.[Index], x.[Id]
	FROM
	(
		MERGE INTO [dbo].[Roles] AS t
		USING (
			SELECT 
				[Index], [Id], [Name], [Name2], [IsPublic], [Code]
			FROM @Roles 
			WHERE [EntityState] IN (N'Inserted', N'Updated')
		) AS s ON (t.Id = s.Id)
		WHEN MATCHED 
		THEN
			UPDATE SET
				t.[Name]		= s.[Name],
				t.[Name2]		= s.[Name2],
				t.[IsPublic]	= s.[IsPublic],
				t.[Code]		= s.[Code],
				t.[ModifiedAt]	= @Now,
				t.[ModifiedById]	= @UserId
		WHEN NOT MATCHED THEN
			INSERT (
				[TenantId], [Name], [Name2],	[IsPublic],		[Code], [CreatedAt], [CreatedById], [ModifiedAt], [ModifiedById]
			)
			VALUES (
				@TenantId, s.[Name], s.[Name2], s.[IsPublic], s.[Code], @Now,		@UserId,		@Now,		@UserId
			)
			OUTPUT s.[Index], inserted.[Id] 
	) As x;

    MERGE INTO [dbo].[Permissions] AS t
		USING (
			SELECT L.[Index], L.[Id], II.[Id] AS [RoleId], [ViewId], [Level], [Criteria], [Memo]
			FROM @Permissions L
			JOIN @IndexedIds II ON L.[HeaderIndex] = II.[Index]
			WHERE L.[EntityState] IN (N'Inserted', N'Updated')
		) AS s ON t.Id = s.Id
		WHEN MATCHED THEN
			UPDATE SET 
				t.[ViewId]		= s.[ViewId], 
				t.[Level]		= s.[Level],
				t.[Criteria]	= s.[Criteria],
				t.[Memo]		= s.[Memo],
				t.[ModifiedAt]	= @Now,
				t.[ModifiedById]	= @UserId
		WHEN NOT MATCHED THEN
			INSERT ([TenantId], [RoleId],	[ViewId],	[Level],	[Criteria], [Memo], [CreatedAt], [CreatedById], [ModifiedAt], [ModifiedById])
			VALUES (@TenantId, s.[RoleId], s.[ViewId], s.[Level], s.[Criteria], s.[Memo], @Now,		@UserId,		@Now,		@UserId);
";
            // Optimization
            if (!(args.ReturnEntities ?? false))
            {
                // IF no returned items are expected, simply execute a non-Query and return an empty list;
                await _db.Database.ExecuteSqlCommandAsync(saveSql, rolesTvp, permissionsTvp);
                return new List<M.Role>();
            }
            else
            {
                // If returned items are expected, append a select statement to the SQL command
                saveSql = saveSql += "SELECT * FROM @IndexedIds;";

                // Retrieve the map from Indexes to Ids
                var indexedIds = await _db.Saving.FromSql(saveSql, rolesTvp, permissionsTvp).ToListAsync();

                //// Load the entities using their Ids
                //DataTable idsTable = DataTable(indexedIds.Select(e => new { e.Id }));
                //var idsTvp = new SqlParameter("@Ids", idsTable)
                //{
                //    TypeName = $"dbo.IdList",
                //    SqlDbType = SqlDbType.Structured
                //};

                //var q = _db.Roles.FromSql("SELECT * FROM dbo.[Roles] WHERE Id IN (SELECT Id FROM @Ids)", idsTvp);
                var ids = indexedIds.Select(e => e.Id);
                var q = _db.Roles.Where(e => ids.Contains(e.Id));
                q = Expand(q, args.Expand); // Includes
                var savedEntities = await q.ToListAsync();

                // SQL Server does not guarantee order, so make sure the result is sorted according to the initial index
                Dictionary<int, int> indices = indexedIds.ToDictionary(e => e.Id, e => e.Index);
                var sortedSavedEntities = new M.Role[savedEntities.Count];
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
                    await _db.Database.ExecuteSqlCommandAsync("DELETE FROM dbo.[Roles] WHERE Id IN (SELECT Id FROM @Ids)", idsTvp);

                    // Commit and return
                    trx.Commit();
                    return;
                }
                catch (SqlException ex) when (IsForeignKeyViolation(ex))
                {
                    throw new BadRequestException(_localizer["Error_CannotDelete0AlreadyInUse", _localizer["Role"]]);
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
            throw new NotImplementedException();
        }

        protected override AbstractDataGrid DtosToAbstractGrid(GetResponse<Role> response, ExportArguments args)
        {
            throw new NotImplementedException();
        }

        protected override Task<(List<RoleForSave>, Func<string, int?>)> ToDtosForSave(AbstractDataGrid grid, ParseArguments args)
        {
            throw new NotImplementedException();
        }
    }
}