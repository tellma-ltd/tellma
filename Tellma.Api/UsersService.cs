using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Tellma.Api.Base;
using Tellma.Api.Behaviors;
using Tellma.Api.Dto;
using Tellma.Api.ImportExport;
using Tellma.Api.Metadata;
using Tellma.Model.Application;
using Tellma.Model.Common;
using Tellma.Repository.Admin;
using Tellma.Repository.Common;
using Tellma.Utilities.Blobs;
using Tellma.Utilities.Common;
using Tellma.Utilities.Email;
using Tellma.Utilities.Sms;

namespace Tellma.Api
{
    public class UsersService : CrudServiceBase<UserForSave, User, int>
    {
        private static readonly PhoneAttribute phoneAtt = new();
        private static readonly EmailAddressAttribute emailAtt = new();

        private readonly ApplicationFactServiceBehavior _behavior;
        private readonly IStringLocalizer _localizer;
        private readonly AdminRepository _adminRepo;
        private readonly IClientProxy _client;
        private readonly IIdentityProxy _identity;
        private readonly IBlobService _blobService;
        private readonly IUserSettingsCache _userSettingsCache;
        private readonly MetadataProvider _metadataProvider;

        // These are created and used across multiple methods
        private List<string> _blobsToDelete;
        private List<(string, byte[])> _blobsToSave;

        protected override string View => "users";

        protected override IFactServiceBehavior FactBehavior => _behavior;

        public UsersService(
            ApplicationFactServiceBehavior behavior,
            CrudServiceDependencies deps,
            IStringLocalizer<Strings> localizer,
            AdminRepository adminRepo,
            IClientProxy client,
            IIdentityProxy identity,
            IBlobService blobService,
            IUserSettingsCache userSettingsCache,
            MetadataProvider metadataProvider) : base(deps)
        {
            _behavior = behavior;
            _localizer = localizer;
            _adminRepo = adminRepo;
            _client = client;
            _identity = identity;
            _blobService = blobService;
            _userSettingsCache = userSettingsCache;
            _metadataProvider = metadataProvider;
        }

        public async Task<Versioned<UserSettingsForClient>> SaveUserSetting(SaveUserSettingsArguments args)
        {
            await Initialize();

            // Retrieve the arguments
            var key = args.Key;
            var value = args.Value;

            // Validation
            int maxKey = 255;
            int maxValue = 2048;

            if (string.IsNullOrWhiteSpace(key))
            {
                // Key is required
                throw new ServiceException(_localizer[ErrorMessages.Error_Field0IsRequired, nameof(args.Key)]);
            }
            else if (key.Length > maxKey)
            {
                // Key cannot be too big
                throw new ServiceException(_localizer[ErrorMessages.Error_Field0LengthMaximumOf1, nameof(args.Key), maxKey]);
            }

            if (value != null && value.Length > maxValue)
            {
                throw new ServiceException(_localizer[ErrorMessages.Error_Field0LengthMaximumOf1, nameof(args.Value), maxValue]);
            }

            // Save and return
            await _behavior.Repository.Users__SaveSettings(key, value, UserId);
            return await UserSettingsForClientImpl("fresh", cancellation: default);
        }

        public async Task<Versioned<UserSettingsForClient>> SaveUserPreferredLanguage(string preferredLanguage, CancellationToken cancellation)
        {
            await Initialize(cancellation);

            if (string.IsNullOrWhiteSpace(preferredLanguage))
            {
                throw new ServiceException(_localizer[ErrorMessages.Error_Field0IsRequired, "PreferredLanguage"]);
            }

            var settings = await _behavior.Settings(cancellation);
            if (settings.PrimaryLanguageId != preferredLanguage &&
                settings.SecondaryLanguageId != preferredLanguage &&
                settings.TernaryLanguageId != preferredLanguage)
            {
                // Not one of the languages supported by this company
                throw new ServiceException(_localizer["Error_Language0IsNotSupported", preferredLanguage]);
            }

            // Save and return
            await _behavior.Repository.Users__SavePreferredLanguage(preferredLanguage, UserId, cancellation);
            return await UserSettingsForClientImpl("fresh", cancellation);
        }

        public async Task<Versioned<UserSettingsForClient>> SaveUserPreferredCalendar(string preferredCalendar, CancellationToken cancellation)
        {
            await Initialize(cancellation);

            if (string.IsNullOrWhiteSpace(preferredCalendar))
            {
                throw new ServiceException(_localizer[ErrorMessages.Error_Field0IsRequired, "PreferredCalendar"]);
            }

            var settings = await _behavior.Settings(cancellation);
            if (settings.PrimaryCalendar != preferredCalendar &&
                settings.SecondaryCalendar != preferredCalendar)
            {
                // Not one of the Calendars supported by this company
                throw new ServiceException(_localizer["Error_Calendar0IsNotSupported", preferredCalendar]);
            }

            // Save and return
            await _behavior.Repository.Users__SavePreferredCalendar(preferredCalendar, UserId, cancellation);
            return await UserSettingsForClientImpl("fresh", cancellation);
        }

        public async Task<Versioned<UserSettingsForClient>> UserSettingsForClient(CancellationToken cancellation)
        {
            await Initialize(cancellation);
            return await UserSettingsForClientImpl(_behavior.UserSettingsVersion, cancellation);
        }

        private async Task<Versioned<UserSettingsForClient>> UserSettingsForClientImpl(string version, CancellationToken cancellation)
        {
            return await _userSettingsCache.GetUserSettings(UserId, _behavior.TenantId, version, cancellation);
        }

        public async Task<(List<User>, Extras)> SendInvitation(List<int> ids, ActionArguments args)
        {
            await Initialize();

            if (!_client.EmailEnabled)
            {
                throw new ServiceException("Email is not enabled in this installation.");
            }

            if (!_identity.CanInviteUsers)
            {
                throw new ServiceException("Identity server in this installation does not support user invitation.");
            }

            // Check if the user has permission
            var action = "SendInvitationEmail";
            var actionFilter = await UserPermissionsFilter(action, cancellation: default);
            ids = await CheckActionPermissionsBefore(actionFilter, ids);

            // Execute and return
            using var trx = TransactionFactory.ReadCommitted();
            var (result, dbUsers) = await _behavior.Repository.Users__Invite(
                    ids: ids,
                    validateOnly: ModelState.IsError,
                    top: ModelState.RemainingErrors,
                    userId: UserId);

            AddErrorsAndThrowIfInvalid(result.Errors);

            List<User> data = null;
            Extras extras = null;

            if (args.ReturnEntities ?? false)
            {
                (data, extras) = await GetByIds(ids, args, action, cancellation: default);
            }

            // Check user permissions again
            await CheckActionPermissionsAfter(actionFilter, ids, data);

            #region Non-Transactional Side-Effects

            // Send invitation emails
            var settings = await _behavior.Settings();
            var userSettings = await _behavior.UserSettings();

            IEnumerable<UserForInvitation> usersToInvite = dbUsers.Select(dbUser =>
            {
                var preferredCulture = GetCulture(dbUser.PreferredLanguage);
                using var _ = new CultureScope(preferredCulture);

                // Localize the names in the user's preferred language
                return new UserForInvitation
                {
                    Email = dbUser.Email,
                    Name = settings.Localize(dbUser.Name, dbUser.Name2, dbUser.Name3),
                    PreferredLanguage = dbUser.PreferredLanguage,
                    InviterName = settings.Localize(userSettings.Name, userSettings.Name2, userSettings.Name3),
                    CompanyName = settings.Localize(settings.ShortCompanyName, settings.ShortCompanyName2, settings.ShortCompanyName3),
                };
            });

            // Start a fresh transaction otherwise MSDTC error is raised.
            using var identityTrx = TransactionFactory.ReadCommitted(TransactionScopeOption.RequiresNew);

            await _identity.InviteUsersToTenant(_behavior.TenantId, usersToInvite);

            identityTrx.Complete();

            #endregion

            trx.Complete();
            return (data, extras);
        }

        public async Task<User> GetMyUser(CancellationToken cancellation)
        {
            await Initialize(cancellation);

            // Prepare the odata query
            var myIdSingleton = new List<int> { UserId };
            var me = await _behavior
                .Repository
                .Users
                .FilterByIds(myIdSingleton)
                .FirstOrDefaultAsync(QueryContext, cancellation);

            return me;
        }

        public async Task<User> SaveMyUser(MyUserForSave me)
        {
            await Initialize();

            var userIdSingleton = new List<int> { UserId };
            var user = await _behavior.Repository
                .Users
                .Expand("Roles")
                .FilterByIds(userIdSingleton).FirstOrDefaultAsync(QueryContext, cancellation: default);

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
                ContactEmail = user.ContactEmail,
                ContactMobile = user.ContactMobile,
                EmailNewInboxItem = user.EmailNewInboxItem,
                SmsNewInboxItem = user.SmsNewInboxItem,
                PushNewInboxItem = user.PushNewInboxItem,
                NormalizedContactMobile = user.NormalizedContactMobile,
                PreferredChannel = user.PreferredChannel,
                PreferredCalendar = user.PreferredCalendar,

                EntityMetadata = new EntityMetadata
                {
                    [nameof(UserForSave.Id)] = FieldMetadata.Loaded,
                    [nameof(UserForSave.Email)] = FieldMetadata.Loaded,
                    [nameof(UserForSave.Name)] = FieldMetadata.Loaded,
                    [nameof(UserForSave.Name2)] = FieldMetadata.Loaded,
                    [nameof(UserForSave.Name3)] = FieldMetadata.Loaded,
                    [nameof(UserForSave.PreferredLanguage)] = FieldMetadata.Loaded,
                    [nameof(UserForSave.Image)] = FieldMetadata.Loaded,
                    [nameof(UserForSave.ContactEmail)] = FieldMetadata.Loaded,
                    [nameof(UserForSave.ContactMobile)] = FieldMetadata.Loaded,
                    [nameof(UserForSave.EmailNewInboxItem)] = FieldMetadata.Loaded,
                    [nameof(UserForSave.SmsNewInboxItem)] = FieldMetadata.Loaded,
                    [nameof(UserForSave.PushNewInboxItem)] = FieldMetadata.Loaded,
                    [nameof(UserForSave.NormalizedContactMobile)] = FieldMetadata.Loaded,
                    [nameof(UserForSave.PreferredChannel)] = FieldMetadata.Loaded,
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

            // Structural Validation
            var meta = _metadataProvider.GetMetadata(_behavior.TenantId, typeof(UserForSave));
            ValidateEntity(userForSave, meta);
            ModelState.ThrowIfInvalid();

            var entities = new List<UserForSave>() { userForSave };

            // Start a transaction scope for save since it causes data modifications
            using var trx = TransactionFactory.ReadCommitted();

            // Preprocess the entities
            entities = await SavePreprocessAsync(entities);

            // Save and retrieve Ids
            await SaveExecuteAsync(entities, returnIds: false);

            // Load response
            var response = await GetMyUser(cancellation: default);

            // Perform side effects of save that are not transactional, just before committing the transaction
            await NonTransactionalSideEffectsForSave(entities, new List<User> { response });

            // Commit and return
            trx.Complete();
            return response;
        }

        public async Task<(string ImageId, byte[] ImageBytes)> GetImage(int id, CancellationToken cancellation)
        {
            await Initialize(cancellation);

            string imageId;
            if (id == UserId)
            {
                // A user can always view their own image, so we bypass read permissions
                User me = await _behavior
                    .Repository
                    .Users
                    .Filter("Id eq me")
                    .Select(nameof(User.ImageId))
                    .FirstOrDefaultAsync(QueryContext, cancellation);

                imageId = me.ImageId;
            }
            else
            {
                // This enforces read permissions
                var (user, _) = await GetById(id, new GetByIdArguments { Select = nameof(User.ImageId) }, cancellation);
                imageId = user.ImageId;
            }

            // Get the blob name
            if (imageId != null)
            {
                try
                {
                    // Get the bytes
                    string blobName = ImageBlobName(imageId);
                    var imageBytes = await _blobService.LoadBlob(_behavior.TenantId, blobName, cancellation);

                    return (imageId, imageBytes);
                }
                catch (BlobNotFoundException)
                {
                    throw new NotFoundException<int>(id);
                }
            }
            else
            {
                throw new NotFoundException<int>(id);
            }
        }

        public Task<(List<User>, Extras)> Activate(List<int> ids, ActionArguments args)
        {
            return SetIsActive(ids, args, isActive: true);
        }

        public Task<(List<User>, Extras)> Deactivate(List<int> ids, ActionArguments args)
        {
            return SetIsActive(ids, args, isActive: false);
        }

        private async Task<(List<User>, Extras)> SetIsActive(List<int> ids, ActionArguments args, bool isActive)
        {
            await Initialize();

            // Check user permissions
            var action = "IsActive";
            var actionFilter = await UserPermissionsFilter(action, cancellation: default);
            ids = await CheckActionPermissionsBefore(actionFilter, ids);

            // C# Validation
            foreach (var (id, index) in ids.Select((id, index) => (id, index)))
            {
                if (id == UserId)
                {
                    ModelState.AddError($"[{index}]", _localizer["Error_CannotDeactivateYourOwnUser"].Value);
                }
            }

            // Execute and return
            using var trx = TransactionFactory.ReadCommitted();
            OperationResult result = await _behavior.Repository.Users__Activate(
                    ids: ids,
                    isActive: isActive,
                    validateOnly: ModelState.IsError,
                    top: ModelState.RemainingErrors,
                    userId: UserId);

            AddErrorsAndThrowIfInvalid(result.Errors);

            List<User> data = null;
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

        protected override Task<EntityQuery<User>> Search(EntityQuery<User> query, GetArguments args, CancellationToken _)
        {
            string search = args.Search;
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Replace("'", "''"); // escape quotes by repeating them

                var email = nameof(User.Email);
                var name = nameof(User.Name);
                var name2 = nameof(User.Name2);
                var name3 = nameof(User.Name3);

                string filter = $"{name} contains '{search}' or {name2} contains '{search}' or {name3} contains '{search}' or {email} contains '{search}'";

                // If the search term looks like an email, include the contact email in the search
                if (emailAtt.IsValid(search))
                {
                    var contactEmail = nameof(User.ContactEmail);

                    filter += $" or {contactEmail} eq '{search}'";
                }

                // If the search term looks like a phone number, include the contact mobile in the search
                if (phoneAtt.IsValid(search))
                {
                    var e164 = BaseUtil.ToE164(search);
                    var normalizedContactMobile = nameof(User.NormalizedContactMobile);

                    filter += $" or {normalizedContactMobile} eq '{e164}'";
                }

                query = query.Filter(filter);
            }

            return Task.FromResult(query);
        }

        protected override Task<List<UserForSave>> SavePreprocessAsync(List<UserForSave> entities)
        {
            foreach (var entity in entities)
            {
                entity.Email = entity.Email.ToLower();

                entity.EmailNewInboxItem ??= false;
                entity.SmsNewInboxItem ??= false;
                entity.PushNewInboxItem ??= false;

                entity.PreferredChannel ??= "Email";

                if (string.IsNullOrWhiteSpace(entity.ContactEmail))
                {
                    entity.ContactEmail = null;
                }
                else
                {
                    entity.ContactEmail = entity.ContactEmail.ToLower();
                }

                if (string.IsNullOrWhiteSpace(entity.ContactMobile))
                {
                    entity.ContactMobile = null;
                }

                // Normalized the contact mobile
                entity.NormalizedContactMobile = BaseUtil.ToE164(entity.ContactMobile);

                // Make sure the role memberships are not referring to the wrong user
                // (RoleMembership is a semi-weak entity, used by both User and Role)
                entity.Roles?.ForEach(role =>
                {
                    role.UserId = entity.Id;
                });
            }

            return base.SavePreprocessAsync(entities);
        }

        protected override async Task<List<int>> SaveExecuteAsync(List<UserForSave> entities, bool returnIds)
        {
            #region Validate

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

            TypeMetadata meta = null;

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
                        ModelState.AddError($"[{index}].{nameof(entity.Roles)}[{lineIndex}].{nameof(entity.Id)}",
                            _localizer["Error_TheEntityWithId0IsSpecifiedMoreThanOnce", id]);
                    }

                    if (line.RoleId == null)
                    {
                        var index = indices[entity];
                        var lineIndex = lineIndices[line];

                        meta ??= await GetMetadataForSave(cancellation: default);
                        var roleProp = meta.CollectionProperty(nameof(UserForSave.Roles)).CollectionTargetTypeMetadata.Property(nameof(RoleMembershipForSave.RoleId)) ??
                            throw new InvalidOperationException($"Bug: Could not retrieve metadata for role Id property");

                        ModelState.AddError($"[{index}].{nameof(entity.Roles)}[{lineIndex}].{nameof(RoleMembershipForSave.RoleId)}",
                            _localizer[ErrorMessages.Error_Field0IsRequired, roleProp.Display()]);
                    }
                }
            }

            #endregion

            #region Save

            // Step (1): Extract the images
            _blobsToSave = BaseUtil.ExtractImages(entities, ImageBlobName).ToList();

            // Step (2): Save users in the application database
            var result = await _behavior.Repository.Users__Save(
                    entities: entities,
                    returnIds: returnIds,
                    validateOnly: ModelState.IsError,
                    top: ModelState.RemainingErrors,
                    userId: UserId);

            AddErrorsAndThrowIfInvalid(result.Errors);

            _blobsToDelete = result.DeletedImageIds.Select(ImageBlobName).ToList();

            // Return the new Ids
            return result.Ids;

            #endregion
        }

        protected override async Task NonTransactionalSideEffectsForSave(List<UserForSave> entities, List<User> data)
        {
            // Step (3): Delete old images from the blob storage
            if (_blobsToDelete.Any())
            {
                await _blobService.DeleteBlobsAsync(_behavior.TenantId, _blobsToDelete);
            }

            // Step (4): Add new images to the blob storage
            if (_blobsToSave.Any())
            {
                await _blobService.SaveBlobsAsync(_behavior.TenantId, _blobsToSave);
            }

            // Step (5): Create the identity users
            using var identityTrx = TransactionFactory.ReadCommitted(TransactionScopeOption.RequiresNew);
            if (_identity.CanCreateUsers)
            {
                var emails = entities.Select(e => e.Email);
                await _identity.CreateUsersIfNotExist(emails);
            }

            // Step (6) Update the directory users in the admin database
            using var adminTrx = TransactionFactory.ReadCommitted(TransactionScopeOption.RequiresNew);

            var oldEmails = new List<string>(); // Emails are readonly after the first save
            var newEmails = entities.Where(e => e.Id == 0).Select(e => e.Email);

            await _adminRepo.DirectoryUsers__Save(newEmails, oldEmails, _behavior.TenantId);

            identityTrx.Complete();
            adminTrx.Complete();
        }

        protected override async Task DeleteExecuteAsync(List<int> ids)
        {
            #region Validate

            // Make sure the user is not deleting his/her own account
            foreach (var (id, index) in ids.Select((id, index) => (id, index)))
            {
                if (id == UserId)
                {
                    ModelState.AddError($"[{index}]", _localizer["Error_CannotDeleteYourOwnUser"].Value);
                }
            }

            #endregion

            #region Delete

            IEnumerable<string> oldEmails;
            IEnumerable<string> newEmails = new List<string>();

            List<string> blobsToDelete;

            var (result, emails) = await _behavior.Repository.Users__Delete(
                ids: ids,
                validateOnly: ModelState.IsError,
                top: ModelState.RemainingErrors,
                userId: UserId);

            AddErrorsAndThrowIfInvalid(result.Errors);

            oldEmails = emails;
            blobsToDelete = result.DeletedImageIds.Select(ImageBlobName).ToList();

            #endregion

            #region Non-Transactional Effects

            // It's unfortunate that EF Core does not support distributed transactions, so there is no
            // guarantee that deletes to both the application and the admin will not complete one without the other

            using var adminTrx = TransactionFactory.ReadCommitted(TransactionScopeOption.RequiresNew);

            // Delete from directory
            await _adminRepo.DirectoryUsers__Save(newEmails, oldEmails, _behavior.TenantId);

            // Delete user images
            await _blobService.DeleteBlobsAsync(_behavior.TenantId, blobsToDelete);

            adminTrx.Complete();

            #endregion
        }

        private string ImageBlobName(string guid)
        {
            return $"Users/{guid}";
        }

        protected override MappingInfo ProcessDefaultMapping(MappingInfo mapping)
        {
            // Remove the UserId property from the template, it's supposed to be hidden
            var roleMemberships = mapping.CollectionPropertyByName(nameof(User.Roles));
            var userProp = roleMemberships.SimplePropertyByName(nameof(RoleMembership.UserId));

            roleMemberships.SimpleProperties = roleMemberships.SimpleProperties.Where(p => p != userProp);
            mapping.NormalizeIndices(); // Fix the gap we created in the previous line

            return base.ProcessDefaultMapping(mapping);
        }

        public async Task<string> TestEmail(string emailAddress)
        {
            await Initialize();

            // This sequence checks for all potential problems that could occur locally
            if (string.IsNullOrWhiteSpace(emailAddress))
            {
                var errorMsg = _localizer[ErrorMessages.Error_Field0IsRequired, _localizer["Entity_ContactEmail"]];
                throw new ServiceException(errorMsg);
            }

            if (!emailAtt.IsValid(emailAddress))
            {
                var errorMsg = _localizer[ErrorMessages.Error_Field0IsNotValidEmail, _localizer["Entity_ContactEmail"]];
                throw new ServiceException(errorMsg);
            }

            if (emailAddress.Length > EmailValidation.MaximumEmailAddressLength)
            {
                var errorMsg = _localizer[ErrorMessages.Error_Field0LengthMaximumOf1, _localizer["Entity_ContactEmail"], EmailValidation.MaximumEmailAddressLength];
                throw new ServiceException(errorMsg);
            }

            var subject = await _client.TestEmailAddress(_behavior.TenantId, emailAddress);

            string successMsg = _localizer["TestEmailSentTo0WithSubject1", emailAddress, subject];
            return successMsg;
        }

        public async Task<string> TestPhone(string phone)
        {
            await Initialize();

            if (!_client.SmsEnabled)
            {
                throw new ServiceException("Email is not enabled in this ERP installation.");
            }

            var settings = await _behavior.Settings();
            if (!settings.SmsEnabled)
            {
                throw new ServiceException("Email is not enabled for this company.");
            }

            // This sequence checks for all potential problems that could occur locally
            if (string.IsNullOrWhiteSpace(phone))
            {
                var errorMsg = _localizer[ErrorMessages.Error_Field0IsRequired, _localizer["Entity_ContactMobile"]];
                throw new ServiceException(errorMsg);
            }

            if (!phoneAtt.IsValid(phone))
            {
                var errorMsg = _localizer[ErrorMessages.Error_Field0IsNotValidPhone, _localizer["Entity_ContactMobile"]];
                throw new ServiceException(errorMsg);
            }

            var normalizedPhone = BaseUtil.ToE164(phone);
            if (normalizedPhone.Length > SmsValidation.MaximumPhoneNumberLength)
            {
                var errorMsg = _localizer[ErrorMessages.Error_Field0LengthMaximumOf1, _localizer["Entity_ContactMobile"], SmsValidation.MaximumPhoneNumberLength];
                throw new ServiceException(errorMsg);
            }

            var message = await _client.TestPhoneNumber(_behavior.TenantId, normalizedPhone);

            string successMsg = _localizer["TestSmsSentTo0WithMessage1", normalizedPhone, message];
            return successMsg;
        }
    }
}
