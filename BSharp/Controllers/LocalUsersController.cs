using AutoMapper;
using BSharp.Controllers.DTO;
using BSharp.Controllers.Misc;
using BSharp.Data;
using BSharp.Services.ImportExport;
using BSharp.Services.MultiTenancy;
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
using System.Linq.Expressions;
using System.Threading.Tasks;
using M = BSharp.Data.Model;


namespace BSharp.Controllers
{
    [Route("api/local-users")]
    [LoadTenantInfo]
    public class LocalUsersController : CrudControllerBase<M.LocalUser, LocalUser, LocalUserForSave, int?>
    {
        private readonly ApplicationContext _db;
        private readonly AdminContext _adminDb;
        private readonly ITenantIdProvider _tenantIdProvider;
        private readonly IModelMetadataProvider _metadataProvider;
        private readonly ILogger<LocalUsersController> _logger;
        private readonly IStringLocalizer<LocalUsersController> _localizer;
        private readonly IMapper _mapper;

        private readonly ITenantUserInfoAccessor _tenantInfo;

        public LocalUsersController(ApplicationContext db, AdminContext adminDb,
            IModelMetadataProvider metadataProvider, ILogger<LocalUsersController> logger,
            IStringLocalizer<LocalUsersController> localizer, IMapper mapper, ITenantIdProvider tenantIdProvider,
            ITenantUserInfoAccessor tenantInfo) : base(logger, localizer, mapper)
        {
            _db = db;
            _adminDb = adminDb;
            _tenantIdProvider = tenantIdProvider;
            _metadataProvider = metadataProvider;
            _logger = logger;
            _localizer = localizer;
            _mapper = mapper;
            _tenantInfo = tenantInfo;
        }

        [HttpPut("activate")]
        public async Task<ActionResult<EntitiesResponse<LocalUser>>> Activate([FromBody] List<int> ids, [FromQuery] ActivateArguments<int> args)
        {
            return await CallAndHandleErrorsAsync(() =>
                ActivateDeactivate(ids, args.ReturnEntities ?? false, args.Expand, isActive: true)
            );
        }

        [HttpPut("deactivate")]
        public async Task<ActionResult<EntitiesResponse<LocalUser>>> Deactivate([FromBody] List<int> ids, [FromQuery] DeactivateArguments<int> args)
        {
            return await CallAndHandleErrorsAsync(async () =>
                {
                    var currentUserId = _tenantInfo.GetCurrentInfo().UserId.Value;
                    if (ids.Any(id => id == currentUserId))
                    {
                        return BadRequest(_localizer["Error_CannotDeactivateYourOwnUser"].Value);
                    }

                    return await ActivateDeactivate(ids, args.ReturnEntities ?? false, args.Expand, isActive: false);
                }
            );
        }

        private async Task<ActionResult<EntitiesResponse<LocalUser>>> ActivateDeactivate([FromBody] List<int> ids, bool returnEntities, string expand, bool isActive)
        {
            await CheckActionPermissions(ids.Cast<int?>());

            var isActiveParam = new SqlParameter("@IsActive", isActive);

            DataTable idsTable = DataTable(ids.Select(id => new { Id = id }), addIndex: false);
            var idsTvp = new SqlParameter("@Ids", idsTable)
            {
                TypeName = $"dbo.IdList",
                SqlDbType = SqlDbType.Structured
            };

            string sql = @"
    SET NOCOUNT ON;
    DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();
    DECLARE @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));
    DECLARE @Emails [dbo].[CodeList];

    INSERT INTO @Emails([Code])
    SELECT x.[Email]
    FROM
    (
        MERGE INTO [dbo].[LocalUsers] AS t
	        USING (
		        SELECT [Id]
		        FROM @Ids
	        ) AS s ON (t.Id = s.Id)
	        WHEN MATCHED AND (t.IsActive <> @IsActive)
	        THEN
		        UPDATE SET 
			    t.[IsActive]	= @IsActive,
			    t.[ModifiedAt]	= @Now,
			    t.[ModifiedById]= @UserId
                OUTPUT inserted.[Email]
    ) As x;

    SELECT [Code] AS [Value] FROM @Emails;
";

            // Tenant Id
            var tenantId = new SqlParameter("TenantId", _tenantIdProvider.GetTenantId());

            using (var trxApp = await _db.Database.BeginTransactionAsync())
            {
                try
                {
                    // Update the entities and retrieve the emails of the entities that were updated
                    List<string> emails = await _db.Strings.FromSql(sql, idsTvp, isActiveParam).Select(e => e.Value).ToListAsync();

                    // Prepare the TVP of emails to update from the manager
                    DataTable emailsTable = DataTable(emails.Select(e => new { Code = e }), addIndex: false);
                    var emailsTvp = new SqlParameter("Emails", emailsTable)
                    {
                        TypeName = $"dbo.CodeList",
                        SqlDbType = SqlDbType.Structured
                    };

                    using (var trxAdmin = await _adminDb.Database.BeginTransactionAsync())
                    {
                        try
                        {
                            if (isActive)
                            {
                                // Insert efficiently with a SQL query
                                await _adminDb.Database.ExecuteSqlCommandAsync($@"
    INSERT INTO dbo.[TenantMemberships] 
    SELECT Id, @TenantId FROM [dbo].[GlobalUsers] WHERE Email IN (SELECT Code from @Emails);
", emailsTvp, tenantId);

                            }
                            else
                            {
                                // Delete efficiently with a SQL query
                                await _adminDb.Database.ExecuteSqlCommandAsync($@"
    DELETE FROM dbo.[TenantMemberships] 
    WHERE TenantId = @TenantId AND UserId IN (
        SELECT Id FROM [dbo].[GlobalUsers] WHERE Email IN (SELECT Code from @Emails)
    );
", emailsTvp, tenantId);
                            }

                            // Commit both
                            trxAdmin.Commit();
                            trxApp.Commit();
                        }
                        catch (Exception ex)
                        {
                            trxApp.Rollback();
                            trxAdmin.Rollback();
                            throw ex;
                        }
                    }
                }
                catch (Exception ex)
                {
                    trxApp.Rollback();
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
                var affectedDbEntitiesQ = _db.LocalUsers.Where(e => ids.Contains(e.Id)); // _db.LocalUsers.FromSql("SELECT * FROM [dbo].[LocalUsers] WHERE Id IN (SELECT Id FROM @Ids)", idsTvp);
                var affectedDbEntitiesExpandedQ = Expand(affectedDbEntitiesQ, expand);
                var affectedDbEntities = await affectedDbEntitiesExpandedQ.ToListAsync();
                var affectedEntities = _mapper.Map<List<LocalUser>>(affectedDbEntities);

                // sort the entities the way their Ids came, as a good practice
                LocalUser[] sortedAffectedEntities = new LocalUser[ids.Count];
                Dictionary<int, LocalUser> affectedEntitiesDic = affectedEntities.ToDictionary(e => e.Id.Value);
                for (int i = 0; i < ids.Count; i++)
                {
                    var id = ids[i];
                    LocalUser entity = null;
                    if (affectedEntitiesDic.ContainsKey(id))
                    {
                        entity = affectedEntitiesDic[id];
                    }

                    sortedAffectedEntities[i] = entity;
                }

                // Prepare a proper response
                var response = new EntitiesResponse<LocalUser>
                {
                    Data = sortedAffectedEntities,
                    CollectionName = GetCollectionName(typeof(LocalUser))
                };

                // Commit and return
                return Ok(response);
            }
        }

        protected override Task<IEnumerable<M.AbstractPermission>> UserPermissions(PermissionLevel level)
        {
            return GetPermissions(_db.AbstractPermissions, level, "local-users");
        }

        protected override async Task<IDbContextTransaction> BeginSaveTransaction()
        {
            return await _db.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);
        }

        protected override IQueryable<M.LocalUser> GetBaseQuery() => _db.LocalUsers;

        protected override IQueryable<M.LocalUser> SingletonQuery(IQueryable<M.LocalUser> query, int? id)
        {
            return query.Where(e => e.Id == id);
        }

        protected override IQueryable<M.LocalUser> Search(IQueryable<M.LocalUser> query, string search)
        {
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(e => e.Name.Contains(search) || e.Name2.Contains(search) || e.Email.Contains(search));
            }

            return query;
        }

        protected override IQueryable<M.LocalUser> IncludeInactive(IQueryable<M.LocalUser> query, bool inactive)
        {
            if (!inactive)
            {
                query = query.Where(e => e.IsActive);
            }

            return query;
        }

        protected override async Task ValidateAsync(List<LocalUserForSave> entities)
        {
            // Hash the indices for performance
            var indices = entities.ToIndexDictionary();

            // Check that Ids make sens {e in relation to EntityState, and that no entity is DELETED
            // All these errors indicate a bug
            foreach (var entity in entities)
            {
                if (entity.EntityState == EntityStates.Deleted)
                {
                    // Won't be supported for this API
                    var index = indices[entity];
                    ModelState.AddModelError($"[{index}].{nameof(entity.EntityState)}", _localizer["Error_Deleting0IsNotSupportedFromThisAPI", _localizer["LocalUsers"]]);
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

            // Check that line ids are unique and that they have supplied a RoleId
            var duplicateLineIds = entities.SelectMany(e => e.Roles) // All lines
                .Where(e => e.Id != null).GroupBy(e => e.Id.Value).Where(g => g.Count() > 1) // Duplicate Ids
                .SelectMany(g => g).ToDictionary(e => e, e => e.Id.Value); // to dictionary

            foreach (var entity in entities)
            {
                var lineIndices = entity.Roles.ToIndexDictionary();
                foreach (var line in entity.Roles)
                {
                    if (duplicateLineIds.ContainsKey(line))
                    {
                        // This error indicates a bug
                        var index = indices[entity];
                        var lineIndex = lineIndices[line];
                        var id = duplicateLineIds[line];
                        ModelState.AddModelError($"[{index}].{nameof(entity.Roles)}[{lineIndex}].{nameof(entity.Id)}",
                            _localizer["Error_TheEntityWithId0IsSpecifiedMoreThanOnce", id]);
                    }

                    if (line.RoleId == null)
                    {
                        var index = indices[entity];
                        var lineIndex = lineIndices[line];
                        var propName = nameof(RoleMembershipForSave.RoleId);
                        var propDisplayName = _metadataProvider.GetMetadataForProperty(typeof(RoleMembershipForSave), propName)?.DisplayName ?? propName;
                        ModelState.AddModelError($"[{index}].{nameof(entity.Roles)}[{lineIndex}].{nameof(RoleMembershipForSave.RoleId)}",
                            _localizer[nameof(RequiredAttribute), propDisplayName]);
                    }
                }
            }

            // No need to invoke SQL if the model state is full of errors
            if (ModelState.HasReachedMaxErrors)
            {
                return;
            }

            // Perform SQL-side validation
            DataTable LocalUsersTable = LocalUsersDataTable(entities);
            var LocalUsersTvp = new SqlParameter("LocalUsers", LocalUsersTable) { TypeName = $"dbo.{nameof(LocalUserForSave)}List", SqlDbType = SqlDbType.Structured };

            var rolesHeaderIndices = indices.Keys.Select(LocalUser => (LocalUser.Roles, indices[LocalUser]));
            DataTable rolesTable = DataTableWithHeaderIndex(rolesHeaderIndices, e => e.EntityState != null);
            var rolesTvp = new SqlParameter("Roles", rolesTable) { TypeName = $"dbo.{nameof(RoleMembershipForSave)}List", SqlDbType = SqlDbType.Structured };

            int remainingErrorCount = ModelState.MaxAllowedErrors - ModelState.ErrorCount;

            // (1) Code must be unique
            var sqlErrors = await _db.Validation.FromSql($@"
    SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];
	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();

    INSERT INTO @ValidationErrors([Key], [ErrorName])
    SELECT '[' + CAST([Index] AS NVARCHAR(255)) + '].Id' As [Key], N'Error_CannotModifyInactiveItem' As [ErrorName]
    FROM @LocalUsers
    WHERE Id IN (SELECT Id from [dbo].[LocalUsers] WHERE IsActive = 0)
	OPTION(HASH JOIN);

    INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument1])
    SELECT '[' + CAST([Index] AS NVARCHAR(255)) + '].Id' As [Key], N'Error_TheId0WasNotFound' As [ErrorName], CAST([Id] As NVARCHAR(255)) As [Argument1]
    FROM @LocalUsers
    WHERE Id Is NOT NULL
	AND Id NOT IN (SELECT Id from [dbo].[LocalUsers])
	OPTION(HASH JOIN);
		
	-- Email must not be already in the back end
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument1], [Argument2], [Argument3], [Argument4], [Argument5]) 
	SELECT '[' + CAST(FE.[Index] AS NVARCHAR(255)) + '].Email' As [Key], N'Error_TheEmail0IsUsed' As [ErrorName],
		FE.Email AS Argument1, NULL AS Argument2, NULL AS Argument3, NULL AS Argument4, NULL AS Argument5
	FROM @LocalUsers FE 
	JOIN [dbo].LocalUsers BE ON FE.Email = BE.Email
	AND ((FE.[EntityState] = N'Inserted') OR (FE.Id <> BE.Id))
	OPTION(HASH JOIN);

	-- Email must not be duplicated in the uploaded list
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument1], [Argument2], [Argument3], [Argument4], [Argument5]) 
	SELECT '[' + CAST([Index] AS NVARCHAR(255)) + '].Email' As [Key], N'Error_TheEmail0IsDuplicated' As [ErrorName],
		[Email] AS Argument1, NULL AS Argument2, NULL AS Argument3, NULL AS Argument4, NULL AS Argument5
	FROM @LocalUsers
	WHERE [Email] IN (
		SELECT [Email]
		FROM @LocalUsers
		WHERE [Email] IS NOT NULL
		GROUP BY [Email]
		HAVING COUNT(*) > 1
	) OPTION(HASH JOIN);

    -- No email can change 
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument1], [Argument2], [Argument3], [Argument4], [Argument5]) 
	SELECT '[' + CAST(FE.[Index] AS NVARCHAR(255)) + '].Email' As [Key], N'Error_TheEmailCannotBeModified' As [ErrorName],
		NULL AS Argument1, NULL AS Argument2, NULL AS Argument3, NULL AS Argument4, NULL AS Argument5
	FROM @LocalUsers FE 
	JOIN [dbo].LocalUsers BE ON FE.Id = BE.Id
	AND ((FE.[EntityState] = N'Updated') AND (FE.Email <> BE.Email))
	OPTION(HASH JOIN);

	-- No inactive role
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument1], [Argument2], [Argument3], [Argument4], [Argument5]) 
	SELECT '[' + CAST(P.[HeaderIndex] AS NVARCHAR(255)) + '].Roles[' + 
				CAST(P.[Index] AS NVARCHAR(255)) + '].RoleId' As [Key], N'Error_TheView0IsInactive' As [ErrorName],
				P.[RoleId] AS Argument1, NULL AS Argument2, NULL AS Argument3, NULL AS Argument4, NULL AS Argument5
	FROM @Roles P
	WHERE P.RoleId NOT IN (
		SELECT [Id] FROM dbo.[Roles] WHERE IsActive = 1
		)
	AND (P.[EntityState] IN (N'Inserted', N'Updated'));
    SELECT TOP {remainingErrorCount} * FROM @ValidationErrors;
", LocalUsersTvp, rolesTvp).ToListAsync();

            // Loop over the errors returned from SQL and add them to ModelState
            foreach (var sqlError in sqlErrors)
            {
                var formatArguments = sqlError.ToFormatArguments();

                string key = sqlError.Key;
                string errorMessage = _localizer[sqlError.ErrorName, formatArguments];

                ModelState.AddModelError(key: key, errorMessage: errorMessage);
            }
        }

        private DataTable LocalUsersDataTable(List<LocalUserForSave> entities, Dictionary<string, M.GlobalUsersMatch> matches = null)
        {
            DataTable table = new DataTable();

            table.Columns.Add(new DataColumn("Index", typeof(int)));
            table.Columns.Add(new DataColumn(nameof(LocalUserForSave.Id), typeof(int)));
            table.Columns.Add(new DataColumn(nameof(LocalUserForSave.EntityState), typeof(string)));
            table.Columns.Add(new DataColumn(nameof(LocalUserForSave.Name), typeof(string)));
            table.Columns.Add(new DataColumn(nameof(LocalUserForSave.Name2), typeof(string)));
            table.Columns.Add(new DataColumn(nameof(LocalUserForSave.Email), typeof(string)));
            table.Columns.Add(new DataColumn(nameof(LocalUser.ExternalId), typeof(string)));
            table.Columns.Add(new DataColumn(nameof(LocalUserForSave.AgentId), typeof(int)));

            int index = 0;
            foreach (var entity in entities)
            {
                DataRow row = table.NewRow();

                row["Index"] = index++;
                row[nameof(LocalUserForSave.Id)] = (object)entity.Id ?? DBNull.Value;
                row[nameof(LocalUserForSave.EntityState)] = entity.EntityState;
                row[nameof(LocalUserForSave.Name)] = (object)entity.Name ?? DBNull.Value;
                row[nameof(LocalUserForSave.Name2)] = (object)entity.Name2 ?? DBNull.Value;
                row[nameof(LocalUserForSave.Email)] = (object)entity.Email ?? DBNull.Value;
                row[nameof(LocalUser.ExternalId)] = matches != null && matches.ContainsKey(entity.Email) ? (object)matches[entity.Email] : DBNull.Value; ;
                row[nameof(LocalUserForSave.AgentId)] = (object)entity.AgentId ?? DBNull.Value;

                table.Rows.Add(row);
            }

            return table;
        }

        protected override async Task<List<M.LocalUser>> PersistAsync(List<LocalUserForSave> entities, SaveArguments args)
        {
            // Make all the emails small case
            entities.ForEach(e => e.Email = e.Email.ToLower());

            // Get the inserted users
            var insertedEntities = entities.Where(e => e.EntityState == EntityStates.Inserted);

            // Query the manager DB for matching emails, here I use the CodeList user-defined table type 
            // of the manager DB, since I only want to pass a list of strings, no need to defined a new type
            var insertedEmails = insertedEntities.Select(e => new { Code = e.Email });
            var emailsTable = DataTable(insertedEmails);
            var emailsTvp = new SqlParameter("Emails", emailsTable)
            {
                TypeName = $"dbo.MCodeList",
                SqlDbType = SqlDbType.Structured
            };

            var tenantId = new SqlParameter("TenantId", _tenantIdProvider.GetTenantId());

            var globalMatches = await _adminDb.GlobalUsersMatches.FromSql($@"
    DECLARE @IndexedIds [dbo].[IdList];

    -- Insert new users
    INSERT INTO @IndexedIds([Id])
    SELECT x.[Id]
    FROM
    (
	    MERGE INTO [dbo].[GlobalUsers] AS t
	    USING (
		    SELECT [Code] as [Email] FROM @Emails 
	    ) AS s ON (t.Email = s.Email)
	    WHEN NOT MATCHED THEN
		    INSERT ([Email]) VALUES (s.[Email])
		    OUTPUT inserted.[Id] 
    ) As x;

    -- Insert memberships
    INSERT INTO [dbo].[TenantMemberships] (UserId, TenantId)
    SELECT Id, @TenantId FROM @IndexedIds;

    -- Return existing users
    SELECT E.[Code] AS [Email], 
           GU.[ExternalId] AS [ExternalId] 
    FROM [dbo].[GlobalUsers] GU JOIN @Emails E ON GU.Email = E.Code
    WHERE GU.ExternalId IS NOT NULL",
            emailsTvp, tenantId).ToDictionaryAsync(e => e.Email);

            // Add created entities
            var localUsersIndices = entities.ToIndexDictionary();
            var localUsersTable = LocalUsersDataTable(entities, globalMatches);
            var localUsersTvp = new SqlParameter("LocalUsers", localUsersTable)
            {
                TypeName = $"dbo.{nameof(LocalUserForSave)}List",
                SqlDbType = SqlDbType.Structured
            };

            // Filter out roles that haven't changed for performance
            var rolesHeaderIndices = localUsersIndices.Keys.Select(localUser => (localUser.Roles, HeaderIndex: localUsersIndices[localUser]));
            DataTable rolesTable = DataTableWithHeaderIndex(rolesHeaderIndices, e => e.EntityState != null);
            var rolesTvp = new SqlParameter("RoleMemberships", rolesTable) { TypeName = $"dbo.{nameof(RoleMembershipForSave)}List", SqlDbType = SqlDbType.Structured };

            string saveSql = $@"
-- Procedure: LocalUsers__Save
    DECLARE @IndexedIds [dbo].[IndexedIdList];
	DECLARE @TenantId int = CONVERT(INT, SESSION_CONTEXT(N'TenantId'));
	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();
	DECLARE @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));

	DELETE FROM [dbo].[RoleMemberships]
	WHERE [Id] IN (SELECT [Id] FROM @RoleMemberships WHERE [EntityState] = N'Deleted');

	INSERT INTO @IndexedIds([Index], [Id])
	SELECT x.[Index], x.[Id]
	FROM
	(
		MERGE INTO [dbo].[LocalUsers] AS t
		USING (
			SELECT 
				[Index], [Id], [Name], [Name2], [Email], [ExternalId], [AgentId]
			FROM @LocalUsers 
			WHERE [EntityState] IN (N'Inserted', N'Updated')
		) AS s ON (t.Id = s.Id)
		WHEN MATCHED 
		THEN
			UPDATE SET
				t.[Name]		    = s.[Name],
				t.[Name2]		    = s.[Name2],
				-- t.[Email]		    = s.[Email],
				-- t.[ExternalId]		= s.[ExternalId],
				t.[AgentId]	        = s.[AgentId],
				t.[ModifiedAt]	    = @Now,
				t.[ModifiedById]    = @UserId,
                t.[PermissionsVersion] = NEWID() -- in case the permissions have changed
		WHEN NOT MATCHED THEN
			INSERT (
				[TenantId], [Name], [Name2],	[Email],	[ExternalId],    [AgentId], [CreatedAt], [CreatedById], [ModifiedAt], [ModifiedById]
			)
			VALUES (
				@TenantId, s.[Name], s.[Name2], s.[Email], s.[ExternalId], s.[AgentId], @Now,		@UserId,		@Now,		@UserId
			)
			OUTPUT s.[Index], inserted.[Id] 
	) As x;

    MERGE INTO [dbo].[RoleMemberships] AS t
		USING (
			SELECT L.[Index], L.[Id], II.[Id] AS [UserId], [RoleId], [Memo]
			FROM @RoleMemberships L
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
                await _db.Database.ExecuteSqlCommandAsync(saveSql, localUsersTvp, rolesTvp);
                return new List<M.LocalUser>();
            }
            else
            {
                // If returned items are expected, append a select statement to the SQL command
                saveSql = saveSql += "SELECT * FROM @IndexedIds;";

                // Retrieve the map from Indexes to Ids
                var indexedIds = await _db.Saving.FromSql(saveSql, localUsersTvp, rolesTvp).ToListAsync();

                //// Load the entities using their Ids
                //DataTable idsTable = DataTable(indexedIds.Select(e => new { e.Id }));
                //var idsTvp = new SqlParameter("@Ids", idsTable)
                //{
                //    TypeName = $"dbo.IdList",
                //    SqlDbType = SqlDbType.Structured
                //};

                //var q = _db.LocalUsers.FromSql("SELECT * FROM dbo.[LocalUsers] WHERE Id IN (SELECT Id FROM @Ids)", idsTvp);
                var ids = indexedIds.Select(e => e.Id);
                var q = _db.LocalUsers.Where(e => ids.Contains(e.Id));
                q = Expand(q, args.Expand); // includes
                var savedEntities = await q.ToListAsync();

                // SQL Server does not guarantee order, so make sure the result is sorted according to the initial index
                Dictionary<int, int> indices = indexedIds.ToDictionary(e => e.Id, e => e.Index);
                var sortedSavedEntities = new M.LocalUser[savedEntities.Count];
                foreach (var item in savedEntities)
                {
                    int index = indices[item.Id];
                    sortedSavedEntities[index] = item;
                }

                // Return the sorted collection
                return sortedSavedEntities.ToList();
            }
        }

        protected override async Task DeleteAsync(List<int?> ids)
        {
            // Make sure the user is not deleting his/her own account
            var currentUserId = _tenantInfo.GetCurrentInfo().UserId.Value;
            if (ids.Any(id => id == currentUserId))
            {
                throw new BadRequestException(_localizer["Error_CannotDeleteYourOwnUser"].Value);
            }

            // It's unfortunate that EF Core does not support distributed transactions, so there is no
            // guarantee that deletes to both the shard and the manager will run one without the other

            // Prepare a list of Ids to delete
            DataTable idsTable = DataTable(ids.Select(e => new { Id = e }), addIndex: false);
            var idsTvp = new SqlParameter("Ids", idsTable)
            {
                TypeName = $"dbo.IdList",
                SqlDbType = SqlDbType.Structured
            };

            var tenantId = new SqlParameter("TenantId", _tenantIdProvider.GetTenantId());

            List<string> deletedEmails = new List<string>();
            using (var trxApp = await _db.Database.BeginTransactionAsync())
            {
                try
                {
                    // Delete efficiently with a SQL query and return the emails of the deleted users
                    deletedEmails = await _db.Strings.FromSql($@"
    DECLARE @Emails [dbo].[CodeList];

    INSERT INTO @Emails SELECT Email FROM [dbo].[LocalUsers] WHERE Id IN (SELECT Id FROM @Ids);

    DELETE FROM dbo.[LocalUsers] WHERE Id IN (SELECT Id FROM @Ids);

    SELECT Code as Value from @Emails;
", idsTvp).Select(e => e.Value).ToListAsync();

                    // Prepare the TVP of emails to delete from the manager
                    DataTable emailsTable = DataTable(deletedEmails.Select(e => new { Code = e }), addIndex: false);
                    var emailsTvp = new SqlParameter("Emails", emailsTable)
                    {
                        TypeName = $"dbo.CodeList",
                        SqlDbType = SqlDbType.Structured
                    };

                    using (var trxAdmin = await _adminDb.Database.BeginTransactionAsync())
                    {
                        try
                        {
                            // Delete efficiently with a SQL query
                            await _adminDb.Database.ExecuteSqlCommandAsync($@"
    DELETE FROM dbo.[TenantMemberships] 
    WHERE TenantId = @TenantId AND UserId IN (
        SELECT Id FROM [dbo].[GlobalUsers] WHERE Email IN (SELECT Code from @Emails)
    );
", emailsTvp, tenantId);

                            // Commit and return
                            trxAdmin.Commit();
                            trxApp.Commit();
                        }
                        catch (Exception ex)
                        {
                            trxApp.Rollback();
                            trxAdmin.Rollback();
                            throw ex;
                        }
                    }
                }
                catch (SqlException ex) when (IsForeignKeyViolation(ex))
                {
                    throw new BadRequestException(_localizer["Error_CannotDelete0AlreadyInUse", _localizer["LocalUser"]]);
                }
                catch (Exception ex)
                {
                    trxApp.Rollback();
                    throw ex;
                }
            }
        }

        protected override AbstractDataGrid GetImportTemplate()
        {
            throw new NotImplementedException();
        }

        protected override AbstractDataGrid DtosToAbstractGrid(GetResponse<LocalUser> response, ExportArguments args)
        {
            throw new NotImplementedException();
        }

        protected override Task<(List<LocalUserForSave>, Func<string, int?>)> ToDtosForSave(AbstractDataGrid grid, ParseArguments args)
        {
            throw new NotImplementedException();
        }

        protected override async Task CheckPermissionsForNew(IEnumerable<LocalUserForSave> newItems, Expression<Func<M.LocalUser, bool>> lambda)
        {
            // Add created entities
            DataTable entitiesTable = LocalUsersDataTable(newItems.ToList());
            var entitiesTvp = new SqlParameter("Entities", entitiesTable)
            {
                TypeName = $"dbo.{nameof(LocalUserForSave)}List",
                SqlDbType = SqlDbType.Structured
            };

            string saveSql = $@"
	DECLARE @TenantId int = CONVERT(INT, SESSION_CONTEXT(N'TenantId'));
	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();
	DECLARE @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));
	DECLARE @True BIT = 1;

    SELECT  @TenantId AS TenantId, ISNULL(E.Id, 0) AS Id, E.Name, E.Name2, E.Email, E.ExternalId, E.AgentId, NULL AS LastAccess, NEWID() AS PermissionsVersion, NEWID() AS UserSettingsVersion,
    @True AS IsActive, @Now AS CreatedAt, @UserId AS CreatedById, @UserId AS CreatedById1, @TenantId AS CreatedByTenantId , @Now AS ModifiedAt, @UserId AS ModifiedById 
    FROM @Entities E
";
            var countBeforeFilter = newItems.Count();
            var countAfterFilter = await _db.LocalUsers.FromSql(saveSql, entitiesTvp).Where(lambda).CountAsync();

            if (countBeforeFilter > countAfterFilter)
            {
                throw new ForbiddenException();
            }
        }

        protected override async Task CheckPermissionsForOld(IEnumerable<int?> entityIds, Expression<Func<M.LocalUser, bool>> lambda)
        {
            // Load the entities using their Ids
            DataTable idsTable = DataTable(entityIds.Where(e => e != null).Select(id => new { Id = id.Value }));
            var idsTvp = new SqlParameter("Ids", idsTable)
            {
                TypeName = $"dbo.IdList",
                SqlDbType = SqlDbType.Structured
            };

            // apply the lambda
            var q = _db.LocalUsers.FromSql("SELECT * FROM [dbo].[LocalUsers] WHERE Id IN (SELECT Id FROM @Ids)", idsTvp);
            int countBeforeFilter = await q.CountAsync();
            int countAfterFilter = await q.Where(lambda).CountAsync();

            if (countBeforeFilter > countAfterFilter)
            {
                throw new ForbiddenException();
            }
        }

        protected override Expression ParseSpecialFilterKeyword(string keyword, ParameterExpression param)
        {
            return ControllerUtilities.CreatedByMeFilter<M.LocalUser>(keyword, param, _tenantInfo.GetCurrentInfo().UserId.Value);
        }
    }
}
