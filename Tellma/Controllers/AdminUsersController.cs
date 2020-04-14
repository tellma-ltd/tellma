using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Tellma.Controllers.Dto;
using Tellma.Controllers.Utilities;
using Tellma.Data;
using Tellma.Data.Queries;
using Tellma.Entities;
using Tellma.Services.ApiAuthentication;
using Tellma.Services.BlobStorage;
using Tellma.Services.Email;
using Tellma.Services.EmbeddedIdentityServer;
using Tellma.Services.Utilities;

namespace Tellma.Controllers
{
    [Route("api/" + BASE_ADDRESS)]
    [AuthorizeAccess]
    [AdminController]
    public class AdminUsersController : CrudControllerBase<AdminUserForSave, AdminUser, int>
    {
        public const string BASE_ADDRESS = "admin-users";

        private readonly AdminRepository _repo;
        private readonly IBlobService _blobService;
        private readonly ILogger _logger;
        private readonly IEmailSender _emailSender;
        private readonly EmailTemplatesProvider _emailTemplates;
        private readonly GlobalOptions _options;
        private readonly IStringLocalizer _localizer;
        private readonly UserManager<EmbeddedIdentityServerUser> _userManager;

        // This is created and disposed across multiple methods
        private TransactionScope _identityTrxScope;

        private string View => BASE_ADDRESS;

        public AdminUsersController(
            AdminRepository repo,
            ILogger<UsersController> logger,
            IOptions<GlobalOptions> options,
            IServiceProvider serviceProvider,
            IEmailSender emailSender,
            EmailTemplatesProvider emailTemplates,
            IStringLocalizer<Strings> localizer,
            IBlobService blobService) : base(logger, localizer)
        {
            _repo = repo;
            _blobService = blobService;
            _logger = logger;
            _emailSender = emailSender;
            _emailTemplates = emailTemplates;
            _options = options.Value;
            _localizer = localizer;

            // we use this trick since this is an optional dependency, it will resolve to null if 
            // the embedded identity server is not enabled
            _userManager = (UserManager<EmbeddedIdentityServerUser>)serviceProvider.GetService(typeof(UserManager<EmbeddedIdentityServerUser>));
        }

        [HttpGet("client")]
        public async Task<ActionResult<DataWithVersion<AdminUserSettingsForClient>>> UserSettingsForClient(CancellationToken cancellation)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                var (version, user, customSettings) = await _repo.UserSettings__Load(cancellation);

                // prepare the result
                var userSettingsForClient = new AdminUserSettingsForClient
                {
                    UserId = user.Id,
                    Name = user.Name,
                    CustomSettings = customSettings.ToDictionary(e => e.Key, e => e.Value)
                };

                var result = new DataWithVersion<AdminUserSettingsForClient>
                {
                    Version = version.ToString(),
                    Data = userSettingsForClient
                };

                return Ok(result);
            }, _logger);
        }

        [HttpPost("client")]
        public async Task<ActionResult<DataWithVersion<AdminUserSettingsForClient>>> SaveUserSetting(
            [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))] [Required(ErrorMessage = Services.Utilities.Constants.Error_TheField0IsRequired)] string key,
            [StringLength(2048, ErrorMessage = nameof(StringLengthAttribute))] string value)
        {
            await _repo.AdminUsers__SaveSettings(key, value);

            return await UserSettingsForClient(cancellation: default);
        }

        //[HttpPut("invite")]
        //public async Task<ActionResult> ResendInvitationEmail(int id)
        //{
        //    return await ControllerUtilities.InvokeActionImpl(async () =>
        //    {
        //        if (!_options.EmailEnabled)
        //        {
        //            // Developer mistake
        //            throw new BadRequestException("Email is not enabled in this installation");
        //        }

        //        if (!_options.EmbeddedIdentityServerEnabled)
        //        {
        //            // Developer mistake
        //            throw new BadRequestException("Embedded identity is not enabled in this installation");
        //        }

        //        // Check if the user has permission
        //        await CheckActionPermissions("ResendInvitationEmail", id);

        //        // Load the user
        //        var user = await _repo.AdminUsers.FilterByIds(id).FirstOrDefaultAsync();
        //        if (user == null)
        //        {
        //            throw new NotFoundException<int>(id);
        //        }

        //        if (!string.IsNullOrWhiteSpace(user.ExternalId))
        //        {
        //            throw new BadRequestException(_localizer["Error_User0HasAlreadyAcceptedTheInvitation", user.Email]);
        //        }

        //        string toEmail = user.Email;
        //        var idUser = await _userManager.FindByEmailAsync(toEmail);
        //        if (idUser == null)
        //        {
        //            throw new NotFoundException<string>(toEmail);
        //        }

        //        if (idUser.EmailConfirmed)
        //        {
        //            throw new BadRequestException(_localizer["Error_User0HasAlreadyAcceptedTheInvitation", user.Email]);
        //        }

        //        var (subject, htmlMessage) = await MakeInvitationEmailAsync(idUser, user.Name, user.Name2, user.Name3, user.PreferredLanguage);
        //        await _emailSender.SendEmailAsync(toEmail, subject, htmlMessage);
        //        return base.Ok();

        //    }, _logger);
        //}

        [HttpGet("me")]
        public async Task<ActionResult<GetByIdResponse<AdminUser>>> GetMyUser(CancellationToken cancellation)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                GetByIdResponse<AdminUser> result = await GetMyUserImpl(cancellation);
                return Ok(result);
            },
            _logger);
        }

        [HttpPost("me")]
        public async Task<ActionResult<GetByIdResponse<AdminUser>>> SaveMyUser([FromBody] MyAdminUserForSave me)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                GetByIdResponse<AdminUser> result = await SaveMyUserImpl(me);
                Response.Headers.Set("x-admin-user-settings-version", Constants.Stale);
                return Ok(result);

            }, _logger);
        }

        private async Task<GetByIdResponse<AdminUser>> GetMyUserImpl(CancellationToken cancellation)
        {
            int meId = (await _repo.GetAdminUserInfoAsync(cancellation)).UserId.Value;

            // Prepare the odata query
            var me = await _repo.AdminUsers.FilterByIds(meId).FirstOrDefaultAsync(cancellation);

            // Apply the permission masks (setting restricted fields to null) and adjust the metadata accordingly
            var relatedEntities = FlattenAndTrim(new List<AdminUser> { me }, cancellation);

            // Return
            return new GetByIdResponse<AdminUser>
            {
                Result = me,
                CollectionName = GetCollectionName(typeof(AdminUser)),
                RelatedEntities = relatedEntities
            };

        }

        private async Task<GetByIdResponse<AdminUser>> SaveMyUserImpl([FromBody] MyAdminUserForSave me)
        {
            int myId = (await _repo.GetAdminUserInfoAsync(cancellation: default)).UserId.Value;
            var user = await _repo.AdminUsers.Expand("Permissions").FilterByIds(myId).FirstOrDefaultAsync(cancellation: default);

            // Create a user for save
            var userForSave = new AdminUserForSave
            {
                Id = user.Id,
                Email = user.Email,
                Name = me.Name?.Trim(),
                EntityMetadata = new EntityMetadata
                {
                    [nameof(AdminUserForSave.Id)] = FieldMetadata.Loaded,
                    [nameof(AdminUserForSave.Email)] = FieldMetadata.Loaded,
                    [nameof(AdminUserForSave.Name)] = FieldMetadata.Loaded
                },

                // The roles must remain the way they are
                Permissions = user.Permissions?.Select(e => new AdminPermissionForSave
                {
                    Id = e.Id,
                    Action = e.Action,
                    Criteria = e.Criteria,
                    View = e.View,
                    Memo = e.Memo,
                    EntityMetadata = new EntityMetadata
                    {
                        [nameof(AdminPermissionForSave.Id)] = FieldMetadata.Loaded,
                        [nameof(AdminPermissionForSave.Action)] = FieldMetadata.Loaded,
                        [nameof(AdminPermissionForSave.Criteria)] = FieldMetadata.Loaded,
                        [nameof(AdminPermissionForSave.View)] = FieldMetadata.Loaded,
                        [nameof(AdminPermissionForSave.Memo)] = FieldMetadata.Loaded,
                    },
                })
                .ToList()
            };

            var entities = new List<AdminUserForSave>() { userForSave };

            // Start a transaction scope for save since it causes data modifications
            using var trx = ControllerUtilities.CreateTransaction(null, GetSaveTransactionOptions());

            // Validation
            await SaveValidateAsync(entities);
            if (!ModelState.IsValid)
            {
                // TODO map the errors
                throw new UnprocessableEntityException(ModelState);
            }

            // Save and retrieve response
            await SaveExecuteAsync(entities, null, false);
            var response = await GetMyUserImpl(cancellation: default);

            // Commit and return
            trx.Complete();
            return response;
        }

        [HttpPut("activate")]
        public async Task<ActionResult<EntitiesResponse<User>>> Activate([FromBody] List<int> ids, [FromQuery] ActivateArguments args)
        {
            bool returnEntities = args.ReturnEntities ?? false;

            return await ControllerUtilities.InvokeActionImpl(() =>
                Activate(ids: ids,
                    returnEntities: returnEntities,
                    expand: args.Expand,
                    select: args.Select,
                    isActive: true)
            , _logger);
        }

        [HttpPut("deactivate")]
        public async Task<ActionResult<EntitiesResponse<User>>> Deactivate([FromBody] List<int> ids, [FromQuery] DeactivateArguments args)
        {
            bool returnEntities = args.ReturnEntities ?? false;

            return await ControllerUtilities.InvokeActionImpl(() =>
                Activate(ids: ids,
                    returnEntities: returnEntities,
                    expand: args.Expand,
                    select: args.Select,
                    isActive: false)
            , _logger);
        }

        private async Task<ActionResult<EntitiesResponse<User>>> Activate(List<int> ids, bool returnEntities, string expand, string select, bool isActive)
        {
            // Parse parameters
            var expandExp = ExpandExpression.Parse(expand);
            var selectExp = SelectExpression.Parse(select);
            var idsArray = ids.ToArray();

            // Check user permissions
            await CheckActionPermissions("IsActive", idsArray);

            // Execute and return
            using var trx = ControllerUtilities.CreateTransaction();
            await _repo.AdminUsers__Activate(ids, isActive);

            if (returnEntities)
            {
                var response = await LoadDataByIdsAndTransform(idsArray, expandExp, selectExp);

                trx.Complete();
                return Ok(response);
            }
            else
            {
                trx.Complete();
                return Ok();
            }
        }

        private async Task<(string Subject, string Body)> MakeInvitationEmailAsync(EmbeddedIdentityServerUser identityRecipient, string nameOfRecipient)
        {
            // Use the recipient's preferred Language
            CultureInfo culture = new CultureInfo(_options.Localization?.DefaultUICulture ?? "en");
            var localizer = _localizer.WithCulture(culture);

            // Prepare the parameters
            string userId = identityRecipient.Id;
            string emailToken = await _userManager.GenerateEmailConfirmationTokenAsync(identityRecipient);
            string passwordToken = await _userManager.GeneratePasswordResetTokenAsync(identityRecipient);
            string nameOfInvitor = _localizer["AppName"];

            string callbackUrl = Url.Page(
                    "/Account/ConfirmEmail",
                    pageHandler: null,
                    values: new { userId, code = emailToken, passwordCode = passwordToken, area = "Identity" },
                    protocol: Request.Scheme);

            // Prepare the email
            string emailSubject = localizer["InvitationEmailSubject0", localizer["AppName"]];
            string emailBody = _emailTemplates.MakeInvitationEmail(
                 nameOfRecipient: nameOfRecipient,
                 nameOfInvitor: nameOfInvitor,
                 validityInDays: Constants.TokenExpiryInDays,
                 userId: userId,
                 callbackUrl: callbackUrl,
                 culture: culture);

            return (emailSubject, emailBody);
        }

        protected override IRepository GetRepository()
        {
            return _repo;
        }

        protected override Query<AdminUser> Search(Query<AdminUser> query, GetArguments args, IEnumerable<AbstractPermission> filteredPermissions)
        {
            string search = args.Search;
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Replace("'", "''"); // escape quotes by repeating them

                var email = nameof(Entities.AdminUser.Email);
                var name = nameof(Entities.AdminUser.Name);
                var cs = Ops.contains;

                query = query.Filter($"{name} {cs} '{search}' or {email} {cs} '{search}'");
            }

            return query;
        }

        protected override async Task SaveValidateAsync(List<AdminUserForSave> entities)
        {
            // Hash the indices for performance
            var indices = entities.ToIndexDictionary();

            // Check that line ids are unique and that they have supplied a RoleId
            var duplicateLineIds = entities
                .SelectMany(e => e.Permissions) // All lines
                .Where(e => e.Id != 0)
                .GroupBy(e => e.Id)
                .Where(g => g.Count() > 1) // Duplicate Ids
                .SelectMany(g => g)
                .ToDictionary(e => e, e => e.Id); // to dictionary

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

            // No need to invoke SQL if the model state is full of errors
            if (ModelState.HasReachedMaxErrors)
            {
                return;
            }

            // SQL validation
            int remainingErrorCount = ModelState.MaxAllowedErrors - ModelState.ErrorCount;
            var sqlErrors = await _repo.AdminUsers_Validate__Save(entities, top: remainingErrorCount);

            // Add errors to model state
            ModelState.AddLocalizedErrors(sqlErrors, _localizer);
        }

        protected override Task<List<AdminUserForSave>> SavePreprocessAsync(List<AdminUserForSave> entities)
        {
            // Make all the emails small case
            entities.ForEach(e => e.Email = e.Email.ToLower());
            return Task.FromResult(entities);
        }

        protected override async Task<List<int>> SaveExecuteAsync(List<AdminUserForSave> entities, ExpandExpression expand, bool returnIds)
        {
            // NOTE: this method is not optimized for massive bulk (e.g. 1,000+ users), since it relies
            // on querying identity through UserManager one email at a time but it should be acceptable
            // with the usual workloads, customers with more than 200 users are rare anyways

            // Step (1) enlist the admin repo
            _repo.EnlistTransaction(Transaction.Current); // So that it is not affected by identity trx scope later

            // Step (2): If Embedded Identity Server is enabled, create any emails that don't already exist there
            var usersToInvite = new List<(EmbeddedIdentityServerUser IdUser, AdminUserForSave User)>();
            if (_options.EmbeddedIdentityServerEnabled)
            {
                _identityTrxScope = ControllerUtilities.CreateTransaction(TransactionScopeOption.RequiresNew);

                foreach (var entity in entities)
                {
                    var email = entity.Email;

                    // In case the user was added in a previous failed transaction
                    // or something, we always try to be forgiving in the code
                    var identityUser = await _userManager.FindByNameAsync(email) ??
                         await _userManager.FindByEmailAsync(email);

                    // This is truly a new user, create it
                    if (identityUser == null)
                    {
                        // Create the identity user
                        identityUser = new EmbeddedIdentityServerUser
                        {
                            UserName = email,
                            Email = email,
                            EmailConfirmed = !_options.EmailEnabled

                            // Note: If the system is integrated with an email service, user emails
                            // are automatically confirmed, otherwise users must confirm their 
                        };

                        var result = await _userManager.CreateAsync(identityUser);
                        if (!result.Succeeded)
                        {
                            string msg = string.Join(", ", result.Errors.Select(e => e.Description));
                            _logger.LogError(msg);

                            throw new BadRequestException($"An unexpected error occurred while creating an account for '{email}'");
                        }
                    }

                    // Mark for invitation later 
                    if (!identityUser.EmailConfirmed)
                    {
                        usersToInvite.Add((identityUser, entity));
                    }
                }
            }

            // Step (3): Save the users in the database
            var ids = await _repo.AdminUsers__Save(entities, returnIds); // Synchronizes with DirectoryUsers automatically

            // Step (4): Send the invitation emails
            if (usersToInvite.Any()) // This will be empty if embedded identity is disabled or if email is disabled
            {
                var userIds = usersToInvite.Select(e => e.User.Id).ToArray();
                var tos = new List<string>();
                var subjects = new List<string>();
                var substitutions = new List<Dictionary<string, string>>();
                foreach (var (idUser, user) in usersToInvite)
                {
                    // Add the email sender parameters
                    var (subject, body) = await MakeInvitationEmailAsync(idUser, user.Name);
                    tos.Add(idUser.Email);
                    subjects.Add(subject);
                    substitutions.Add(new Dictionary<string, string> { { "-message-", body } });
                }

                await _emailSender.SendEmailBulkAsync(
                    tos: tos,
                    subjects: subjects,
                    htmlMessage: $"-message-",
                    substitutions: substitutions.ToList()
                    );
            }

            // Signal the client to refresh some cached stuff
            int meId = (await _repo.GetAdminUserInfoAsync(cancellation: default)).UserId.Value;
            if (entities.Any(e => e.Id == meId))
            {
                Response.Headers.Set("x-admin-user-settings-version", Constants.Stale);
                Response.Headers.Set("x-admin-permissions-version", Constants.Stale);
            }

            // Return the new Ids
            return ids;
        }

        protected override Task OnSaveCompleted()
        {
            if (_identityTrxScope != null)
            {
                _identityTrxScope.Complete();
                _identityTrxScope.Dispose();
            }

            return Task.CompletedTask;
        }

        protected override Task OnSaveError(Exception ex)
        {
            if (_identityTrxScope != null)
            {
                _identityTrxScope.Dispose();
            }

            return Task.CompletedTask;
        }

        protected override async Task DeleteValidateAsync(List<int> ids)
        {
            // Make sure the user is not deleting his/her own account
            var userInfo = await _repo.GetAdminUserInfoAsync(cancellation: default);
            var index = ids.IndexOf(userInfo.UserId.Value);
            if (index >= 0)
            {
                ModelState.AddModelError($"[{index}]", _localizer["Error_CannotDeleteYourOwnUser"].Value);
            }

            // SQL validation
            int remainingErrorCount = ModelState.MaxAllowedErrors - ModelState.ErrorCount;
            var sqlErrors = await _repo.AdminUsers_Validate__Delete(ids, top: remainingErrorCount);

            // Add errors to model state
            ModelState.AddLocalizedErrors(sqlErrors, _localizer);
        }

        protected override async Task DeleteExecuteAsync(List<int> ids)
        {
            try
            {
                await _repo.AdminUsers__Delete(ids); // Should delete old emails from DirectoryUsers
            }
            catch (ForeignKeyViolationException)
            {
                throw new BadRequestException(_localizer["Error_CannotDelete0AlreadyInUse", _localizer["AdminUser"]]);
            }
        }

        protected override Query<AdminUser> GetAsQuery(List<AdminUserForSave> entities)
        {
            throw new NotImplementedException(nameof(GetAsQuery));
        }

        protected override Task<IEnumerable<AbstractPermission>> UserPermissions(string action, CancellationToken cancellation)
        {
            return _repo.UserPermissions(action, View, cancellation);
        }

        protected override OrderByExpression DefaultOrderBy()
        {
            return OrderByExpression.Parse(nameof(AdminUser.Name));
        }
    }
}
