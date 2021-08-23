using Microsoft.Extensions.Localization;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Tellma.Api.Base;
using Tellma.Api.Behaviors;
using Tellma.Api.Dto;
using Tellma.Api.Metadata;
using Tellma.Model.Admin;
using Tellma.Model.Common;
using Tellma.Repository.Admin;
using Tellma.Repository.Common;

namespace Tellma.Api
{
    public class AdminUsersService : CrudServiceBase<AdminUserForSave, AdminUser, int>
    {
        private readonly AdminFactServiceBehavior _behavior;
        private readonly AdminRepository _repo;
        private readonly IStringLocalizer<Strings> _localizer;
        private readonly IIdentityProxy _identity;
        private readonly IClientProxy _client;
        private readonly MetadataProvider _metadataProvider;

        protected override string View => "admin-users";

        protected override IFactServiceBehavior FactBehavior => _behavior;

        public AdminUsersService(
            AdminFactServiceBehavior behavior,
            CrudServiceDependencies deps,
            AdminRepository repo,
            IIdentityProxy identity,
            IClientProxy client) : base(deps)
        {
            _behavior = behavior;
            _repo = repo;
            _localizer = deps.Localizer;
            _identity = identity;
            _client = client;
            _metadataProvider = deps.Metadata;
        }

        public async Task<Versioned<AdminUserSettingsForClient>> SaveUserSetting(SaveUserSettingsArguments args)
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
                // 
                throw new ServiceException(_localizer[ErrorMessages.Error_Field0LengthMaximumOf1, nameof(args.Key), maxKey]);
            }

            if (value != null && value.Length > maxValue)
            {
                throw new ServiceException(_localizer[ErrorMessages.Error_Field0LengthMaximumOf1, nameof(args.Value), maxValue]);
            }

            // Save and return
            await _repo.AdminUsers__SaveSettings(key, value);
            return await UserSettingsForClient(cancellation: default);
        }

        public async Task<Versioned<AdminUserSettingsForClient>> UserSettingsForClient(CancellationToken cancellation)
        {
            await Initialize(cancellation);

            var (version, user, customSettings) = await _repo.UserSettings__Load(UserId, cancellation);

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
            await Initialize(cancellation);

            // Prepare the query
            var myIdSingleton = new List<int> { UserId };
            var me = await _repo.AdminUsers.FilterByIds(myIdSingleton).FirstOrDefaultAsync(QueryContext, cancellation);

            return me;
        }

        public async Task<AdminUser> SaveMyUser(MyAdminUserForSave me)
        {
            await Initialize();

            var userIdSingleton = new List<int> { UserId };
            var user = await _repo.AdminUsers
                .Expand("Permissions")
                .FilterByIds(userIdSingleton)
                .FirstOrDefaultAsync(QueryContext, cancellation: default);

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

            // Structural validation
            var meta = _metadataProvider.GetMetadata(null, typeof(AdminUserForSave));
            ValidateEntity(userForSave, meta);
            ModelState.ThrowIfInvalid();

            var entities = new List<AdminUserForSave>() { userForSave };

            // Start a transaction scope for save since it causes data modifications
            using var trx = TransactionFactory.ReadCommitted();

            // Preprocess the entities
            entities = await SavePreprocessAsync(entities);

            // Save and retrieve Ids
            await SaveExecuteAsync(entities, returnIds: false);

            // Load response
            var response = await GetMyUser(cancellation: default);

            // Perform side effects of save that are not transactional, just before committing the transaction
            await NonTransactionalSideEffectsForSave(entities, new List<AdminUser> { response });

            // Commit and return
            trx.Complete();
            return response;
        }

        public Task<EntitiesResult<AdminUser>> Activate(List<int> ids, ActionArguments args)
        {
            return SetIsActive(ids, args, isActive: true);
        }

        public Task<EntitiesResult<AdminUser>> Deactivate(List<int> ids, ActionArguments args)
        {
            return SetIsActive(ids, args, isActive: false);
        }

        public async Task<EntitiesResult<AdminUser>> SendInvitation(List<int> ids, ActionArguments args)
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
            var (output, dbUsers) = await _behavior.Repository.AdminUsers__Invite(
                    ids: ids,
                    validateOnly: ModelState.IsError,
                    top: ModelState.RemainingErrors,
                    userId: UserId);

            AddErrorsAndThrowIfInvalid(output.Errors);

            var result = (args.ReturnEntities ?? false) ?
                await GetByIds(ids, args, action, cancellation: default) :
                EntitiesResult<AdminUser>.Empty();

            // Check user permissions again
            await CheckActionPermissionsAfter(actionFilter, ids, result.Data);

            #region Non-Transactional Side-Effects

            // Send invitation emails
            var currentUser = await _repo.AdminUsers
                .FilterByIds(new List<int> { UserId })
                .FirstOrDefaultAsync(QueryContext);

            IEnumerable<AdminUserForInvitation> usersToInvite = dbUsers.Select(dbUser => new AdminUserForInvitation
            {
                Email = dbUser.Email,
                Name = dbUser.Name,
                InviterName = currentUser.Name
            });

            // Start a fresh transaction otherwise MSDTC error is raised.
            using var identityTrx = TransactionFactory.ReadCommitted(TransactionScopeOption.RequiresNew);

            await _identity.InviteUsersToAdmin(usersToInvite);

            identityTrx.Complete();

            #endregion

            trx.Complete();
            return result;
        }

        private async Task<EntitiesResult<AdminUser>> SetIsActive(List<int> ids, ActionArguments args, bool isActive)
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
            OperationOutput output = await _behavior.Repository.AdminUsers__Activate(
                    ids: ids,
                    isActive: isActive,
                    validateOnly: ModelState.IsError,
                    top: ModelState.RemainingErrors,
                    userId: UserId);

            AddErrorsAndThrowIfInvalid(output.Errors);

            var result = (args.ReturnEntities ?? false) ?
                await GetByIds(ids, args, action, cancellation: default) :
                EntitiesResult<AdminUser>.Empty();

            // Check user permissions again
            await CheckActionPermissionsAfter(actionFilter, ids, result.Data);

            trx.Complete();
            return result;
        }

        protected override Task<EntityQuery<AdminUser>> Search(EntityQuery<AdminUser> query, GetArguments args, CancellationToken _)
        {
            string search = args.Search;
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Replace("'", "''"); // escape quotes by repeating them

                var email = nameof(AdminUser.Email);
                var name = nameof(AdminUser.Name);

                query = query.Filter($"{name} contains '{search}' or {email} contains '{search}'");
            }

            return Task.FromResult(query);
        }

        protected override Task<List<AdminUserForSave>> SavePreprocessAsync(List<AdminUserForSave> entities)
        {
            foreach (var entity in entities)
            {
                // Make all the emails small case
                entity.Email = entity.Email?.ToLower();

                entity.IsService ??= false;
                if (entity.IsService.Value)
                {
                    // Service accounts do not have emails
                    entity.Email = null;
                }
                else
                {
                    // human accounts do not have a client ID
                    entity.ClientId = null;
                }
            }

            return Task.FromResult(entities);
        }

        protected override async Task<List<int>> SaveExecuteAsync(List<AdminUserForSave> entities, bool returnIds)
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

            TypeMetadata meta = null;

            foreach (var (entity, index) in entities.Select((e, i) => (e, i)))
            {
                if (entity.IsService.Value)
                {
                    // For service accounts, the ClientId is required
                    if (string.IsNullOrWhiteSpace(entity.ClientId))
                    {
                        meta ??= await GetMetadataForSave(cancellation: default);
                        var prop = meta.Property(nameof(entity.ClientId));

                        ModelState.AddError($"[{index}]{nameof(entity.ClientId)}",
                            _localizer[ErrorMessages.Error_Field0IsRequired, prop.Display()]);
                    }
                }
                else
                {
                    // For human accounts, the Email is required
                    if (string.IsNullOrWhiteSpace(entity.Email))
                    {
                        meta ??= await GetMetadataForSave(cancellation: default);
                        var prop = meta.Property(nameof(entity.Email));

                        ModelState.AddError($"[{index}]{nameof(entity.Email)}",
                            _localizer[ErrorMessages.Error_Field0IsRequired, prop.Display()]);
                    }
                }

                var lineIndices = entity.Permissions.ToIndexDictionary();
                foreach (var line in entity.Permissions)
                {
                    if (duplicateLineIds.ContainsKey(line))
                    {
                        // This error indicates a bug
                        var lineIndex = lineIndices[line];
                        var id = duplicateLineIds[line];
                        ModelState.AddError($"[{index}].{nameof(entity.Permissions)}[{lineIndex}].{nameof(entity.Id)}",
                            _localizer["Error_TheEntityWithId0IsSpecifiedMoreThanOnce", id]);
                    }
                }
            }

            // Step (2): Save users in the admin database
            var result = await _behavior.Repository.AdminUsers__Save(
                    entities: entities,
                    returnIds: returnIds,
                    validateOnly: ModelState.IsError,
                    top: ModelState.RemainingErrors,
                    userId: UserId); // Synchronizes with directory automatically

            AddErrorsAndThrowIfInvalid(result.Errors);

            // Return the new Ids
            return result.Ids;
        }

        protected override async Task NonTransactionalSideEffectsForSave(List<AdminUserForSave> entities, IReadOnlyList<AdminUser> data)
        {
            // Create the identity users
            using var identityTrx = TransactionFactory.ReadCommitted(TransactionScopeOption.RequiresNew);
            if (_identity.CanCreateUsers)
            {
                var emails = entities.Where(e => e.Email != null).Select(e => e.Email);
                await _identity.CreateUsersIfNotExist(emails);
            }

            identityTrx.Complete();
        }

        protected override async Task DeleteExecuteAsync(List<int> ids)
        {
            // Make sure the user is not deleting his/her own account
            foreach (var (id, index) in ids.Select((id, index) => (id, index)))
            {
                if (id == UserId)
                {
                    ModelState.AddError($"[{index}]", _localizer["Error_CannotDeleteYourOwnUser"].Value);
                }
            }

            var result = await _repo.AdminUsers__Delete(
                ids: ids,
                validateOnly: ModelState.IsError,
                top: ModelState.RemainingErrors,
                userId: UserId); // Synchronizes with directory automatically

            AddErrorsAndThrowIfInvalid(result.Errors);
        }

        protected override Task<ExpressionOrderBy> DefaultOrderBy(CancellationToken cancellation)
        {
            var result = ExpressionOrderBy.Parse(nameof(AdminUser.Name));
            return Task.FromResult(result);
        }
    }
}
