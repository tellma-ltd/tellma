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
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using System.Threading;
using Microsoft.AspNetCore.Routing;
using Tellma.Controllers.ImportExport;
using System.Text;
using System.ComponentModel.DataAnnotations;
using Tellma.Controllers.Utiltites;
using Tellma.Controllers.Jobs;
using Tellma.Services.Sms;

namespace Tellma.Controllers
{
    [Route("api/" + BASE_ADDRESS)]
    [AuthorizeJwtBearer]
    [ApplicationController(allowUnobtrusive: true)]
    public class UsersController : CrudControllerBase<UserForSave, User, int>
    {
        public const string BASE_ADDRESS = "users";

        private readonly UsersService _service;

        public UsersController(UsersService service, IServiceProvider sp) : base(sp)
        {
            _service = service;
        }

        [HttpGet("client")]
        public async Task<ActionResult<Versioned<UserSettingsForClient>>> UserSettingsForClient(CancellationToken cancellation)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                var result = await _service.UserSettingsForClient(cancellation);
                return Ok(result);
            },
            _logger);
        }

        [HttpPost("client")]
        public async Task<ActionResult<Versioned<UserSettingsForClient>>> SaveUserSetting(SaveUserSettingsArguments args)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                var result = await _service.SaveUserSetting(args);
                return Ok(result);
            },
            _logger);
        }

        [HttpPut("invite")]
        public async Task<ActionResult> ResendInvitationEmail(int id)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                await _service.ResendInvitationEmail(id);
                return Ok();
            },
            _logger);
        }

        [HttpGet("{id}/image")]
        public async Task<ActionResult> GetImage(int id, CancellationToken cancellation)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                var (imageId, imageBytes) = await _service.GetImage(id, cancellation);
                Response.Headers.Add("x-image-id", imageId);
                return File(imageBytes, "image/jpeg");
            },
            _logger);
        }

        [HttpGet("me")]
        public async Task<ActionResult<GetByIdResponse<User>>> GetMyUser(CancellationToken cancellation)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                User user = await _service.GetMyUser(cancellation);
                GetByIdResponse<User> response = TransformToResponse(user, cancellation);
                return Ok(response);
            },
            _logger);
        }

        [HttpPost("me")]
        public async Task<ActionResult<GetByIdResponse<User>>> SaveMyUser([FromBody] MyUserForSave me)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                User user = await _service.SaveMyUser(me);
                GetByIdResponse<User> result = TransformToResponse(user, cancellation: default);
                Response.Headers.Set("x-user-settings-version", Constants.Stale);
                return Ok(result);

            }, _logger);
        }

        [HttpPut("activate")]
        public async Task<ActionResult<EntitiesResponse<User>>> Activate([FromBody] List<int> ids, [FromQuery] ActivateArguments args)
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
        public async Task<ActionResult<EntitiesResponse<User>>> Deactivate([FromBody] List<int> ids, [FromQuery] DeactivateArguments args)
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


        [HttpPut("test-email")]
        public async Task<ActionResult<string>> TestEmail([FromQuery] string email)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                string result = await _service.TestEmail(email);
                return Ok(new
                {
                    Message = result
                });
            },
            _logger);
        }

        [HttpPut("test-phone")]
        public async Task<ActionResult<string>> TestPhone([FromQuery] string phone)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                string result = await _service.TestPhone(phone);
                return Ok(new
                {
                    Message = result
                });
            },
            _logger);
        }

        private GetByIdResponse<User> TransformToResponse(User me, CancellationToken cancellation)
        {
            // Apply the permission masks (setting restricted fields to null) and adjust the metadata accordingly
            var relatedEntities = FlattenAndTrim(new List<User> { me }, cancellation);

            // Return
            return new GetByIdResponse<User>
            {
                Result = me,
                CollectionName = GetCollectionName(typeof(User)),
                RelatedEntities = relatedEntities
            };
        }

        protected override CrudServiceBase<UserForSave, User, int> GetCrudService()
        {
            return _service.SetUrlHelper(Url).SetScheme(Request.Scheme);
        }
    }

    public class UsersService : CrudServiceBase<UserForSave, User, int>
    {
        private static readonly PhoneAttribute phoneAtt = new PhoneAttribute();
        private static readonly EmailAddressAttribute emailAtt = new EmailAddressAttribute();
        private static readonly Random rand = new Random();

        private readonly ApplicationRepository _appRepo;
        private readonly AdminRepository _adminRepo;
        private readonly ITenantIdAccessor _tenantIdAccessor;
        private readonly IBlobService _blobService;
        private readonly MetadataProvider _metadataProvider;
        private readonly ExternalNotificationsService _notifications;
        private readonly IEmailSender _emailSender;
        private readonly EmailTemplatesProvider _emailTemplates;
        private readonly GlobalOptions _options;
        private readonly UserManager<EmbeddedIdentityServerUser> _userManager;

        // These are created and used across multiple methods
        private TransactionScope _adminTrxScope;
        private TransactionScope _identityTrxScope;
        private List<(EmbeddedIdentityServerUser IdUser, UserForSave User)> _usersToInvite;
        private List<string> _blobsToDelete;
        private List<(string, byte[])> _blobsToSave;

        private IUrlHelper _urlHelper = null;
        private string _scheme = null;

        private int TenantId => _tenantIdAccessor.GetTenantId(); // Syntactic sugar

        public UsersService SetUrlHelper(IUrlHelper urlHelper)
        {
            _urlHelper = urlHelper;
            return this;
        }

        public UsersService SetScheme(string scheme)
        {
            _scheme = scheme;
            return this;
        }

        private string View => UsersController.BASE_ADDRESS;

        public UsersService(
            ApplicationRepository appRepo,
            AdminRepository adminRepo,
            IOptions<GlobalOptions> options,
            IServiceProvider serviceProvider,
            IEmailSender emailSender,
            EmailTemplatesProvider emailTemplates,
            ITenantIdAccessor tenantIdAccessor,
            IBlobService blobService,
            MetadataProvider metadataProvider,
            ExternalNotificationsService notifications) : base(serviceProvider)
        {
            _appRepo = appRepo;
            _adminRepo = adminRepo;
            _tenantIdAccessor = tenantIdAccessor;
            _blobService = blobService;
            _metadataProvider = metadataProvider;
            _notifications = notifications;
            _emailSender = emailSender;
            _emailTemplates = emailTemplates;
            _options = options.Value;

            // we use this trick since this is an optional dependency, it will resolve to null if 
            // the embedded identity server is not enabled
            _userManager = (UserManager<EmbeddedIdentityServerUser>)serviceProvider.GetService(typeof(UserManager<EmbeddedIdentityServerUser>));
        }

        public async Task<Versioned<UserSettingsForClient>> SaveUserSetting(SaveUserSettingsArguments args)
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
                throw new BadRequestException(_localizer[Constants.Error_Field0IsRequired, nameof(args.Key)]);
            }
            else if (key.Length > maxKey)
            {
                // 
                throw new BadRequestException(_localizer[Constants.Error_Field0LengthMaximumOf1, nameof(args.Key), maxKey]);
            }

            if (value != null && value.Length > maxValue)
            {
                throw new BadRequestException(_localizer[Constants.Error_Field0LengthMaximumOf1, nameof(args.Value), maxValue]);
            }

            // Save and return
            await _appRepo.Users__SaveSettings(key, value);
            return await UserSettingsForClient(cancellation: default);
        }

        public async Task<Versioned<UserSettingsForClient>> UserSettingsForClient(CancellationToken cancellation)
        {
            var (version, user, customSettings) = await _appRepo.UserSettings__Load(cancellation);

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

            var result = new Versioned<UserSettingsForClient>
            (
                version: version.ToString(),
                data: userSettingsForClient
            );

            return result;
        }

        public async Task ResendInvitationEmail(int userId)
        {
            if (!_options.EmailEnabled)
            {
                // Developer mistake
                throw new BadRequestException("Email is not enabled in this installation");
            }

            if (!_options.EmbeddedIdentityServerEnabled)
            {
                // Developer mistake
                throw new BadRequestException("Embedded identity is not enabled in this installation");
            }

            // Check if the user has permission
            var actionFilter = await UserPermissionsFilter("ResendInvitationEmail", cancellation: default);
            var idSingleton = new List<int> { userId };
            idSingleton = await CheckActionPermissionsBefore(actionFilter, idSingleton);

            if (!idSingleton.Any())
            {
                // The user cannot see those Ids or they are completely missing
                throw new NotFoundException<int>(userId);
            }

            // Load the user
            var user = await _appRepo.Users.FilterByIds(idSingleton).FirstOrDefaultAsync(cancellation: default);
            if (user == null)
            {
                throw new NotFoundException<int>(userId);
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

            var (subject, body) = await MakeInvitationEmailAsync(idUser, user.Name, user.Name2, user.Name3, user.PreferredLanguage);
            await _emailSender.SendAsync(new Email(toEmail)
            {
                Subject = subject,
                Body = body,
            });
        }

        public async Task<User> GetMyUser(CancellationToken cancellation)
        {
            int myId = _appRepo.GetUserInfo().UserId.Value;

            // Prepare the odata query
            var myIdSingleton = new List<int> { myId };
            var me = await _appRepo.Users.FilterByIds(myIdSingleton).FirstOrDefaultAsync(cancellation);

            return me;
        }

        public async Task<User> SaveMyUser([FromBody] MyUserForSave me)
        {
            // Basic validation
            var meta = _metadataProvider.GetMetadata(_tenantIdAccessor.GetTenantId(), typeof(MyUserForSave));
            ValidateEntity(me, meta);
            ModelState.ThrowIfInvalid();

            int myId = _appRepo.GetUserInfo().UserId.Value;
            var myIdSingleton = new List<int> { myId };
            var user = await _appRepo.Users.Expand("Roles").FilterByIds(myIdSingleton).FirstOrDefaultAsync(cancellation: default);

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
            string imageId;
            if (id == _appRepo.GetUserInfo().UserId)
            {
                // A user can always view their own image, so we bypass read permissions
                User me = await _appRepo.Users.Filter("Id eq me").Select(nameof(User.ImageId)).FirstOrDefaultAsync(cancellation);
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
                string blobName = BlobName(imageId);
                var imageBytes = await _blobService.LoadBlob(blobName, cancellation);

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
            // Check user permissions
            var action = "IsActive";
            var actionFilter = await UserPermissionsFilter(action, cancellation: default);
            ids = await CheckActionPermissionsBefore(actionFilter, ids);

            // C# Validation
            var userInfo = await _appRepo.GetUserInfoAsync(cancellation: default);
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

            if (!ModelState.IsValid)
            {
                throw new UnprocessableEntityException(ModelState);
            }

            // Execute and return
            using var trx = ControllerUtilities.CreateTransaction();
            await _appRepo.Users__Activate(ids, isActive);

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

        protected override IRepository GetRepository()
        {
            return _appRepo;
        }

        protected override Query<User> Search(Query<User> query, GetArguments args)
        {
            string search = args.Search;
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Replace("'", "''"); // escape quotes by repeating them

                var email = nameof(User.Email);
                var name = nameof(User.Name);
                var name2 = nameof(User.Name2);
                var name3 = nameof(User.Name3);
                var cs = Ops.contains;

                string filter = $"{name} {cs} '{search}' or {name2} {cs} '{search}' or {name3} {cs} '{search}' or {email} {cs} '{search}'";

                // If the search term looks like an email, include the contact email in the search
                if (emailAtt.IsValid(search))
                {
                    var contactEmail = nameof(User.ContactEmail);
                    var eq = Ops.eq;

                    filter += $" or {contactEmail} {eq} '{search}'";
                }

                // If the search term looks like a phone number, include the contact mobile in the search
                if (phoneAtt.IsValid(search))
                {
                    var e164 = ControllerUtilities.ToE164(search);
                    var normalizedContactMobile = nameof(User.NormalizedContactMobile);
                    var eq = Ops.eq;

                    filter += $" or {normalizedContactMobile} {eq} '{e164}'";
                }

                query = query.Filter(filter);
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

                        meta ??= GetMetadataForSave();
                        var roleProp = meta.CollectionProperty(nameof(UserForSave.Roles)).CollectionTargetTypeMetadata.Property(nameof(RoleMembershipForSave.RoleId)) ??
                            throw new InvalidOperationException($"Bug: Could not retrieve metadata for role Id property");

                        ModelState.AddModelError($"[{index}].{nameof(entity.Roles)}[{lineIndex}].{nameof(RoleMembershipForSave.RoleId)}",
                            _localizer[Constants.Error_Field0IsRequired, roleProp.Display()]);
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
                entity.NormalizedContactMobile = ControllerUtilities.ToE164(entity.ContactMobile);
            }

            // Make all the emails small case
            return Task.FromResult(entities);
        }

        protected override async Task<List<int>> SaveExecuteAsync(List<UserForSave> entities, bool returnIds)
        {
            // NOTE: this method is not optimized for massive bulk (e.g. 1,000+ users), since it relies
            // on querying identity through UserManager one email at a time but it should be acceptable
            // with the usual workloads, companies with more than 200 users are rare anyways

            // Step (1) enlist the app repo
            _appRepo.EnlistTransaction(Transaction.Current); // So that it is not affected by identity or admin trx scope later

            // Step (2): If Embedded Identity Server is enabled, create any emails that don't already exist there
            _usersToInvite = new List<(EmbeddedIdentityServerUser IdUser, UserForSave User)>();
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
                    if (!identityUser.EmailConfirmed && entity.Id == 0)
                    {
                        _usersToInvite.Add((identityUser, entity));
                    }
                }
            }

            // Step (3): Extract the images
            var (blobsToDelete, blobsToSave, imageIds) = await ImageUtilities.ExtractImages<User, UserForSave>(_appRepo, entities, BlobName);

            _blobsToDelete = blobsToDelete;
            _blobsToSave = blobsToSave;

            // Step (4): Save the users in the app database
            var ids = await _appRepo.Users__Save(entities, imageIds, returnIds);

            // TODO: Check if the user lost his/her admin permissions

            // Return the new Ids
            return ids;
        }

        protected override async Task NonTransactionalSideEffectsForSave(List<UserForSave> entities, List<User> data)
        {
            // Step (5): Delete old images from the blob storage
            if (_blobsToDelete.Any())
            {
                await _blobService.DeleteBlobsAsync(_blobsToDelete);
            }

            // Step (6): Save new images to the blob storage
            if (_blobsToSave.Any())
            {
                await _blobService.SaveBlobsAsync(_blobsToSave);
            }

            // Step (7) Save the emails in the admin database
            var tenantId = _tenantIdAccessor.GetTenantId();
            _adminTrxScope = ControllerUtilities.CreateTransaction(TransactionScopeOption.RequiresNew);
            _adminRepo.EnlistTransaction(Transaction.Current);
            var oldEmails = new List<string>(); // Emails are readonly after the first save
            var newEmails = entities.Where(e => e.Id == 0).Select(e => e.Email);
            await _adminRepo.DirectoryUsers__Save(newEmails, oldEmails, tenantId);

            // Step (8): Send the invitation emails
            if (_usersToInvite.Any()) // This will be empty if embedded identity is disabled or if email is disabled
            {
                var emails = new List<Email>(_usersToInvite.Count);

                foreach (var (idUser, user) in _usersToInvite)
                {
                    // Add the email sender parameters
                    var (subject, body) = await MakeInvitationEmailAsync(idUser, user.Name, user.Name2, user.Name3, user.PreferredLanguage);

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
            var userInfo = await _appRepo.GetUserInfoAsync(cancellation: default);
            var userId = userInfo.UserId.Value;
            foreach (var (id, index) in ids.Select((id, index) => (id, index)))
            {
                if (id == userId)
                {
                    ModelState.AddModelError($"[{index}]", _localizer["Error_CannotDeleteYourOwnUser"].Value);

                    if (ModelState.HasReachedMaxErrors)
                    {
                        break;
                    }
                }
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

                await _adminRepo.DirectoryUsers__Save(newEmails, oldEmails, tenantId);

                appTrx.Complete();
                adminTrx.Complete();
            }
            catch (ForeignKeyViolationException)
            {
                throw new BadRequestException(_localizer["Error_CannotDelete0AlreadyInUse", _localizer["User"]]);
            }
        }

        protected override Task<IEnumerable<AbstractPermission>> UserPermissions(string action, CancellationToken cancellation)
        {
            return _appRepo.PermissionsFromCache(View, action, cancellation);
        }

        private string BlobName(string guid)
        {
            int tenantId = _tenantIdAccessor.GetTenantId();
            return $"{tenantId}/Users/{guid}";
        }

        private async Task<(string Subject, string Body)> MakeInvitationEmailAsync(EmbeddedIdentityServerUser identityRecipient, string name, string name2, string name3, string preferredLang)
        {
            // Load the info
            var info = await _appRepo.GetTenantInfoAsync(cancellation: default);

            // Use the recipient's preferred Language
            CultureInfo culture = string.IsNullOrWhiteSpace(preferredLang) ?
                CultureInfo.CurrentUICulture : new CultureInfo(preferredLang);
            using var _ = new CultureScope(culture);

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

            if (_urlHelper == null || _scheme == null)
            {
                throw new InvalidOperationException("Bug: The UrlHelper and/or the request scheme were not set");
            }

            string callbackUrl = _urlHelper.Page(
                    "/Account/ConfirmEmail",
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

        protected override MappingInfo ProcessDefaultMapping(MappingInfo mapping)
        {
            // Remove the UserId property from the template, it's supposed to be hidden
            var roleMemberships = mapping.CollectionProperty(nameof(User.Roles));
            var userProp = roleMemberships.SimpleProperty(nameof(RoleMembership.UserId));

            roleMemberships.SimpleProperties = roleMemberships.SimpleProperties.Where(p => p != userProp);
            mapping.NormalizeIndices(); // Fix the gap we created in the previous line

            return base.ProcessDefaultMapping(mapping);
        }

        public async Task<string> TestEmail(string emailAddress)
        {
            // This sequence checks for all potential problems that could occur locally
            if (string.IsNullOrWhiteSpace(emailAddress))
            {
                var errorMsg = _localizer[Constants.Error_Field0IsRequired, _localizer["User_ContactEmail"]];
                throw new BadRequestException(errorMsg);
            }

            if (!emailAtt.IsValid(emailAddress))
            {
                var errorMsg = _localizer[Constants.Error_Field0IsNotValidEmail, _localizer["User_ContactEmail"]];
                throw new BadRequestException(errorMsg);
            }

            if (emailAddress.Length > EmailValidation.MaximumEmailAddressLength)
            {
                var errorMsg = _localizer[Constants.Error_Field0LengthMaximumOf1, _localizer["User_ContactEmail"], EmailValidation.MaximumEmailAddressLength];
                throw new BadRequestException(errorMsg);
            }

            var email = new Email(emailAddress)
            {
                Subject = $"{ _localizer["Test"]} {rand.Next()}"
            };

            var error = EmailValidation.Validate(email);
            if (!string.IsNullOrWhiteSpace(error))
            {
                throw new BadRequestException(error);
            }

            try
            {
                await _notifications.Enqueue(TenantId, emails: new List<Email> { email });
            }
            catch (Exception ex)
            {
                throw new BadRequestException(ex.Message);
            }

            string successMsg = _localizer["TestEmailSentTo0WithSubject1", emailAddress, email.Subject];
            return successMsg;
        }

        public async Task<string> TestPhone(string phone)
        {
            // This sequence checks for all potential problems that could occur locally
            if (string.IsNullOrWhiteSpace(phone))
            {
                var errorMsg = _localizer[Constants.Error_Field0IsRequired, _localizer["User_ContactMobile"]];
                throw new BadRequestException(errorMsg);
            }

            if (!phoneAtt.IsValid(phone))
            {
                var errorMsg = _localizer[Constants.Error_Field0IsNotValidPhone, _localizer["User_ContactMobile"]];
                throw new BadRequestException(errorMsg);
            }

            var normalizedPhone = ControllerUtilities.ToE164(phone);
            if (normalizedPhone.Length > SmsValidation.MaximumPhoneNumberLength)
            {
                var errorMsg = _localizer[Constants.Error_Field0LengthMaximumOf1, _localizer["User_ContactMobile"], SmsValidation.MaximumPhoneNumberLength];
                throw new BadRequestException(errorMsg);
            }

            var msg = $"{ _localizer["Test"]} {rand.Next(10000)}, {_localizer["AppFullName"]}";
            var sms = new SmsMessage(normalizedPhone, msg);

            var error = SmsValidation.Validate(sms);
            if (!string.IsNullOrWhiteSpace(error))
            {
                throw new BadRequestException(error);
            }

            try
            {
                await _notifications.Enqueue(TenantId, smsMessages: new List<SmsMessage> { sms });
            }
            catch (Exception ex)
            {
                throw new BadRequestException(ex.Message);
            }

            string successMsg = _localizer["TestSmsSentTo0WithMessage1", normalizedPhone, sms.Message];
            return successMsg;
        }
    }
}
