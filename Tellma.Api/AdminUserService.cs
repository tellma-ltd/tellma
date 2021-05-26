using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Tellma.Model.Admin;
using Tellma.Model.Common;
using Tellma.Repository.Admin;
using Tellma.Repository.Common;

namespace Tellma.Api
{
    public class AdminUsersService : CrudServiceBase<AdminUserForSave, AdminUser, int>
    {

    }


    public class AdminUsersServiceOld : CrudServiceBase<AdminUserForSave, AdminUser, int>
    {
        private readonly AdminRepository _repo;
        private readonly MetadataProvider _metadataProvider;
        private readonly IEmailSender _emailSender;
        private readonly EmailTemplatesProvider _emailTemplates;
        private readonly GlobalOptions _options;
        private readonly UserManager<EmbeddedIdentityServerUser> _userManager;

        // This is created and used across multiple methods
        private TransactionScope _identityTrxScope;
        private List<(EmbeddedIdentityServerUser IdUser, AdminUserForSave User)> _usersToInvite;

        private IUrlHelper _urlHelper = null;
        private string _scheme = null;

        public AdminUsersService SetUrlHelper(IUrlHelper urlHelper)
        {
            _urlHelper = urlHelper;
            return this;
        }

        public AdminUsersService SetScheme(string scheme)
        {
            _scheme = scheme;
            return this;
        }

        private string View => AdminUsersController.BASE_ADDRESS;

        public AdminUsersService(
            AdminRepository repo,
            IOptions<GlobalOptions> options,
            IServiceProvider serviceProvider,
            IEmailSender emailSender,
            EmailTemplatesProvider emailTemplates,
            MetadataProvider metadataProvider) : base(serviceProvider)
        {
            _repo = repo;
            _metadataProvider = metadataProvider;
            _emailSender = emailSender;
            _emailTemplates = emailTemplates;
            _options = options.Value;

            // we use this trick since this is an optional dependency, it will resolve to null if 
            // the embedded identity server is not enabled
            _userManager = (UserManager<EmbeddedIdentityServerUser>)serviceProvider.GetService(typeof(UserManager<EmbeddedIdentityServerUser>));
        }

        public async Task<Versioned<AdminUserSettingsForClient>> SaveUserSetting(SaveUserSettingsArguments args)
        {
            // Retrieve the arguments
            var key = args.Key;
            var value = args.Value;

            // Validation
            int maxKey = 255;
            int maxValue = 2048;

            if (string.IsNullOrWhiteSpace(key))
            {
                // Key is required
                throw new ServiceException(_localizer[Constants.Error_Field0IsRequired, nameof(args.Key)]);
            }
            else if (key.Length > maxKey)
            {
                // 
                throw new ServiceException(_localizer[Constants.Error_Field0LengthMaximumOf1, nameof(args.Key), maxKey]);
            }

            if (value != null && value.Length > maxValue)
            {
                throw new ServiceException(_localizer[Constants.Error_Field0LengthMaximumOf1, nameof(args.Value), maxValue]);
            }

            // Save and return
            await _repo.AdminUsers__SaveSettings(key, value);
            return await UserSettingsForClient(cancellation: default);
        }

        public async Task<Versioned<AdminUserSettingsForClient>> UserSettingsForClient(CancellationToken cancellation)
        {
            var (version, user, customSettings) = await _repo.UserSettings__Load(cancellation);

            // prepare the result
            var userSettingsForClient = new AdminUserSettingsForClient
            {
                UserId = user.Id,
                Name = user.Name,
                CustomSettings = customSettings.ToDictionary(e => e.Key, e => e.Value)
            };

            var result = new Versioned<AdminUserSettingsForClient>
            (
                version: version.ToString(),
                data: userSettingsForClient
            );

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

        public async Task<AdminUser> SaveMyUser(MyAdminUserForSave me)
        {
            // Basic validation
            var meta = _metadataProvider.GetMetadata(null, typeof(MyAdminUserForSave));
            ValidateEntity(me, meta);
            ModelState.ThrowIfInvalid();

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

            // Perform side effects of save that are not transactional, just before committing the transaction
            await NonTransactionalSideEffectsForSave(entities, new List<AdminUser> { response });

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
            var action = "IsActive";
            var actionFilter = await UserPermissionsFilter(action, cancellation: default);
            ids = await CheckActionPermissionsBefore(actionFilter, ids);

            // User cannot deactivate themselves
            var userInfo = await _repo.GetAdminUserInfoAsync(cancellation: default);
            var userId = userInfo.UserId.Value;
            foreach (var (id, index) in ids.Select((id, index) => (id, index)))
            {
                if (id == userId)
                {
                    ModelState.AddModelError($"[{index}]", _localizer["Error_CannotDeactivateYourOwnUser"].Value);

                    if (ModelState.HasReachedMaxErrors)
                    {
                        break;
                    }
                }
            }

            // Execute and return
            using var trx = ControllerUtilities.CreateTransaction();
            await _repo.AdminUsers__Activate(ids, isActive);

            List<AdminUser> data = null;
            Extras extras = null;

            if (args.ReturnEntities ?? false)
            {
                (data, extras) = await GetByIds(ids, args, action, cancellation: default);
            }

            // Check user permissions again
            await CheckActionPermissionsAfter(actionFilter, ids, data);

            trx.Complete();
            return (data, extras);
        }

        protected override IRepository GetRepository()
        {
            return _repo;
        }

        protected override EntityQuery<AdminUser> Search(EntityQuery<AdminUser> query, GetArguments args)
        {
            string search = args.Search;
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Replace("'", "''"); // escape quotes by repeating them

                var email = nameof(AdminUser.Email);
                var name = nameof(AdminUser.Name);

                query = query.Filter($"{name} contains '{search}' or {email} contains '{search}'");
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
            int remainingErrorCount = ValidationErrorsDictionary.MaxAllowedErrors - ModelState.ErrorCount;
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
            _usersToInvite = new List<(EmbeddedIdentityServerUser IdUser, AdminUserForSave User)>();
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
                        _usersToInvite.Add((identityUser, entity));
                    }
                }
            }

            // Step (3): Save the users in the database
            var ids = await _repo.AdminUsers__Save(entities, returnIds); // Synchronizes with DirectoryUsers automatically

            // Return the new Ids
            return ids;
        }

        protected override async Task NonTransactionalSideEffectsForSave(List<AdminUserForSave> entities, List<AdminUser> data)
        {
            // Step (4): Send the invitation emails
            if (_usersToInvite.Any()) // This will be empty if embedded identity is disabled or if email is disabled
            {
                var emails = new List<Email>(_usersToInvite.Count);

                foreach (var (idUser, user) in _usersToInvite)
                {
                    // Add the email sender parameters
                    var (subject, body) = await MakeInvitationEmailAsync(idUser, user.Name);

                    emails.Add(new Email(toEmail: idUser.Email)
                    {
                        Subject = subject,
                        Body = body
                    });
                }

                await _emailSender.SendBulkAsync(emails);
            }
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
            int remainingErrorCount = ValidationErrorsDictionary.MaxAllowedErrors - ModelState.ErrorCount;
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
                throw new ServiceException(_localizer["Error_CannotDelete0AlreadyInUse", _localizer["AdminUser"]]);
            }
        }

        protected override Task<IEnumerable<AbstractPermission>> UserPermissions(string action, CancellationToken cancellation)
        {
            return _repo.UserPermissions(action, View, cancellation); // TODO: Cache
        }

        protected override ExpressionOrderBy DefaultOrderBy()
        {
            return ExpressionOrderBy.Parse(nameof(AdminUser.Name));
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

            //string callbackUrl = _linkGenerator.GetUriByPage(
            //        httpContext: _contextAccessor.HttpContext ?? throw new InvalidOperationException("Unable to access the HttpContext to generate invitation links"),
            //        page: "/Account/ConfirmEmail");

            string callbackUrl = _urlHelper.Page(
                    pageName: "/Account/ConfirmEmail",
                    pageHandler: null,
                    values: new { userId, code = emailToken, passwordCode = passwordToken, area = "Identity" },
                    protocol: _scheme);

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
