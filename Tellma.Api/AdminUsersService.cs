using Microsoft.Extensions.Localization;
using System;
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
        private readonly MetadataProvider _metadataProvider;

        protected override string View => "admin-users";

        protected override IFactServiceBehavior FactBehavior => _behavior;

        public AdminUsersService(
            AdminFactServiceBehavior behavior,
            CrudServiceDependencies deps,
            AdminRepository repo,
            IIdentityProxy identity) : base(deps)
        {
            _behavior = behavior;
            _repo = repo;
            _localizer = deps.Localizer;
            _identity = identity;
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
            await Initialize(cancellation);

            // Prepare the odata query
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

            // Basic validation
            var meta = _metadataProvider.GetMetadata(null, typeof(AdminUserForSave));
            ValidateEntity(userForSave, meta);
            ModelState.ThrowIfInvalid();

            var entities = new List<AdminUserForSave>() { userForSave };

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
            OperationResult result = await _behavior.Repository.AdminUsers__Activate(ids, isActive, userId: UserId);
            AddLocalizedErrors(result.Errors);
            ModelState.ThrowIfInvalid();

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
            // Make all the emails small case
            entities.ForEach(e => e.Email = e.Email.ToLower());
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
                return null;
            }

            // Step (2): Save users in the application database
            var result = await _behavior.Repository.AdminUsers__Save(entities, returnIds: returnIds, userId: UserId); // Synchronizes with directory automatically
            AddLocalizedErrors(result.Errors);

            // Return the new Ids
            return result.Ids;
        }

        protected override async Task NonTransactionalSideEffectsForSave(List<AdminUserForSave> entities, List<AdminUser> data)
        {
            // Create the identity users
            using var identityTrx = new TransactionScope(TransactionScopeOption.RequiresNew, TransactionScopeAsyncFlowOption.Enabled);
            if (_identity.CanCreateUsers)
            {
                var emails = entities.Select(e => e.Email);
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


            try
            {
                var result = await _repo.AdminUsers__Delete(ids, userId: UserId); // Synchronizes with directory automatically
                AddLocalizedErrors(result.Errors);
            }
            catch (ForeignKeyViolationException)
            {
                var meta = await GetMetadata(cancellation: default);
                throw new ServiceException(_localizer["Error_CannotDelete0AlreadyInUse", meta.SingularDisplay()]);
            }
        }

        protected override Task<ExpressionOrderBy> DefaultOrderBy(CancellationToken cancellation)
        {
            var result = ExpressionOrderBy.Parse(nameof(AdminUser.Name));
            return Task.FromResult(result);
        }
    }
}
