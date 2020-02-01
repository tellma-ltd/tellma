using Tellma.Controllers.Dto;
using Tellma.Controllers.Utilities;
using Tellma.Data;
using Tellma.Data.Queries;
using Tellma.Entities;
using Tellma.Services.ApiAuthentication;
using Tellma.Services.BlobStorage;
using Tellma.Services.Email;
using Tellma.Services.EmbeddedIdentityServer;
using Tellma.Services.MultiTenancy;
using Tellma.Services.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

namespace Tellma.Controllers
{
    [Route("api/" + BASE_ADDRESS)]
    [AuthorizeAccess]
    [ApplicationApi]
    public class UsersController : CrudControllerBase<UserForSave, User, int>
    {
        public const string BASE_ADDRESS = "users";

        private readonly ApplicationRepository _appRepo;
        private readonly AdminRepository _adminRepo;
        private readonly ITenantIdAccessor _tenantIdAccessor;
        private readonly IBlobService _blobService;
        private readonly IModelMetadataProvider _metadataProvider;
        private readonly ILogger _logger;
        private readonly IEmailSender _emailSender;
        private readonly EmailTemplatesProvider _emailTemplates;
        private readonly GlobalOptions _options;
        private readonly IStringLocalizer _localizer;
        private readonly UserManager<EmbeddedIdentityServerUser> _userManager;

        // This is created and disposed across multiple methods
        private TransactionScope _adminTrxScope;
        private TransactionScope _identityTrxScope;

        private string View => BASE_ADDRESS;

        public UsersController(
            ApplicationRepository appRepo,
            AdminRepository adminRepo,
            IModelMetadataProvider metadataProvider,
            ILogger<UsersController> logger,
            IOptions<GlobalOptions> options,
            IServiceProvider serviceProvider,
            IEmailSender emailSender,
            EmailTemplatesProvider emailTemplates,
            IStringLocalizer<Strings> localizer,
            ITenantIdAccessor tenantIdAccessor,
            IBlobService blobService) : base(logger, localizer)
        {
            _appRepo = appRepo;
            _adminRepo = adminRepo;
            _tenantIdAccessor = tenantIdAccessor;
            _blobService = blobService;
            _metadataProvider = metadataProvider;
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
        public async Task<ActionResult<DataWithVersion<UserSettingsForClient>>> UserSettingsForClient()
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                var (version, user, customSettings) = await _appRepo.UserSettings__Load();

                // prepare the result
                var userSettingsForClient = new UserSettingsForClient
                {
                    UserId = user.Id,
                    Name = user.Name,
                    Name2 = user.Name2,
                    Name3 = user.Name3,
                    ImageId = user.ImageId,
                    PreferredLanguage = user.PreferredLanguage,
                    CustomSettings = customSettings.ToDictionary(e => e.Key, e => e.Value)
                };

                var result = new DataWithVersion<UserSettingsForClient>
                {
                    Version = version.ToString(),
                    Data = userSettingsForClient
                };

                return Ok(result);
            }, _logger);
        }

        [HttpPost("client")]
        public async Task<ActionResult<DataWithVersion<UserSettingsForClient>>> SaveUserSetting(
            [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))] [Required(ErrorMessage = nameof(RequiredAttribute))] string key,
            [StringLength(2048, ErrorMessage = nameof(StringLengthAttribute))] string value)
        {
            await _appRepo.Users__SaveSettings(key, value);

            return await UserSettingsForClient();
        }

        [HttpPut("invite")]
        public async Task<ActionResult> ResendInvitationEmail(int id)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                if (!_options.EmailEnabled)
                {
                    // Developer mistake
                    throw new BadRequestException("Email is not enabled in this installation");
                }

                if (!this._options.EmbeddedIdentityServerEnabled)
                {
                    // Developer mistake
                    throw new BadRequestException("Embedded identity is not enabled in this installation");
                }

                // Check if the user has permission
                await CheckActionPermissions("ResendInvitationEmail", id);

                // Load the user
                var user = await _appRepo.Users.FilterByIds(id).FirstOrDefaultAsync();
                if (user == null)
                {
                    throw new NotFoundException<int>(id);
                }

                if (!string.IsNullOrWhiteSpace(user.ExternalId))
                {
                    throw new BadRequestException(_localizer["Error_User0HasAlreadyAcceptedTheInvitation", user.Email]);
                }

                string toEmail = user.Email;
                var idUser = await _userManager.FindByEmailAsync(toEmail);
                if (idUser == null)
                {
                    throw new NotFoundException<string>(toEmail);
                }

                if (idUser.EmailConfirmed)
                {
                    throw new BadRequestException(_localizer["Error_User0HasAlreadyAcceptedTheInvitation", user.Email]);
                }

                var (subject, htmlMessage) = await MakeInvitationEmailAsync(idUser, user.Name, user.Name2, user.Name3, user.PreferredLanguage);
                await _emailSender.SendEmailAsync(toEmail, subject, htmlMessage);
                return base.Ok();

            }, _logger);
        }

        [HttpGet("{id}/image")]
        public async Task<ActionResult> GetImage(int id)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                string imageId;
                if (id == _appRepo.GetUserInfo().UserId)
                {
                    // A user can always view their own image, so we bypass read permissions
                    User me = await _appRepo.Users.Filter("Id eq me").Select(nameof(Entities.User.ImageId)).FirstOrDefaultAsync();
                    imageId = me.ImageId;
                }
                else
                {
                    // GetByIdImplAsync() enforces read permissions
                    var userResponse = await GetByIdImplAsync(id, new GetByIdArguments { Select = nameof(Entities.User.ImageId) });
                    imageId = userResponse.Result.ImageId;
                }

                // Get the blob name
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

        [HttpGet("me")]
        public async Task<ActionResult<GetByIdResponse<User>>> GetMyUser()
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                GetByIdResponse<User> result = await GetMyUserImpl();
                return Ok(result);
            },
            _logger);
        }

        [HttpPost("me")]
        public async Task<ActionResult<GetByIdResponse<User>>> SaveMyUser([FromBody] MyUserForSave me)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                GetByIdResponse<User> result = await SaveMyUserImpl(me);
                Response.Headers.Set("x-user-settings-version", Constants.Stale);
                return Ok(result);

            }, _logger);
        }

        private async Task<GetByIdResponse<User>> GetMyUserImpl()
        {
            int meId = _appRepo.GetUserInfo().UserId.Value;

            // Prepare the odata query
            var me = await _appRepo.Users.FilterByIds(meId).FirstOrDefaultAsync();

            // Apply the permission masks (setting restricted fields to null) and adjust the metadata accordingly
            var relatedEntities = FlattenAndTrim(new List<User> { me }, null);

            // Return
            return new GetByIdResponse<User>
            {
                Result = me,
                CollectionName = GetCollectionName(typeof(User)),
                RelatedEntities = relatedEntities
            };

        }

        private async Task<GetByIdResponse<User>> SaveMyUserImpl([FromBody] MyUserForSave me)
        {
            int myId = _appRepo.GetUserInfo().UserId.Value;
            var user = await _appRepo.Users.Expand("Roles").FilterByIds(myId).FirstOrDefaultAsync();

            // Create a user for save
            var userForSave = new UserForSave
            {
                Id = user.Id,
                Email = user.Email,
                Name = me.Name?.Trim(),
                Name2 = me.Name2?.Trim(),
                Name3 = me.Name3?.Trim(),
                PreferredLanguage = me.PreferredLanguage?.Trim(),
                Image = me.Image,
                EntityMetadata = new EntityMetadata
                {
                    [nameof(UserForSave.Id)] = FieldMetadata.Loaded,
                    [nameof(UserForSave.Email)] = FieldMetadata.Loaded,
                    [nameof(UserForSave.Name)] = FieldMetadata.Loaded,
                    [nameof(UserForSave.Name2)] = FieldMetadata.Loaded,
                    [nameof(UserForSave.Name3)] = FieldMetadata.Loaded,
                    [nameof(UserForSave.PreferredLanguage)] = FieldMetadata.Loaded,
                    [nameof(UserForSave.Image)] = FieldMetadata.Loaded
                },

                // The roles must remain the way they are
                Roles = user.Roles?.Select(e => new RoleMembershipForSave
                {
                    Id = e.Id,
                    Memo = e.Memo,
                    RoleId = e.RoleId,
                    UserId = e.UserId,
                    EntityMetadata = new EntityMetadata
                    {
                        [nameof(RoleMembershipForSave.Id)] = FieldMetadata.Loaded,
                        [nameof(RoleMembershipForSave.Memo)] = FieldMetadata.Loaded,
                        [nameof(RoleMembershipForSave.RoleId)] = FieldMetadata.Loaded,
                        [nameof(RoleMembershipForSave.UserId)] = FieldMetadata.Loaded
                    },
                })
                .ToList()
            };

            var entities = new List<UserForSave>() { userForSave };

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
            var response = await GetMyUserImpl();

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
                    isActive: false)
            , _logger);
        }

        private async Task<ActionResult<EntitiesResponse<User>>> Activate([FromBody] List<int> ids, bool returnEntities, string expand, bool isActive)
        {
            // Parse parameters
            var expandExp = ExpandExpression.Parse(expand);
            var idsArray = ids.ToArray();

            // Check user permissions
            await CheckActionPermissions("IsActive", idsArray);

            // Execute and return
            using var trx = ControllerUtilities.CreateTransaction();
            await _appRepo.Users__Activate(ids, isActive);

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

        private async Task<(string Subject, string Body)> MakeInvitationEmailAsync(EmbeddedIdentityServerUser identityRecipient, string name, string name2, string name3, string preferredLang)
        {
            // Load the info
            var info = await _appRepo.GetTenantInfoAsync();

            // Use the recipient's preferred Language
            CultureInfo culture = string.IsNullOrWhiteSpace(preferredLang) ?
                CultureInfo.CurrentUICulture : new CultureInfo(preferredLang);
            var localizer = _localizer.WithCulture(culture);

            // Prepare the parameters
            string userId = identityRecipient.Id;
            string emailToken = await _userManager.GenerateEmailConfirmationTokenAsync(identityRecipient);
            string passwordToken = await _userManager.GeneratePasswordResetTokenAsync(identityRecipient);
            string nameOfInvitor =
                info.SecondaryLanguageId == culture.Name ? info.ShortCompanyName2 ?? info.ShortCompanyName :
                info.TernaryLanguageId == culture.Name ? info.ShortCompanyName3 ?? info.ShortCompanyName :
                info.ShortCompanyName;

            string nameOfRecipient =
                info.SecondaryLanguageId == name ? name2 ?? name :
                info.TernaryLanguageId == name ? name3 ?? name :
                name;

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
            return _appRepo;
        }

        protected override Query<User> Search(Query<User> query, GetArguments args, IEnumerable<AbstractPermission> filteredPermissions)
        {
            string search = args.Search;
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Replace("'", "''"); // escape quotes by repeating them

                var email = nameof(Entities.User.Email);
                var name = nameof(Entities.User.Name);
                var name2 = nameof(Entities.User.Name2);
                var name3 = nameof(Entities.User.Name3);
                var cs = Ops.contains;

                query = query.Filter($"{name} {cs} '{search}' or {name2} {cs} '{search}' or {name3} {cs} '{search}' or {email} {cs} '{search}'");
            }

            return query;
        }

        protected override async Task SaveValidateAsync(List<UserForSave> entities)
        {
            // Hash the indices for performance
            var indices = entities.ToIndexDictionary();

            // Check that line ids are unique and that they have supplied a RoleId
            var duplicateLineIds = entities
                .SelectMany(e => e.Roles) // All lines
                .Where(e => e.Id != 0)
                .GroupBy(e => e.Id)
                .Where(g => g.Count() > 1) // Duplicate Ids
                .SelectMany(g => g)
                .ToDictionary(e => e, e => e.Id); // to dictionary

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

            // SQL validation
            int remainingErrorCount = ModelState.MaxAllowedErrors - ModelState.ErrorCount;
            var sqlErrors = await _appRepo.Users_Validate__Save(entities, top: remainingErrorCount);

            // Add errors to model state
            ModelState.AddLocalizedErrors(sqlErrors, _localizer);
        }

        protected override Task<List<UserForSave>> SavePreprocessAsync(List<UserForSave> entities)
        {
            // Make all the emails small case
            entities.ForEach(e => e.Email = e.Email.ToLower());
            return Task.FromResult(entities);
        }

        protected override async Task<List<int>> SaveExecuteAsync(List<UserForSave> entities, ExpandExpression expand, bool returnIds)
        {
            // NOTE: this method is not optimized for massive bulk (e.g. 1,000+ users), since it relies
            // on querying identity through UserManager one email at a time but it should be acceptable
            // with the usual workloads, customers with more than 200 users are rare anyways

            // Step (1) enlist the app repo
            _appRepo.EnlistTransaction(Transaction.Current); // So that it is not affected by admin trx scope later

            // Step (2): If Embedded Identity Server is enabled, create any emails that don't already exist there
            var usersToInvite = new List<(EmbeddedIdentityServerUser IdUser, UserForSave User)>();
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

            // Step (3): Extract the images
            var (blobsToDelete, blobsToSave, imageIds) = await ImageUtilities.ExtractImages<User, UserForSave>(_appRepo, entities, BlobName);

            // Step (4): Save the users in the app database
            var ids = await _appRepo.Users__Save(entities, imageIds, returnIds);

            // Step (5): Delete old images from the blob storage
            if (blobsToDelete.Any())
            {
                await _blobService.DeleteBlobsAsync(blobsToDelete);
            }

            // Step (6): Save new images to the blob storage
            if (blobsToSave.Any())
            {
                await _blobService.SaveBlobsAsync(blobsToSave);
            }

            // Step (7) Same the emails in the admin database
            var tenantId = _tenantIdAccessor.GetTenantId();
            _adminTrxScope = ControllerUtilities.CreateTransaction(TransactionScopeOption.RequiresNew);
            _adminRepo.EnlistTransaction(Transaction.Current);
            var oldEmails = new List<string>(); // Emails are readonly after the first save
            var newEmails = entities.Where(e => e.Id == 0).Select(e => e.Email);
            await _adminRepo.GlobalUsers__Save(newEmails, oldEmails, tenantId);

            // Step (8): Send the invitation emails
            if (usersToInvite.Any()) // This will be empty if embedded identity is disabled or if email is disabled
            {
                var userIds = usersToInvite.Select(e => e.User.Id).ToArray();
                var tos = new List<string>();
                var subjects = new List<string>();
                var substitutions = new List<Dictionary<string, string>>();
                foreach (var (idUser, user) in usersToInvite)
                {
                    // Add the email sender parameters
                    var (subject, body) = await MakeInvitationEmailAsync(idUser, user.Name, user.Name2, user.Name3, user.PreferredLanguage);
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
            if (_adminTrxScope != null)
            {
                _adminTrxScope.Complete();
                _adminTrxScope.Dispose();
            }

            if (_identityTrxScope != null)
            {
                _identityTrxScope.Complete();
                _identityTrxScope.Dispose();
            }

            return Task.CompletedTask;
        }

        protected override Task OnSaveError(Exception ex)
        {
            if (_adminTrxScope != null)
            {
                _adminTrxScope.Dispose();
            }

            if (_identityTrxScope != null)
            {
                _identityTrxScope.Dispose();
            }

            return Task.CompletedTask;
        }

        protected override async Task DeleteValidateAsync(List<int> ids)
        {
            // Make sure the user is not deleting his/her own account
            var userInfo = await _appRepo.GetUserInfoAsync();
            var index = ids.IndexOf(userInfo.UserId.Value);
            if (index >= 0)
            {
                ModelState.AddModelError($"[{index}]", _localizer["Error_CannotDeleteYourOwnUser"].Value);
            }

            // SQL validation
            int remainingErrorCount = ModelState.MaxAllowedErrors - ModelState.ErrorCount;
            var sqlErrors = await _appRepo.Users_Validate__Delete(ids, top: remainingErrorCount);

            // Add errors to model state
            ModelState.AddLocalizedErrors(sqlErrors, _localizer);
        }

        protected override async Task DeleteExecuteAsync(List<int> ids)
        {
            try
            {
                // It's unfortunate that EF Core does not support distributed transactions, so there is no
                // guarantee that deletes to both the application and the admin will run one without the other
                using var appTrx = ControllerUtilities.CreateTransaction();
                _appRepo.EnlistTransaction(Transaction.Current);
                var oldEmails = await _appRepo.Users__Delete(ids);

                using var adminTrx = ControllerUtilities.CreateTransaction(TransactionScopeOption.RequiresNew);
                var newEmails = new List<string>();
                var tenantId = _tenantIdAccessor.GetTenantId();

                await _adminRepo.GlobalUsers__Save(newEmails, oldEmails, tenantId);

                appTrx.Complete();
                adminTrx.Complete();
            }
            catch (ForeignKeyViolationException)
            {
                throw new BadRequestException(_localizer["Error_CannotDelete0AlreadyInUse", _localizer["User"]]);
            }
        }

        protected override Query<User> GetAsQuery(List<UserForSave> entities)
        {
            return _appRepo.Users__AsQuery(entities);
        }

        protected override Task<IEnumerable<AbstractPermission>> UserPermissions(string action)
        {
            return _appRepo.UserPermissions(action, View);
        }

        private string BlobName(string guid)
        {
            int tenantId = _tenantIdAccessor.GetTenantId();
            return $"{tenantId}/Users/{guid}";
        }
    }
}
