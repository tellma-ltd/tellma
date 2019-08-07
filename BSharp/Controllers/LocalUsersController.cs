using BSharp.Controllers.DTO;
using BSharp.Controllers.Misc;
using BSharp.Data;
using BSharp.Services.BlobStorage;
using BSharp.Services.Email;
using BSharp.Services.ImportExport;
using BSharp.Services.MultiTenancy;
using BSharp.Services.OData;
using BSharp.Services.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using M = BSharp.Data.Model;


namespace BSharp.Controllers
{
    [Route("api/local-users")]
    [LoadTenantInfo]
    public class LocalUsersController : CrudControllerBase<LocalUserForSave, LocalUser, int?>
    {
        private readonly ApplicationContext _db;
        private readonly AdminContext _adminDb;
        private readonly ITenantIdAccessor _tenantIdProvider;
        private readonly IModelMetadataProvider _metadataProvider;
        private readonly ILogger<LocalUsersController> _logger;
        private readonly IEmailSender _emailSender;
        private readonly EmailTemplatesProvider _emailTemplates;
        private readonly GlobalOptions _config;
        private readonly IStringLocalizer<LocalUsersController> _localizer;
        private readonly ITenantUserInfoAccessor _tenantInfo;
        private readonly IBlobService _blobService;
        private readonly UserManager<M.User> _userManager;

        public LocalUsersController(
            ApplicationContext db,
            AdminContext adminDb,
            IModelMetadataProvider metadataProvider,
            ILogger<LocalUsersController> logger,
            IOptions<GlobalOptions> options,
            IServiceProvider serviceProvider,
            IEmailSender emailSender,
            EmailTemplatesProvider emailTemplates,
            IStringLocalizer<LocalUsersController> localizer,
            ITenantIdAccessor tenantIdProvider,
            ITenantUserInfoAccessor tenantInfo,
            IBlobService blobService) : base(logger, localizer, serviceProvider)
        {
            _db = db;
            _adminDb = adminDb;
            _tenantIdProvider = tenantIdProvider;
            _metadataProvider = metadataProvider;
            _logger = logger;
            _emailSender = emailSender;
            _emailTemplates = emailTemplates;
            _config = options.Value;
            _localizer = localizer;
            _tenantInfo = tenantInfo;
            _blobService = blobService;

            // we use this trick since this is an optional dependency, it will resolve to null if 
            // the embedded identity server is not enabled
            _userManager = (UserManager<M.User>)serviceProvider.GetService(typeof(UserManager<M.User>));
        }

        [HttpGet("{id}/image")]
        public async Task<ActionResult> GetImage(int id)
        {
            return await ControllerUtilities.ExecuteAndHandleErrorsAsync(async () =>
            {
                // Retrieve the user whose image we're about to return (This also checks the read permissions of the caller)
                var dbUserResponse = await GetByIdImplAsync(id, new GetByIdArguments { Select = nameof(LocalUser.ImageId), Expand = null });

                // Get the blob name
                var imageId = dbUserResponse.Result?.ImageId;
                //var imageId = dbUserResponse.RelatedEntities[dbUserResponse.CollectionName].Cast<LocalUser>().SingleOrDefault(e => e.Id == dbUserResponse.Result?.Id)?.ImageId;
                if (imageId != null)
                {
                    // Get the bytes
                    string blobName = BlobName(imageId);
                    var imageBytes = await _blobService.LoadBlob(blobName);

                    Response.Headers.Add("x-image-id", imageId);
                    return File(imageBytes, "image/jpeg");
                }
                else
                {
                    return NotFound("This user does not have a picture");
                }
            }, _logger);
        }

        [HttpPut("activate")]
        public async Task<ActionResult<EntitiesResponse<LocalUser>>> Activate([FromBody] List<int> ids, [FromQuery] ActivateArguments args)
        {
            return await ControllerUtilities.ExecuteAndHandleErrorsAsync(() =>
                ActivateDeactivate(ids, args.ReturnEntities ?? false, args.Expand, isActive: true)
            , _logger);
        }

        [HttpPut("deactivate")]
        public async Task<ActionResult<EntitiesResponse<LocalUser>>> Deactivate([FromBody] List<int> ids, [FromQuery] DeactivateArguments args)
        {
            return await ControllerUtilities.ExecuteAndHandleErrorsAsync(async () =>
                {
                    var currentUserId = _tenantInfo.GetCurrentInfo().UserId.Value;
                    if (ids.Any(id => id == currentUserId))
                    {
                        return BadRequest(_localizer["Error_CannotDeactivateYourOwnUser"].Value);
                    }

                    return await ActivateDeactivate(ids, args.ReturnEntities ?? false, args.Expand, isActive: false);
                }
            , _logger);
        }

        [HttpGet("client")]
        public async Task<ActionResult<DataWithVersion<UserSettingsForClient>>> UserSettingsForClient()
        {
            return await ControllerUtilities.ExecuteAndHandleErrorsAsync(async () =>
            {
                int userId = _tenantInfo.UserId();
                var user = await _db.LocalUsers.Include(e => e.Settings).FirstOrDefaultAsync(e => e.Id == userId);

                // prepare the result
                var forClient = new UserSettingsForClient
                {
                    UserId = userId,
                    Name = user.Name,
                    Name2 = user.Name2,
                    ImageId = user.ImageId,
                    CustomSettings = user.Settings.ToDictionary(e => e.Key, e => e.Value),
                };

                var result = new DataWithVersion<UserSettingsForClient>
                {
                    Version = user.UserSettingsVersion.ToString(),
                    Data = forClient
                };

                return Ok(result);
            }, _logger);
        }

        [HttpPost("client")]
        public async Task<ActionResult<DataWithVersion<UserSettingsForClient>>> SaveUserSetting(
            [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))] [Required(ErrorMessage = nameof(RequiredAttribute))] string key,
            [StringLength(2048, ErrorMessage = nameof(StringLengthAttribute))] string value)
        {
            var userId = _tenantInfo.UserId();
            var setting = await _db.LocalUserSettings.FirstOrDefaultAsync(e => e.UserId == userId && e.Key == key);
            bool hasChanged = false;

            if (string.IsNullOrWhiteSpace(value))
            {
                if (setting != null)
                {
                    // DELETE
                    _db.LocalUserSettings.Remove(setting);
                    hasChanged = true;
                }
            }
            else if (setting != null)
            {
                // UPDATE
                if(setting.Value != value)
                {
                    setting.Value = value;
                    hasChanged = true;
                }
            }
            else
            {
                // INSERT
                setting = new M.LocalUserSetting
                {
                    UserId = userId,
                    Key = key,
                    Value = value
                };

                               
                _db.LocalUserSettings.Add(setting);
                _db.Entry(setting).Property(nameof(M.ModelBase.TenantId)).CurrentValue = _tenantIdProvider.GetTenantId().Value;
                hasChanged = true;
            }

            if(hasChanged)
            {
                // Update the version
                var user = await _db.LocalUsers.FirstOrDefaultAsync(e => e.Id == userId);
                user.UserSettingsVersion = Guid.NewGuid();

                // Save all changes
                await _db.SaveChangesAsync();
            }
            return await UserSettingsForClient();
        }

        [HttpPut("invite")]
        public async Task<ActionResult> ResendInvitationEmail(int id)
        {
            return await ControllerUtilities.ExecuteAndHandleErrorsAsync(async () =>
            {
                await CheckActionPermissions(new List<int?> { id });

                var localUser = await _db.LocalUsers.FirstOrDefaultAsync(e => e.Id == id);
                if (localUser == null)
                {
                    return NotFound(id);
                }

                string toEmail = localUser.Email;
                var identityUser = await _userManager.FindByEmailAsync(toEmail);
                if (identityUser == null)
                {
                    return NotFound(toEmail);
                }

                var (subject, htmlMessage) = await MakeInvitationEmailAsync(identityUser, localUser);
                await _emailSender.SendEmailAsync(toEmail, subject, htmlMessage);
                return Ok();
            }, _logger);
        }

        private async Task<(string Subject, string Body)> MakeInvitationEmailAsync(M.User recipient, IMultilingualName recipientName)
        {
            // Load the info
            var info = _tenantInfo.GetCurrentInfo();

            // TODO: Get the preferred culture of the recipient user
            CultureInfo culture = CultureInfo.CurrentUICulture;
            var localizer = _localizer.WithCulture(culture);

            // Prepare the parameters
            string userId = recipient.Id;
            string emailToken = await _userManager.GenerateEmailConfirmationTokenAsync(recipient);
            string passwordToken = await _userManager.GeneratePasswordResetTokenAsync(recipient);
            string nameOfInvitor = info.SecondaryLanguageId == culture.Name ? info.Name2 ?? info.Name : info.Name;

            string callbackUrl = Url.Page(
                    "/Account/ConfirmEmail",
                    pageHandler: null,
                    values: new { userId, code = emailToken, passwordCode = passwordToken, area = "Identity" },
                    protocol: Request.Scheme);

            // Prepare the email
            string invitationEmail = _emailTemplates.MakeInvitationEmail(
                 nameOfRecipient: info.SecondaryLanguageId == culture.Name ? recipientName.Name2 ?? recipientName.Name : recipientName.Name,
                 nameOfInvitor: nameOfInvitor,
                 validityInDays: Constants.TokenExpiryInDays,
                 userId: userId,
                 callbackUrl: callbackUrl,
                 culture: culture
                 );

            string subject = localizer["InvitationEmailSubject0", localizer["AppName"]];
            return (subject, invitationEmail);
        }

        private async Task<ActionResult<EntitiesResponse<LocalUser>>> ActivateDeactivate([FromBody] List<int> ids, bool returnEntities, string expand, bool isActive)
        {
            var nullableIds = ids.Cast<int?>().ToArray();
            await CheckActionPermissions(nullableIds);

            var isActiveParam = new SqlParameter("@IsActive", isActive);

            DataTable idsTable = ControllerUtilities.DataTable(ids.Select(id => new { Id = id }), addIndex: false);
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
                    DataTable emailsTable = ControllerUtilities.DataTable(emails.Select(e => new { Code = e }), addIndex: false);
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
            if (returnEntities)
            {
                // Return results
                var response = await GetByIdListAsync(nullableIds, expand);
                return Ok(response);
            }
            else
            {
                // IF no returned items are expected, simply return 200 OK
                return Ok();
            }
        }

        protected override async Task<IEnumerable<AbstractPermission>> UserPermissions(string action)
        {
            var result = await ControllerUtilities.GetPermissions(_db.AbstractPermissions, action, "local-users");

            // This gives every user the ability to view their Local-User object
            if (action == Constants.Read)
            {
                var readMyUser = new AbstractPermission
                {
                    Action = "Read",
                    Criteria = "Id eq me",
                    ViewId = "local-users"
                };

                return Enumerable.Repeat(readMyUser, 1).Union(result);
            }
            else
            {
                return result;
            }
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

        protected override ODataQuery<LocalUser> Search(ODataQuery<LocalUser> query, GetArguments args, IEnumerable<AbstractPermission> filteredPermissions)
        {
            string search = args.Search;
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Replace("'", "''"); // escape quotes by repeating them

                var name = nameof(LocalUser.Name);
                var name2 = nameof(LocalUser.Name2);
                // var name3 = nameof(MeasurementUnitForQuery.Name3); // TODO
                var email = nameof(LocalUser.Email);

                query.Filter($"{name} {Ops.contains} '{search}' or {name2} {Ops.contains} '{search}' or {email} {Ops.contains} '{search}'");
            }

            return query;
        }

        protected override async Task ValidateAsync(List<LocalUserForSave> entities)
        {
            // For changing pictures, only one user at a time is allowed
            var usersWithUpdatedImgIds = entities.Where(e => e.Image != null);
            if (usersWithUpdatedImgIds.Count() > 1)
            {
                throw new BadRequestException("This API does not support changing pictures for more than one employee at a time");
            }

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
            DataTable rolesTable = ControllerUtilities.DataTableWithHeaderIndex(rolesHeaderIndices, e => e.EntityState != null);
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
                row[nameof(LocalUser.ExternalId)] = matches != null && matches.ContainsKey(entity.Email) ? (object)matches[entity.Email].ExternalId : DBNull.Value;
                row[nameof(LocalUserForSave.AgentId)] = (object)entity.AgentId ?? DBNull.Value;

                table.Rows.Add(row);
            }

            return table;
        }

        protected override Task<List<int?>> PersistAsync(List<LocalUserForSave> entitiesAndMasks, SaveArguments args)
        {
            throw new NotImplementedException();
        }

        //        protected override async Task<List<int?>> PersistAsync(List<LocalUserForSave> entities, SaveArguments args)
        //        {
        //            // Make all the emails small case
        //            entities.ForEach(e => e.Email = e.Email.ToLower());

        //            // Get the inserted users
        //            var insertedEntities = entities.Where(e => e.EntityState == EntityStates.Inserted);

        //            using (var trx = await _adminDb.Database.BeginTransactionAsync())
        //            {
        //                try
        //                {
        //                    // Query the manager DB for matching emails, here I use the CodeList user-defined table type 
        //                    // of the manager DB, since I only want to pass a list of strings, no need to defined a new type
        //                    var insertedEmails = insertedEntities.Select(e => new { Code = e.Email });
        //                    var emailsTable = ControllerUtilities.DataTable(insertedEmails);
        //                    var emailsTvp = new SqlParameter("Emails", emailsTable)
        //                    {
        //                        TypeName = $"dbo.MCodeList",
        //                        SqlDbType = SqlDbType.Structured
        //                    };

        //                    var tenantId = new SqlParameter("TenantId", _tenantIdProvider.GetTenantId());

        //                    var globalMatches = await _adminDb.GlobalUsersMatches.FromSql($@"
        //    DECLARE @IndexedIds [dbo].[IdList];

        //    -- Insert new users
        //    INSERT INTO @IndexedIds([Id])
        //    SELECT x.[Id]
        //    FROM
        //    (
        //	    MERGE INTO [dbo].[GlobalUsers] AS t
        //	    USING (
        //		    SELECT [Code] as [Email] FROM @Emails 
        //	    ) AS s ON (t.Email = s.Email)
        //	    WHEN NOT MATCHED THEN
        //		    INSERT ([Email]) VALUES (s.[Email])
        //		    OUTPUT inserted.[Id] 
        //    ) As x;

        //    -- Insert memberships
        //    INSERT INTO [dbo].[TenantMemberships] (UserId, TenantId)
        //    SELECT Id, @TenantId FROM @IndexedIds;

        //    -- Return existing users
        //    SELECT E.[Code] AS [Email], 
        //           GU.[ExternalId] AS [ExternalId] 
        //    FROM [dbo].[GlobalUsers] GU JOIN @Emails E ON GU.Email = E.Code
        //    WHERE GU.ExternalId IS NOT NULL",
        //                    emailsTvp, tenantId).ToDictionaryAsync(e => e.Email);

        //                    // Add created entities
        //                    var localUsersIndices = entities.ToIndexDictionary();
        //                    var localUsersTable = LocalUsersDataTable(entities, globalMatches);
        //                    var localUsersTvp = new SqlParameter("LocalUsers", localUsersTable)
        //                    {
        //                        TypeName = $"dbo.{nameof(LocalUserForSave)}List",
        //                        SqlDbType = SqlDbType.Structured
        //                    };

        //                    // Filter out roles that haven't changed for performance
        //                    var rolesHeaderIndices = localUsersIndices.Keys.Select(localUser => (localUser.Roles, HeaderIndex: localUsersIndices[localUser]));
        //                    DataTable rolesTable = ControllerUtilities.DataTableWithHeaderIndex(rolesHeaderIndices, e => e.EntityState != null);
        //                    var rolesTvp = new SqlParameter("RoleMemberships", rolesTable) { TypeName = $"dbo.{nameof(RoleMembershipForSave)}List", SqlDbType = SqlDbType.Structured };

        //                    string saveSql = $@"
        //-- Procedure: LocalUsers__Save
        //    DECLARE @IndexedIds [dbo].[IndexedIdList];
        //	DECLARE @TenantId int = CONVERT(INT, SESSION_CONTEXT(N'TenantId'));
        //	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();
        //	DECLARE @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));

        //	DELETE FROM [dbo].[RoleMemberships]
        //	WHERE [Id] IN (SELECT [Id] FROM @RoleMemberships WHERE [EntityState] = N'Deleted');

        //	INSERT INTO @IndexedIds([Index], [Id])
        //	SELECT x.[Index], x.[Id]
        //	FROM
        //	(
        //		MERGE INTO [dbo].[LocalUsers] AS t
        //		USING (
        //			SELECT 
        //				[Index], [Id], [Name], [Name2], [Email], [ExternalId], [AgentId]
        //			FROM @LocalUsers 
        //			WHERE [EntityState] IN (N'Inserted', N'Updated')
        //		) AS s ON (t.Id = s.Id)
        //		WHEN MATCHED 
        //		THEN
        //			UPDATE SET
        //				t.[Name]		    = s.[Name],
        //				t.[Name2]		    = s.[Name2],
        //				-- t.[Email]		    = s.[Email],
        //				-- t.[ExternalId]		= s.[ExternalId],
        //				t.[AgentId]	        = s.[AgentId],
        //				t.[ModifiedAt]	    = @Now,
        //				t.[ModifiedById]    = @UserId,
        //                t.[PermissionsVersion] = NEWID(), -- in case the permissions have changed
        //                t.[UserSettingsVersion] = NEWID() -- in case the permissions have changed
        //		WHEN NOT MATCHED THEN
        //			INSERT (
        //				[TenantId], [Name], [Name2],	[Email],	[ExternalId],    [AgentId], [CreatedAt], [CreatedById], [ModifiedAt], [ModifiedById]
        //			)
        //			VALUES (
        //				@TenantId, s.[Name], s.[Name2], s.[Email], s.[ExternalId], s.[AgentId], @Now,		@UserId,		@Now,		@UserId
        //			)
        //			OUTPUT s.[Index], inserted.[Id] 
        //	) As x;

        //    MERGE INTO [dbo].[RoleMemberships] AS t
        //		USING (
        //			SELECT L.[Index], L.[Id], II.[Id] AS [UserId], [RoleId], [Memo]
        //			FROM @RoleMemberships L
        //			JOIN @IndexedIds II ON L.[HeaderIndex] = II.[Index]
        //			WHERE L.[EntityState] IN (N'Inserted', N'Updated')
        //		) AS s ON t.Id = s.Id
        //		WHEN MATCHED THEN
        //			UPDATE SET 
        //				t.[UserId]		    = s.[UserId], 
        //				t.[RoleId]		    = s.[RoleId],
        //				t.[Memo]		    = s.[Memo],
        //				t.[ModifiedAt]	    = @Now,
        //				t.[ModifiedById]	= @UserId
        //		WHEN NOT MATCHED THEN
        //			INSERT ([TenantId], [UserId],	[RoleId],	 [Memo], [CreatedAt], [CreatedById], [ModifiedAt], [ModifiedById])
        //			VALUES (@TenantId, s.[UserId], s.[RoleId], s.[Memo], @Now,		@UserId,		@Now,		@UserId);
        //";

        //                    // Prepare the list of users whose profile picture has changed:
        //                    var usersWithModifiedImgs = entities.Where(e => e.Image != null);
        //                    bool newPictures = usersWithModifiedImgs.Any();
        //                    bool returnEntities = (args.ReturnEntities ?? false);

        //                    // Optimization
        //                    if (!returnEntities && !newPictures)
        //                    {
        //                        // IF no returned items are expected, simply execute a non-Query and return an empty list;
        //                        await _db.Database.ExecuteSqlCommandAsync(saveSql, localUsersTvp, rolesTvp);
        //                        return null;
        //                    }
        //                    else
        //                    {
        //                        // If returned items are expected, append a select statement to the SQL command
        //                        saveSql = saveSql += "SELECT * FROM @IndexedIds;";

        //                        // Retrieve the map from Indexes to Ids
        //                        var indexedIds = await _db.Saving.FromSql(saveSql, localUsersTvp, rolesTvp).ToListAsync();

        //                        // return the Ids in the same order they came
        //                        var result = indexedIds.OrderBy(e => e.Index).Select(e => (int?)e.Id).ToList();


        //                        // Hmmmmmmmmmmmmmmmmmmmmmmm

        //                        var idsString = string.Join(",", indexedIds.Select(e => e.Id));
        //                        var q = _db.VW_LocalUsers.FromSql($"SELECT * FROM [dbo].[VW_LocalUsers] WHERE Id IN (SELECT CONVERT(INT, VALUE) AS Id FROM STRING_SPLIT({idsString}, ','))");
        //                        q = Expand(q, args.Expand); // includes
        //                        var savedEntities = await q.AsNoTracking().ToListAsync();

        //                        // SQL Server does not guarantee order, so make sure the result is sorted according to the initial index
        //                        Dictionary<int, int> indices = indexedIds.ToDictionary(e => e.Id, e => e.Index);
        //                        var sortedSavedEntities = new LocalUserForQuery[savedEntities.Count];
        //                        foreach (var item in savedEntities)
        //                        {
        //                            int index = indices[item.Id.Value];
        //                            sortedSavedEntities[index] = item;
        //                        }

        //                        // The code inside here is not optimized for bulk, we assume for now
        //                        // that users will be entering images one at a time
        //                        if (newPictures)
        //                        {
        //                            var entitiesDic = entities.ToIndexDictionary();
        //                            // Retrieve blobs to delete
        //                            var blobsToDelete = usersWithModifiedImgs.Where(e => e.EntityState == EntityStates.Updated)
        //                                .Select(u => sortedSavedEntities[entitiesDic[u]].ImageId).Where(e => e != null).Select(e => BlobName(e)).ToList();


        //                            var blobsToSave = new List<(string name, byte[] content)>();
        //                            foreach (var user in usersWithModifiedImgs)
        //                            {
        //                                // Get the Id of the user
        //                                int index = entitiesDic[user];
        //                                var savedEntity = sortedSavedEntities[entitiesDic[user]];
        //                                int id = savedEntity.Id.Value;
        //                                if (user.Image.Length == 0)
        //                                {
        //                                    // We simply NULL image Id
        //                                    await _db.Database.ExecuteSqlCommandAsync($@"UPDATE [dbo].[LocalUsers] SET ImageId = NULL WHERE [Id] = {id}");
        //                                    savedEntity.ImageId = null;
        //                                }
        //                                else
        //                                {
        //                                    // We create a new Image Id
        //                                    string imageId = Guid.NewGuid().ToString();
        //                                    await _db.Database.ExecuteSqlCommandAsync($@"UPDATE [dbo].[LocalUsers] SET ImageId = {imageId} WHERE [Id] = {id}");
        //                                    savedEntity.ImageId = imageId;

        //                                    // We make the image smaller and turn it into JPEG
        //                                    var imageBytes = user.Image;
        //                                    using (var image = Image.Load(imageBytes))
        //                                    {
        //                                        // Resize to 128x128px
        //                                        image.Mutate(c => c.Resize(new ResizeOptions
        //                                        {
        //                                            // 'Max' mode maintains the aspect ratio and keeps the entire image
        //                                            Mode = ResizeMode.Max,
        //                                            Size = new Size(128),
        //                                            Position = AnchorPositionMode.Center
        //                                        }));

        //                                        // some image formats that support transparent regions
        //                                        // these regions will turn black in JPEG format unless we do this
        //                                        image.Mutate(c => c.BackgroundColor(Rgba32.White)); ;

        //                                        // Save as JPEG
        //                                        var memoryStream = new MemoryStream();
        //                                        image.SaveAsJpeg(memoryStream);
        //                                        imageBytes = memoryStream.ToArray();

        //                                        // Note: JPEG is the format of choice for photography
        //                                        // for such pictures it provides better quality at a lower size
        //                                        // Since these pictures are expected to be mostly photographs
        //                                        // we save them as JPEGs
        //                                    }

        //                                    // Add it to blobs to save
        //                                    blobsToSave.Add((BlobName(imageId), imageBytes));
        //                                }
        //                            }

        //                            // Delete the blobs retrieved earlier
        //                            if (blobsToDelete.Any())
        //                            {
        //                                await _blobService.DeleteBlobs(blobsToDelete);
        //                            }

        //                            // Save new blobs if any
        //                            if (blobsToSave.Any())
        //                            {
        //                                await _blobService.SaveBlobs(blobsToSave);
        //                            }

        //                            // Note: Since the blob service is not a transactional resource it is good to do the blob calls
        //                            // near the end to minimize the chance of modifying blobs first only to have the transaction roll back later
        //                        }

        //                        // Send invitation emails, sending emails is also a non transactional resource, so we keep it till the end
        //                        var newUsers = insertedEntities.Where(e => !globalMatches.ContainsKey(e.Email)).ToList();
        //                        int count = newUsers.Count;
        //                        if (count > 0 && _config.EmbeddedIdentityServerEnabled)
        //                        {
        //                            // NOTE: the section below is not optimized for massive bulk (e.g. 1,000+ users), but it should be 
        //                            // acceptable with the usual workloads, customers with more than 200 users are rare anyways

        //                            // The email sender parameters
        //                            var tos = new string[count];
        //                            var subjects = new string[count];
        //                            var substitutions = new Dictionary<string, string>[count];

        //                            // this loop adds the users to the identity database and prepares the invitation email parameters
        //                            for (int i = 0; i < count; i++)
        //                            {
        //                                var localUser = newUsers[i];
        //                                var email = localUser.Email;

        //                                // in case the user was added in a previous failed transaction, we try to load the email from the DB first
        //                                var identityUser = await _userManager.FindByNameAsync(email) ??
        //                                     await _userManager.FindByNameAsync(email);

        //                                // this is truly a new user, create it
        //                                if (identityUser == null)
        //                                {
        //                                    // create the identity user
        //                                    identityUser = new M.User
        //                                    {
        //                                        UserName = email,
        //                                        Email = email,

        //                                        // if the system is offline, emails are automatically confirmed
        //                                        EmailConfirmed = !_config.Online
        //                                    };

        //                                    var result = await _userManager.CreateAsync(identityUser);
        //                                    if (!result.Succeeded)
        //                                    {
        //                                        string msg = string.Join(", ", result.Errors.Select(e => e.Description));
        //                                        throw new BadRequestException($"An unexpected error occurred while creating an account for '{localUser.Name}': {msg}");
        //                                    }
        //                                }

        //                                // if the system is online: prepare an invitation email that contains an email confirmation link
        //                                if (_config.Online)
        //                                {
        //                                    // Add the email sender parameters
        //                                    var (subject, body) = await MakeInvitationEmailAsync(identityUser, localUser);
        //                                    tos[i] = email;
        //                                    subjects[i] = subject;
        //                                    substitutions[i] = new Dictionary<string, string> { { "-message-", body } };
        //                                }
        //                            }

        //                            // send all the inviation emails en masse
        //                            if (_config.Online)
        //                            {
        //                                await _emailSender.SendEmailBulkAsync(
        //                                    tos: tos.ToList(),
        //                                    subjects: subjects.ToList(),
        //                                    htmlMessage: $"-message-",
        //                                    substitutions: substitutions.ToList()
        //                                    );
        //                            }
        //                        }

        //                        trx.Commit();

        //                        // Return the saved entities if requested
        //                        if (!returnEntities)
        //                        {
        //                            return null;
        //                        }
        //                        else
        //                        {
        //                            // Return the sorted collection
        //                            return (sortedSavedEntities.ToList(), q);
        //                        }
        //                    }
        //                }
        //                catch (Exception ex)
        //                {
        //                    trx.Rollback();
        //                    throw ex;
        //                }
        //            }
        //        }

        private string BlobName(string guid)
        {
            int tenantId = _tenantIdProvider.GetTenantId().Value;
            return $"{tenantId}/LocalUsers/{guid}";
        }

        protected override async Task DeleteAsync(List<int?> ids)
        {
            // Make sure the user is not deleting his/her own account
            var currentUserId = _tenantInfo.UserId();
            if (ids.Any(id => id == currentUserId))
            {
                throw new BadRequestException(_localizer["Error_CannotDeleteYourOwnUser"].Value);
            }

            // It's unfortunate that EF Core does not support distributed transactions, so there is no
            // guarantee that deletes to both the shard and the manager will run one without the other

            // Prepare a list of Ids to delete
            DataTable idsTable = ControllerUtilities.DataTable(ids.Select(e => new { Id = e }), addIndex: false);
            var idsTvp = new SqlParameter("Ids", idsTable)
            {
                TypeName = $"dbo.IdList",
                SqlDbType = SqlDbType.Structured
            };

            var tenantId = new SqlParameter("TenantId", _tenantIdProvider.GetTenantId());

            using (var trxApp = await _db.Database.BeginTransactionAsync())
            {
                try
                {
                    // Retrieve the deleted Image Ids, and delete them near the end when we are as sure as we can
                    // that the transaction will commit, since Azure blob storage is not a transactional resource
                    var deletedImages = await _db.Strings.FromSql($@"
    SELECT [ImageId] AS Value FROM [dbo].[LocalUsers] WHERE [ImageId] IS NOT NULL AND [Id] IN (SELECT [Id] FROM @Ids)
", idsTvp).Select(e => e.Value).ToListAsync();


                    // Delete efficiently with a SQL query and return the emails of the deleted users
                    var deletedEmails = await _db.Strings.FromSql($@"
    DECLARE @Emails [dbo].[CodeList];

    INSERT INTO @Emails SELECT Email FROM [dbo].[LocalUsers] WHERE Id IN (SELECT Id FROM @Ids);

    DELETE FROM dbo.[LocalUsers] WHERE Id IN (SELECT Id FROM @Ids);

    SELECT Code AS Value from @Emails;
", idsTvp).Select(e => e.Value).ToListAsync();

                    // Prepare the TVP of emails to delete from the manager
                    DataTable emailsTable = ControllerUtilities.DataTable(deletedEmails.Select(e => new { Code = e }), addIndex: false);
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

                            // Delete the blobs just before committing the transaction
                            await _blobService.DeleteBlobs(deletedImages.Select(e => BlobName(e)));

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

        protected override (string PreambleSql, string ComposableSql, List<SqlParameter> Parameters) GetAsSql(IEnumerable<LocalUserForSave> entities)
        {
            var preambleSql =
@"DECLARE @TenantId int = CONVERT(INT, SESSION_CONTEXT(N'TenantId'));
	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();
	DECLARE @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));
	DECLARE @True BIT = 1;";

            var sql =
@"SELECT  @TenantId AS TenantId, ISNULL(E.Id, 0) AS Id, E.Name, E.Name2, E.Email, E.ExternalId, E.AgentId, NULL AS LastAccess, NEWID() AS PermissionsVersion, NEWID() AS UserSettingsVersion, NULL AS ImageId,
@True AS IsActive, @Now AS CreatedAt, @UserId AS CreatedById, @UserId AS CreatedById1, @TenantId AS CreatedByTenantId , @Now AS ModifiedAt, @UserId AS ModifiedById 
FROM @Entities E";

            // Add created entities
            DataTable entitiesTable = LocalUsersDataTable(entities.ToList());
            var entitiesTvp = new SqlParameter("Entities", entitiesTable)
            {
                TypeName = $"dbo.{nameof(LocalUserForSave)}List",
                SqlDbType = SqlDbType.Structured
            };

            var ps = new List<SqlParameter>() { entitiesTvp };

            // Return the result
            return (preambleSql, sql, ps);
        }
    }
}
