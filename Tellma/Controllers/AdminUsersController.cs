using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
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

        private readonly AdminUsersService _service;
        private readonly ILogger _logger;
        private readonly AdminRepository _repo;

        public AdminUsersController(AdminUsersService service, ILogger<AdminUsersController> logger, AdminRepository repo) : base(logger)
        {
            _service = service;
            _logger = logger;
            _repo = repo;
        }

        [HttpGet("client")]
        public async Task<ActionResult<DataWithVersion<AdminUserSettingsForClient>>> UserSettingsForClient(CancellationToken cancellation)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                var result = await _service.UserSettingsForClient(cancellation);
                return Ok(result);
            },
            _logger);
        }

        [HttpPost("client")]
        public async Task<ActionResult<DataWithVersion<AdminUserSettingsForClient>>> SaveUserSetting(
            [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))] [Required(ErrorMessage = Constants.Error_TheField0IsRequired)] string key,
            [StringLength(2048, ErrorMessage = nameof(StringLengthAttribute))] string value)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                var result = await _service.SaveUserSetting(key, value);
                return Ok(result);
            },
            _logger);
        }

        [HttpGet("me")]
        public async Task<ActionResult<GetByIdResponse<AdminUser>>> GetMyUser(CancellationToken cancellation)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                var user = await _service.GetMyUser(cancellation);
                GetByIdResponse<AdminUser> result = TransformToResponse(user, cancellation);
                return Ok(result);
            },
            _logger);
        }

        [HttpPost("me")]
        public async Task<ActionResult<GetByIdResponse<AdminUser>>> SaveMyUser([FromBody] MyAdminUserForSave me)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                var user = await _service.SaveMyUser(me);
                Response.Headers.Set("x-admin-user-settings-version", Constants.Stale);
                GetByIdResponse<AdminUser> result = TransformToResponse(user, cancellation: default);
                return Ok(result);

            }, _logger);
        }

        [HttpPut("activate")]
        public async Task<ActionResult<EntitiesResponse<AdminUser>>> Activate([FromBody] List<int> ids, [FromQuery] ActivateArguments args)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                var serverTime = DateTimeOffset.UtcNow;
                var (data, extras) = await _service.Activate(ids: ids, args);
                var response = TransformToEntitiesResponse(data, extras, serverTime, cancellation: default);
                return Ok(response);
            },
            _logger);
        }

        [HttpPut("deactivate")]
        public async Task<ActionResult<EntitiesResponse<AdminUser>>> Deactivate([FromBody] List<int> ids, [FromQuery] DeactivateArguments args)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                var serverTime = DateTimeOffset.UtcNow;
                var (data, extras) = await _service.Deactivate(ids: ids, args);
                var response = TransformToEntitiesResponse(data, extras, serverTime, cancellation: default);
                return Ok(response);
            },
            _logger);
        }

        private GetByIdResponse<AdminUser> TransformToResponse(AdminUser me, CancellationToken cancellation)
        {
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

        protected override CrudServiceBase<AdminUserForSave, AdminUser, int> GetCrudService()
        {
            return _service;
        }

        protected override async Task OnSuccessfulSave(List<AdminUser> data, Extras extras)
        {
            var meInfo = await _repo.GetAdminUserInfoAsync(cancellation: default);
            var meId = meInfo.UserId;

            if (data.Any(e => e.Id == meId))
            {
                Response.Headers.Set("x-admin-user-settings-version", Constants.Stale);
                Response.Headers.Set("x-admin-permissions-version", Constants.Stale);
            }

            await base.OnSuccessfulSave(data, extras);
        }
    }

    public class AdminUsersService : CrudServiceBase<AdminUserForSave, AdminUser, int>
    {
        private readonly AdminRepository _repo;
        private readonly IBlobService _blobService;
        private readonly IEmailSender _emailSender;
        private readonly EmailTemplatesProvider _emailTemplates;
        private readonly GlobalOptions _options;
        private readonly IStringLocalizer _localizer;
        private readonly UserManager<EmbeddedIdentityServerUser> _userManager;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly LinkGenerator _linkGenerator;

        // This is created and disposed across multiple methods
        private TransactionScope _identityTrxScope;

        private string View => AdminUsersController.BASE_ADDRESS;

        public AdminUsersService(IHttpContextAccessor contextAccessor,
            LinkGenerator linkGenerator,
            AdminRepository repo,
            IOptions<GlobalOptions> options,
            IServiceProvider serviceProvider,
            IEmailSender emailSender,
            EmailTemplatesProvider emailTemplates,
            IStringLocalizer<Strings> localizer,
            IBlobService blobService) : base(localizer)
        {
            _contextAccessor = contextAccessor;
            _linkGenerator = linkGenerator;
            _repo = repo;
            _blobService = blobService;
            _emailSender = emailSender;
            _emailTemplates = emailTemplates;
            _options = options.Value;
            _localizer = localizer;

            // we use this trick since this is an optional dependency, it will resolve to null if 
            // the embedded identity server is not enabled
            _userManager = (UserManager<EmbeddedIdentityServerUser>)serviceProvider.GetService(typeof(UserManager<EmbeddedIdentityServerUser>));
        }

        public async Task<DataWithVersion<AdminUserSettingsForClient>> SaveUserSetting(string key, string value)
        {
            await _repo.AdminUsers__SaveSettings(key, value);
            return await UserSettingsForClient(cancellation: default);
        }

        public async Task<DataWithVersion<AdminUserSettingsForClient>> UserSettingsForClient(CancellationToken cancellation)
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

            return result;
        }

        public async Task<AdminUser> GetMyUser(CancellationToken cancellation)
        {
            int myId = (await _repo.GetAdminUserInfoAsync(cancellation)).UserId.Value;

            // Prepare the odata query
            var meyIdSingleton = new List<int> { myId };
            var me = await _repo.AdminUsers.FilterByIds(meyIdSingleton).FirstOrDefaultAsync(cancellation);

            return me;
        }

        public async Task<AdminUser> SaveMyUser([FromBody] MyAdminUserForSave me)
        {
            int myId = (await _repo.GetAdminUserInfoAsync(cancellation: default)).UserId.Value;
            var myIdSingleton = new List<int> { myId };
            var user = await _repo.AdminUsers.Expand("Permissions").FilterByIds(myIdSingleton).FirstOrDefaultAsync(cancellation: default);

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
            await SaveExecuteAsync(entities, false);
            var response = await GetMyUser(cancellation: default);

            // Commit and return
            trx.Complete();
            return response;
        }

        public Task<(List<AdminUser>, Extras)> Activate(List<int> ids, ActionArguments args)
        {
            return SetIsActive(ids, args, isActive: true);
        }

        public Task<(List<AdminUser>, Extras)> Deactivate(List<int> ids, ActionArguments args)
        {
            return SetIsActive(ids, args, isActive: false);
        }

        private async Task<(List<AdminUser>, Extras)> SetIsActive(List<int> ids, ActionArguments args, bool isActive)
        {
            // Check user permissions
            await CheckActionPermissions("IsActive", ids);

            // Execute and return
            using var trx = ControllerUtilities.CreateTransaction();
            await _repo.AdminUsers__Activate(ids, isActive);

            if (args.ReturnEntities ?? false)
            {
                var response = await GetByIds(ids, args, cancellation: default);

                trx.Complete();
                return response;
            }
            else
            {
                trx.Complete();
                return default;
            }
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

        protected override async Task<List<int>> SaveExecuteAsync(List<AdminUserForSave> entities, bool returnIds)
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
                            throw new InvalidOperationException(msg);
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

        protected override Task<IEnumerable<AbstractPermission>> UserPermissions(string action, CancellationToken cancellation)
        {
            return _repo.UserPermissions(action, View, cancellation);
        }

        protected override OrderByExpression DefaultOrderBy()
        {
            return OrderByExpression.Parse(nameof(AdminUser.Name));
        }

        private async Task<(string Subject, string Body)> MakeInvitationEmailAsync(EmbeddedIdentityServerUser identityRecipient, string nameOfRecipient)
        {
            // Use the recipient's preferred Language
            CultureInfo culture = new CultureInfo(_options.Localization?.DefaultUICulture ?? "en");
            using var _ = new CultureScope(culture);

            // Prepare the parameters
            string userId = identityRecipient.Id;
            string emailToken = await _userManager.GenerateEmailConfirmationTokenAsync(identityRecipient);
            string passwordToken = await _userManager.GeneratePasswordResetTokenAsync(identityRecipient);
            string nameOfInvitor = _localizer["AppName"];

            string callbackUrl = _linkGenerator.GetUriByPage(
                    httpContext: _contextAccessor.HttpContext ?? throw new InvalidOperationException("Unable to access the HttpContext to generate invitation links"),
                    page: "/Account/ConfirmEmail");

            //string callbackUrl = Url.Page(
            //        pageName: "/Account/ConfirmEmail",
            //        pageHandler: null,
            //        values: new { userId, code = emailToken, passwordCode = passwordToken, area = "Identity" },
            //        protocol: Request.Scheme);

            // Prepare the email
            string emailSubject = _localizer["InvitationEmailSubject0", _localizer["AppName"]];
            string emailBody = _emailTemplates.MakeInvitationEmail(
                 nameOfRecipient: nameOfRecipient,
                 nameOfInvitor: nameOfInvitor,
                 validityInDays: Constants.TokenExpiryInDays,
                 userId: userId,
                 callbackUrl: callbackUrl,
                 culture: culture);

            return (emailSubject, emailBody);
        }
    }
}
