using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
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
            await _behavior.Repository.Users__SaveSettings(key, value);
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
            await _behavior.Repository.Users__SavePreferredLanguage(preferredLanguage, cancellation);
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
            await _behavior.Repository.Users__SavePreferredCalendar(preferredCalendar, cancellation);
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

        public async Task SendInvitationEmail(List<int> userIds)
        {
            await Initialize();

            // TODO
            throw new NotImplementedException();

            //if (!_client.EmailEnabled)
            //{
            //    // Developer mistake
            //    throw new ServiceException("Email is not enabled in this installation.");
            //}

            //if (!_identity.CanInviteUsers)
            //{
            //    // Developer mistake
            //    throw new ServiceException("Identity server in this installation does not support user invitation.");
            //}

            //// Check if the user has permission
            //var actionFilter = await UserPermissionsFilter("ResendInvitationEmail", cancellation: default);
            //userIds = await CheckActionPermissionsBefore(actionFilter, userIds);

            //if (!userIds.Any())
            //{
            //    // The user cannot see that Id or that Id is completely missing
            //    throw new NotFoundException<int>(userIds);
            //}

            //// Load the user
            //var user = await _behavior.Repository.Users.FilterByIds(userIds).FirstOrDefaultAsync(QueryContext, cancellation: default);
            //if (user == null)
            //{
            //    throw new NotFoundException<int>(userIds);
            //}

            //if (!string.IsNullOrWhiteSpace(user.ExternalId))
            //{
            //    throw new ServiceException(_localizer["Error_User0HasAlreadyAcceptedTheInvitation", user.Email]);
            //}

            //string toEmail = user.Email;
            //var idUser = await _userManager.FindByEmailAsync(toEmail);
            //if (idUser == null)
            //{
            //    throw new NotFoundException<string>(toEmail);
            //}

            //if (idUser.EmailConfirmed)
            //{
            //    throw new ServiceException(_localizer["Error_User0HasAlreadyAcceptedTheInvitation", user.Email]);
            //}

            //var (subject, body) = await MakeInvitationEmailAsync(idUser, user.Name, user.Name2, user.Name3, user.PreferredLanguage);
            //await _emailSender.SendAsync(new Email(toEmail)
            //{
            //    Subject = subject,
            //    Body = body,
            //});
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

            // Basic Validation
            var meta = _metadataProvider.GetMetadata(_behavior.TenantId, typeof(UserForSave));
            ValidateEntity(userForSave, meta);
            ModelState.ThrowIfInvalid();

            var entities = new List<UserForSave>() { userForSave };

            // Start a transaction scope for save since it causes data modifications
            using var trx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

            // Preprocess the entities
            entities = await SavePreprocessAsync(entities);

            // Save and retrieve Ids
            await SaveExecuteAsync(entities, returnIds: false);

            // Handle Errors
            ModelState.ThrowIfInvalid();

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
                // Get the bytes
                string blobName = ImageBlobName(imageId);
                var imageBytes = await _blobService.LoadBlob(_behavior.TenantId, blobName, cancellation);

                return (imageId, imageBytes);
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
                    ModelState.AddModelError($"[{index}]", _localizer["Error_CannotDeactivateYourOwnUser"].Value);

                    if (ModelState.HasReachedMaxErrors)
                    {
                        break;
                    }
                }
            }

            ModelState.ThrowIfInvalid();

            // Execute and return
            using var trx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            OperationResult result = await _behavior.Repository.Users__Activate(ids, isActive, userId: UserId);
            AddLocalizedErrors(result.Errors);
            ModelState.ThrowIfInvalid();

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

        protected override EntityQuery<User> Search(EntityQuery<User> query, GetArguments args)
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
                    var e164 = BaseUtilities.ToE164(search);
                    var normalizedContactMobile = nameof(User.NormalizedContactMobile);

                    filter += $" or {normalizedContactMobile} eq '{e164}'";
                }

                query = query.Filter(filter);
            }

            return query;
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
                entity.NormalizedContactMobile = BaseUtilities.ToE164(entity.ContactMobile);

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
                        ModelState.AddModelError($"[{index}].{nameof(entity.Roles)}[{lineIndex}].{nameof(entity.Id)}",
                            _localizer["Error_TheEntityWithId0IsSpecifiedMoreThanOnce", id]);
                    }

                    if (line.RoleId == null)
                    {
                        var index = indices[entity];
                        var lineIndex = lineIndices[line];

                        meta ??= await GetMetadataForSave(cancellation: default);
                        var roleProp = meta.CollectionProperty(nameof(UserForSave.Roles)).CollectionTargetTypeMetadata.Property(nameof(RoleMembershipForSave.RoleId)) ??
                            throw new InvalidOperationException($"Bug: Could not retrieve metadata for role Id property");

                        ModelState.AddModelError($"[{index}].{nameof(entity.Roles)}[{lineIndex}].{nameof(RoleMembershipForSave.RoleId)}",
                            _localizer[ErrorMessages.Error_Field0IsRequired, roleProp.Display()]);
                    }
                }
            }

            // No need to invoke SQL if the model state is full of errors
            if (!ModelState.IsValid)
            {
                return null;
            }

            #endregion

            #region Save

            // Step (1): Extract the images
            _blobsToSave = BaseUtilities.ExtractImages(entities, ImageBlobName).ToList();

            // Step (2): Save users in the application database
            var result = await _behavior.Repository.Users__Save(entities, returnIds: returnIds, userId: UserId);
            AddLocalizedErrors(result.Errors);
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
            using var identityTrx = new TransactionScope(TransactionScopeOption.RequiresNew, TransactionScopeAsyncFlowOption.Enabled);
            if (_identity.CanCreateUsers)
            {
                var emails = entities.Select(e => e.Email);
                await _identity.CreateUsersIfNotExist(emails);
            }

            // Step (6) Update the directory users in the admin database
            using var adminTrx = new TransactionScope(TransactionScopeOption.RequiresNew, TransactionScopeAsyncFlowOption.Enabled);

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
                    ModelState.AddModelError($"[{index}]", _localizer["Error_CannotDeleteYourOwnUser"].Value);

                    if (ModelState.HasReachedMaxErrors)
                    {
                        break;
                    }
                }
            }

            if (!ModelState.IsValid)
            {
                return;
            }

            #endregion

            #region Delete

            IEnumerable<string> oldEmails;
            IEnumerable<string> newEmails = new List<string>();

            try
            {
                var (result, emails) = await _behavior.Repository.Users__Delete(ids, userId: UserId);
                AddLocalizedErrors(result.Errors);
                oldEmails = emails;
            }
            catch (ForeignKeyViolationException)
            {
                var meta = await GetMetadata(cancellation: default);
                throw new ServiceException(_localizer["Error_CannotDelete0AlreadyInUse", meta.SingularDisplay()]);
            }

            #endregion

            #region Non-Transactional Effects

            // It's unfortunate that EF Core does not support distributed transactions, so there is no
            // guarantee that deletes to both the application and the admin will not complete one without the other

            using var adminTrx = new TransactionScope(TransactionScopeOption.RequiresNew, TransactionScopeAsyncFlowOption.Enabled);

            await _adminRepo.DirectoryUsers__Save(newEmails, oldEmails, _behavior.TenantId);

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

            var normalizedPhone = BaseUtilities.ToE164(phone);
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

    // Client Proxy

    public interface IClientProxy
    {
        /// <summary>
        /// True if Email is enabled in this installation, false otherwise.
        /// </summary>
        public bool EmailEnabled { get; }

        /// <summary>
        /// True if SMS is enabled in this installation, false otherwise.
        /// </summary>
        public bool SmsEnabled { get; }

        /// <summary>
        /// Sends a test email to the given email address.
        /// </summary>
        /// <param name="tenantId">The Id of the tenant performing the test.</param>
        /// <param name="emailAddress">The email address to test.</param>
        /// <returns>The subject of the email.</returns>
        public Task<string> TestEmailAddress(int tenantId, string emailAddress);

        /// <summary>
        /// Sends a test SMS message to the given phone number.
        /// </summary>
        /// <param name="tenantId">The Id of the tenant performing the test.</param>
        /// <param name="phoneNumber">The phone number to test.</param>
        /// <returns>The body of the test SMS.</returns>
        public Task<string> TestPhoneNumber(int tenantId, string phoneNumber);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="infos"></param>
        /// <returns></returns>
        public Task InviteConfirmedUsersToTenant(int tenantId, IEnumerable<ConfirmedEmailInvitation> infos);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="infos"></param>
        /// <param name="identityUrl"></param>
        /// <returns></returns>
        public Task InviteUnconfirmedUsersToTenant(int tenantId, IEnumerable<UnconfirmedEmailInvitation> infos);
    }

    public class ConfirmedEmailInvitation
    {
        public string Email { get; set; }
        public string Name { get; set; }
        public string InviterName { get; set; }
        public string PreferredLanguage { get; set; }
    }

    public class UnconfirmedEmailInvitation : ConfirmedEmailInvitation
    {
        public string EmailConfirmationLink { get; set; }
    }


    // Identity

    public interface IIdentityProxy
    {
        public bool CanCreateUsers { get; }
        public Task CreateUsersIfNotExist(IEnumerable<string> emails);
        public bool CanInviteUsers { get; }
        public Task InviteUsersToTenant(int tenantId, IEnumerable<UserForInvitation> users);
    }

    public class UserForInvitation
    {
        /// <summary>
        /// The email of the invited user.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// The name of the user in their preferred language.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The preferred language of the invited user.
        /// </summary>
        public string PreferredLanguage { get; set; }

        /// <summary>
        /// The name of the inviter in the user's preferred language.
        /// </summary>
        public string InviterName { get; set; }
    }

    public class NullIdentityProxy : IIdentityProxy
    {
        public bool CanCreateUsers => false;

        public bool CanInviteUsers => false;

        public Task CreateUsersIfNotExist(IEnumerable<string> emails)
        {
            throw new InvalidOperationException("Attempt to create users through an identity proxy that does not support user creation.");
        }

        public Task InviteUsersToTenant(int tenantId, IEnumerable<UserForInvitation> users)
        {
            throw new InvalidOperationException("Attempt to invite users through an identity proxy that does not support user invitation.");
        }
    }
}
