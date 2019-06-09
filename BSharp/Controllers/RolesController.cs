using BSharp.Controllers.DTO;
using BSharp.Controllers.Misc;
using BSharp.Data;
using BSharp.Services.ImportExport;
using BSharp.Services.MultiTenancy;
using BSharp.Services.OData;
using BSharp.Services.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace BSharp.Controllers
{
    [Route("api/roles")]
    [LoadTenantInfo]
    public class RolesController : CrudControllerBase<RoleForSave, Role, RoleForQuery, int?>
    {
        private readonly ApplicationContext _db;
        private readonly IModelMetadataProvider _metadataProvider;
        private readonly ILogger<RolesController> _logger;
        private readonly IStringLocalizer<RolesController> _localizer;
        private readonly ITenantUserInfoAccessor _tenantInfo;

        public RolesController(ILogger<RolesController> logger,
            IStringLocalizer<RolesController> localizer, IServiceProvider serviceProvider, ITenantUserInfoAccessor tenantInfoAccessor) : base(logger, localizer, serviceProvider)
        {
            _db = serviceProvider.GetRequiredService<ApplicationContext>();
            _metadataProvider = serviceProvider.GetRequiredService<IModelMetadataProvider>();
            _tenantInfo = serviceProvider.GetRequiredService<ITenantUserInfoAccessor>();

            _logger = logger;
            _localizer = localizer;
        }

        [HttpPut("activate")]
        public async Task<ActionResult<EntitiesResponse<Role>>> Activate([FromBody] List<int> ids, [FromQuery] ActivateArguments args)
        {
            return await ControllerUtilities.ExecuteAndHandleErrorsAsync(() =>
                ActivateDeactivate(ids, args.ReturnEntities ?? false, args.Expand, isActive: true)
            , _logger);
        }

        [HttpPut("deactivate")]
        public async Task<ActionResult<EntitiesResponse<Role>>> Deactivate([FromBody] List<int> ids, [FromQuery] DeactivateArguments args)
        {
            return await ControllerUtilities.ExecuteAndHandleErrorsAsync(() =>
                ActivateDeactivate(ids, args.ReturnEntities ?? false, args.Expand, isActive: false)
            , _logger);
        }

        private async Task<ActionResult<EntitiesResponse<Role>>> ActivateDeactivate([FromBody] List<int> ids, bool returnEntities, string expand, bool isActive)
        {
            var nullableIds = ids.Cast<int?>().ToArray();
            await CheckActionPermissions(nullableIds);

            using (var trx = await _db.Database.BeginTransactionAsync())
            {
                try
                {
                    var isActiveParam = new SqlParameter("@IsActive", isActive);

                    DataTable idsTable = ControllerUtilities.DataTable(ids.Select(id => new { Id = id }), addIndex: false);
                    var idsTvp = new SqlParameter("@Ids", idsTable)
                    {
                        TypeName = $"dbo.IdList",
                        SqlDbType = SqlDbType.Structured
                    };

                    string sql = @"
-- TODO: PermissionsVersion
DECLARE @NewId UNIQUEIDENTIFIER = NEWID();
UPDATE [dbo].[LocalUsers] SET PermissionsVersion = @NewId;

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
                    throw ex;
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
                var affectedDbEntitiesQ = CreateODataQuery().FilterByIds(nullableIds);
                var affectedDbEntitiesExpandedQ = affectedDbEntitiesQ.Clone().Expand(expand);
                var affectedDbEntities = await affectedDbEntitiesExpandedQ.ToListAsync();

                // sort the entities the way their Ids came, as a good practice
                var affectedEntities = Mapper.Map<List<Role>>(affectedDbEntities);
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

                // Apply the permission masks (setting restricted fields to null) and adjust the metadata accordingly
                await ApplyReadPermissionsMask(affectedDbEntities, affectedDbEntitiesExpandedQ, await UserPermissions(PermissionLevel.Read), GetDefaultMask());

                // Flatten related entities and map each to its respective DTO 
                var relatedEntities = FlattenRelatedEntitiesAndTrim(affectedDbEntities, expand);

                // Prepare a proper response
                var response = new EntitiesResponse<Role>
                {
                    Data = sortedAffectedEntities,
                    CollectionName = GetCollectionName(typeof(Role)),
                    RelatedEntities = relatedEntities
                };

                // Commit and return
                return Ok(response);
            }
        }

        protected override Task<IEnumerable<AbstractPermission>> UserPermissions(PermissionLevel level)
        {
            return ControllerUtilities.GetPermissions(_db.AbstractPermissions, level, "roles");
        }

        protected override DbContext GetDbContext()
        {
            return _db;
        }

        protected override Func<Type, string> GetSources()
        {
            var info = _tenantInfo.GetCurrentInfo();
            return ControllerUtilities.GetApplicationSources(_localizer, info.PrimaryLanguageId, info.SecondaryLanguageId, info.TernaryLanguageId);
        }

        protected override ODataQuery<RoleForQuery, int?> Search(ODataQuery<RoleForQuery, int?> query, GetArguments args, IEnumerable<AbstractPermission> filteredPermissions)
        {
            string search = args.Search;
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Replace("'", "''"); // escape quotes by repeating them

                var name = nameof(RoleForQuery.Name);
                var name2 = nameof(RoleForQuery.Name2);
                // var name3 = nameof(MeasurementUnitForQuery.Name3); // TODO
                var code = nameof(RoleForQuery.Code);

                query.Filter($"{name} contains '{search}' or {name2} contains '{search}' or {code} contains '{search}'");
            }

            return query;
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
            entities.ForEach(e => { if (e.Permissions == null) { e.Permissions = new List<PermissionForSave>(); } });
            var duplicatePermissionId = entities.SelectMany(e => e.Permissions) // All lines
                .Where(e => e.Id != null).GroupBy(e => e.Id.Value).Where(g => g.Count() > 1) // Duplicate Ids
                .SelectMany(g => g).ToDictionary(e => e, e => e.Id.Value); // to dictionary

            // Check that line ids are unique
            entities.ForEach(e => { if (e.Members == null) { e.Members = new List<RoleMembershipForSave>(); } });
            var duplicateMembershipId = entities.SelectMany(e => e.Members) // All lines
                .Where(e => e.Id != null).GroupBy(e => e.Id.Value).Where(g => g.Count() > 1) // Duplicate Ids
                .SelectMany(g => g).ToDictionary(e => e, e => e.Id.Value); // to dictionary


            // Check that line ids are unique
            //var duplicateLineIds = entities.SelectMany(e => e.Permissions) // All lines
            //    .Where(e => e.Id != null).GroupBy(e => e.Id.Value).Where(g => g.Count() > 1) // Duplicate Ids
            //    .SelectMany(g => g).ToDictionary(e => e, e => e.Id.Value); // to dictionary

            foreach (var entity in entities)
            {
                var permissionIndices = entity.Permissions.ToIndexDictionary();
                foreach (var line in entity.Permissions)
                {
                    if (duplicatePermissionId.ContainsKey(line))
                    {
                        // This error indicates a bug
                        var index = indices[entity];
                        var lineIndex = permissionIndices[line];
                        var id = duplicatePermissionId[line];
                        ModelState.AddModelError($"[{index}].{nameof(entity.Permissions)}[{lineIndex}].{nameof(entity.Id)}",
                            _localizer["Error_TheEntityWithId0IsSpecifiedMoreThanOnce", id]);
                    }

                }

                var membersIndices = entity.Members.ToIndexDictionary();
                foreach (var line in entity.Members)
                {
                    if (duplicateMembershipId.ContainsKey(line))
                    {
                        // This error indicates a bug
                        var index = indices[entity];
                        var lineIndex = membersIndices[line];
                        var id = duplicateMembershipId[line];
                        ModelState.AddModelError($"[{index}].{nameof(entity.Members)}[{lineIndex}].{nameof(entity.Id)}",
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
            DataTable rolesTable = ControllerUtilities.DataTable(entities, addIndex: true);
            var rolesTvp = new SqlParameter("Roles", rolesTable) { TypeName = $"dbo.{nameof(RoleForSave)}List", SqlDbType = SqlDbType.Structured };

            var permissionHeaderIndices = indices.Keys.Select(role => (role.Permissions, indices[role]));
            DataTable permissionsTable = ControllerUtilities.DataTableWithHeaderIndex(permissionHeaderIndices, e => e.EntityState != null);
            var permissionsTvp = new SqlParameter("Permissions", permissionsTable) { TypeName = $"dbo.{nameof(PermissionForSave)}List", SqlDbType = SqlDbType.Structured };

            var signatureHeaderIndices = indices.Keys.Select(role => (role.Signatures, indices[role]));
            DataTable signaturesTable = ControllerUtilities.DataTableWithHeaderIndex(signatureHeaderIndices, e => e.EntityState != null);
            var signaturesTvp = new SqlParameter("Signatures", signaturesTable) { TypeName = $"dbo.{nameof(RequiredSignatureForSave)}List", SqlDbType = SqlDbType.Structured };

            var memberHeaderIndices = indices.Keys.Select(role => (role.Members, indices[role]));
            DataTable membersTable = ControllerUtilities.DataTableWithHeaderIndex(memberHeaderIndices, e => e.EntityState != null);
            var membersTvp = new SqlParameter("Members", membersTable) { TypeName = $"dbo.{nameof(RoleMembershipForSave)}List", SqlDbType = SqlDbType.Structured };

            int remainingErrorCount = ModelState.MaxAllowedErrors - ModelState.ErrorCount;

            // (1) Code must be unique
            var sqlErrors = await _db.Validation.FromSql($@"
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];
	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();
	DECLARE @Language INT = dbo.fn_User__Language();

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

	-- No inactive user
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument1], [Argument2], [Argument3], [Argument4], [Argument5]) 
	SELECT '[' + CAST(P.[HeaderIndex] AS NVARCHAR(255)) + '].Members[' + 
				CAST(P.[Index] AS NVARCHAR(255)) + '].UserId' As [Key], N'Error_TheUser0IsInactive' As [ErrorName],
				CASE WHEN @Language = 2 THEN [dbo].[fn_IsNullOrEmpty](U.[Name2], U.[Name]) ELSE U.[Name] END AS Argument1, NULL AS Argument2, NULL AS Argument3, NULL AS Argument4, NULL AS Argument5
	FROM @Members P JOIN [dbo].[LocalUsers] U ON P.UserId = U.Id
	WHERE P.UserId NOT IN (
		SELECT [Id] FROM dbo.[LocalUsers] WHERE IsActive = 1
		)
	AND (P.[EntityState] IN (N'Inserted', N'Updated'));

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

	-- No inactive view
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument1], [Argument2], [Argument3], [Argument4], [Argument5]) 
	SELECT '[' + CAST(P.[HeaderIndex] AS NVARCHAR(255)) + '].Signatures[' + 
				CAST(P.[Index] AS NVARCHAR(255)) + '].ViewId' As [Key], N'Error_TheView0IsInactive' As [ErrorName],
				P.[ViewId] AS Argument1, NULL AS Argument2, NULL AS Argument3, NULL AS Argument4, NULL AS Argument5
	FROM @Signatures P
	WHERE P.ViewId NOT IN (
		SELECT [Id] FROM dbo.[Views] WHERE IsActive = 1
		) AND P.ViewId <> 'all'
	AND (P.[EntityState] IN (N'Inserted', N'Updated'));

SELECT TOP {remainingErrorCount} * FROM @ValidationErrors;
", rolesTvp, permissionsTvp, signaturesTvp, membersTvp).ToListAsync();

            // Loop over the errors returned from SQL and add them to ModelState
            foreach (var sqlError in sqlErrors)
            {
                var formatArguments = sqlError.ToFormatArguments();

                string key = sqlError.Key;
                string errorMessage = _localizer[sqlError.ErrorName, formatArguments];

                ModelState.AddModelError(key: key, errorMessage: errorMessage);
            }
        }

        protected override async Task<List<int?>> PersistAsync(List<RoleForSave> entities, SaveArguments args)
        {
            // Add created entities
            var roleIndices = entities.ToIndexDictionary();
            DataTable rolesTable = ControllerUtilities.DataTable(entities, addIndex: true);
            var rolesTvp = new SqlParameter("Roles", rolesTable)
            {
                TypeName = $"dbo.{nameof(RoleForSave)}List",
                SqlDbType = SqlDbType.Structured
            };

            // Filter out permissions that haven't changed for performance
            var permissionHeaderIndices = roleIndices.Keys.Select(role => (role.Permissions.Where(e => e.EntityState != null).ToList(), roleIndices[role]));
            DataTable permissionsTable = ControllerUtilities.DataTableWithHeaderIndex(permissionHeaderIndices, e => e.EntityState != null);
            var permissionsTvp = new SqlParameter("Permissions", permissionsTable)
            {
                TypeName = $"dbo.{nameof(PermissionForSave)}List",
                SqlDbType = SqlDbType.Structured
            };

            var signatureHeaderIndices = roleIndices.Keys.Select(role => (role.Signatures.Where(e => e.EntityState != null).ToList(), roleIndices[role]));
            DataTable signaturesTable = ControllerUtilities.DataTableWithHeaderIndex(signatureHeaderIndices, e => e.EntityState != null);
            var signaturesTvp = new SqlParameter("Signatures", signaturesTable)
            {
                TypeName = $"dbo.{nameof(RequiredSignatureForSave)}List",
                SqlDbType = SqlDbType.Structured
            };

            var memberHeaderIndices = roleIndices.Keys.Select(role => (role.Members, roleIndices[role]));
            DataTable membersTable = ControllerUtilities.DataTableWithHeaderIndex(memberHeaderIndices, e => e.EntityState != null);
            var membersTvp = new SqlParameter("Members", membersTable)
            {
                TypeName = $"dbo.{nameof(RoleMembershipForSave)}List",
                SqlDbType = SqlDbType.Structured
            };


            string saveSql = $@"
-- TODO: PermissionsVersion
DECLARE @NewId UNIQUEIDENTIFIER = NEWID();
UPDATE [dbo].[LocalUsers] SET PermissionsVersion = @NewId;

-- Procedure: Roles__Save
    DECLARE @IndexedIds [dbo].[IndexedIdList], @PermissionsIndexedIds [dbo].[IndexedIdList];
	DECLARE @TenantId int = CONVERT(INT, SESSION_CONTEXT(N'TenantId'));
	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();
	DECLARE @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));

	DELETE FROM [dbo].[Permissions]
	WHERE [Id] IN (SELECT [Id] FROM @Permissions WHERE [EntityState] = N'Deleted');

	DELETE FROM [dbo].[Permissions]
	WHERE [Id] IN (SELECT [Id] FROM @Signatures WHERE [EntityState] = N'Deleted');

	DELETE FROM [dbo].[RoleMemberships]
	WHERE [Id] IN (SELECT [Id] FROM @Members WHERE [EntityState] = N'Deleted');

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
			SELECT L.[Index], L.[Id], II.[Id] AS [RoleId], [ViewId], [Level], L.[Criteria], L.[Mask], L.[Memo]
			FROM @Permissions L 
			JOIN @IndexedIds II ON L.[HeaderIndex] = II.[Index]
			WHERE L.[EntityState] IN (N'Inserted', N'Updated')
            UNION
			SELECT L.[Index], L.[Id], II.[Id] AS [RoleId], [ViewId], 'Sign' AS [Level], L.[Criteria], NULL as [Mask], L.[Memo]
			FROM @Signatures L 
			JOIN @IndexedIds II ON L.[HeaderIndex] = II.[Index]
			WHERE L.[EntityState] IN (N'Inserted', N'Updated')
		) AS s ON t.Id = s.Id
		WHEN MATCHED THEN
			UPDATE SET 
				t.[ViewId]		    = s.[ViewId], 
				t.[Level]		    = s.[Level],
				t.[Criteria]	    = s.[Criteria],
				t.[Mask]	        = s.[Mask],
				t.[Memo]		    = s.[Memo],
				t.[ModifiedAt]	    = @Now,
				t.[ModifiedById]	= @UserId
		WHEN NOT MATCHED THEN
			INSERT ([TenantId], [RoleId],	[ViewId],	[Level],	[Criteria], [Mask], [Memo], [CreatedAt], [CreatedById], [ModifiedAt], [ModifiedById])
			VALUES (@TenantId, s.[RoleId], s.[ViewId], s.[Level], s.[Criteria], s.[Mask], s.[Memo], @Now,		@UserId,		@Now,		@UserId);


    MERGE INTO [dbo].[RoleMemberships] AS t
		USING (
			SELECT L.[Index], L.[Id], II.[Id] AS [RoleId], [UserId], [Memo]
			FROM @Members L
			JOIN @IndexedIds II ON L.[HeaderIndex] = II.[Index]
			WHERE L.[EntityState] IN (N'Inserted', N'Updated')
		) AS s ON t.Id = s.Id
		WHEN MATCHED THEN
			UPDATE SET 
				t.[UserId]		    = s.[UserId], 
				t.[RoleId]		    = s.[RoleId],
				t.[Memo]		    = s.[Memo],
				t.[ModifiedAt]	    = @Now,
				t.[ModifiedById]	= @UserId
		WHEN NOT MATCHED THEN
			INSERT ([TenantId], [UserId],	[RoleId],	 [Memo], [CreatedAt], [CreatedById], [ModifiedAt], [ModifiedById])
			VALUES (@TenantId, s.[UserId], s.[RoleId], s.[Memo], @Now,		@UserId,		@Now,		@UserId);
";
            // Optimization
            if (!(args.ReturnEntities ?? false))
            {
                // IF no returned items are expected, simply execute a non-Query and return an empty list;
                await _db.Database.ExecuteSqlCommandAsync(saveSql, rolesTvp, permissionsTvp, signaturesTvp, membersTvp);
                return null;
            }
            else
            {
                // If returned items are expected, append a select statement to the SQL command
                saveSql = saveSql += "SELECT * FROM @IndexedIds;";

                // Retrieve the map from Indexes to Ids
                var indexedIds = await _db.Saving.FromSql(saveSql, rolesTvp, permissionsTvp, signaturesTvp, membersTvp).ToListAsync();

                // return the Ids in the same order they came
                return indexedIds.OrderBy(e => e.Index).Select(e => (int?)e.Id).ToList();
            }
        }

        protected override async Task DeleteAsync(List<int?> ids)
        {
            // Prepare a list of Ids to delete
            DataTable idsTable = ControllerUtilities.DataTable(ids.Select(e => new { Id = e }), addIndex: false);
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
                    await _db.Database.ExecuteSqlCommandAsync(@"
-- TODO: PermissionsVersion
DECLARE @NewId UNIQUEIDENTIFIER = NEWID();
UPDATE [dbo].[LocalUsers] SET PermissionsVersion = @NewId;

DELETE FROM dbo.[Roles] WHERE Id IN (SELECT Id FROM @Ids)", idsTvp);

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

        protected override (string PreambleSql, string ComposableSql, List<SqlParameter> Parameters) GetAsSql(IEnumerable<RoleForSave> entities)
        {
            // Preamble SQL
            string preambleSql = 
$@" DECLARE @TenantId int = CONVERT(INT, SESSION_CONTEXT(N'TenantId'));
	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();
	DECLARE @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));
	DECLARE @True BIT = 1;";

            // Composable SQL
            string sql = 
$@"SELECT  @TenantId AS TenantId, ISNULL(E.Id, 0) AS Id, E.Name, E.Name2, E.Code, E.IsPublic,
    @True AS IsActive, @Now AS CreatedAt, @UserId AS CreatedById, @Now AS ModifiedAt, @UserId AS ModifiedById 
    FROM @Entities E";

            // Entities TVP put in a singleton
            DataTable entitiesTable = ControllerUtilities.DataTable(entities, addIndex: true);
            var entitiesTvp = new SqlParameter("Entities", entitiesTable)
            {
                TypeName = $"dbo.{nameof(RoleForSave)}List",
                SqlDbType = SqlDbType.Structured
            };

            var ps = new List<SqlParameter>() { entitiesTvp };

            // Return the result
            return (preambleSql, sql, ps);
        }
    }
}