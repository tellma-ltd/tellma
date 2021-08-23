using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api.Base;
using Tellma.Api.Behaviors;
using Tellma.Api.Dto;
using Tellma.Api.Metadata;
using Tellma.Model.Common;
using Tellma.Repository.Application;
using Tellma.Repository.Common;

namespace Tellma.Api
{
    /// <summary>
    /// Services inheriting from this class allow loading and saving a group of settings.
    /// </summary>
    public abstract class ApplicationSettingsServiceBase<TSettingsForSave, TSettings> : ServiceBase
        where TSettings : Entity
        where TSettingsForSave : Entity
    {
        private readonly ISettingsCache _settingsCache;
        private readonly IPermissionsCache _permissionsCache;
        private readonly ApplicationServiceBehavior _behavior;
        private readonly MetadataProvider _metadataProvider;

        public ApplicationSettingsServiceBase(ApplicationSettingsServiceDependencies deps) : base(deps.Context)
        {
            _settingsCache = deps.SettingsCache;
            _permissionsCache = deps.PermissionsCache;
            _behavior = deps.Behavior;
            _metadataProvider = deps.MetadataProvider;
        }

        #region API

        /// <summary>
        /// Retrieves the company settings after authorization.
        /// </summary>
        public async Task<TSettings> GetSettings(SelectExpandArguments args, CancellationToken cancellation)
        {
            await Initialize(cancellation);

            var permissions = await UserPermissions(PermissionActions.Read, cancellation);
            if (!permissions.Any())
            {
                throw new ForbiddenException();
            }

            return await GetExecute(args, cancellation);
        }

        /// <summary>
        /// Saves <paramref name="settingsForSave"/> as per the specifications in <paramref name="args"/> 
        /// after authorization.
        /// </summary>
        /// <param name="settingsForSave">The settings to save.</param>
        /// <param name="args">The specifications of the save operation.</param>
        /// <returns>Optionally returns the new READ settings and the new <see cref="SettingsForClient"/>.</returns>
        public async Task<(TSettings, Versioned<SettingsForClient>)> SaveSettings(TSettingsForSave settingsForSave, SaveArguments args)
        {
            await Initialize();

            var updatePermissions = await UserPermissions(PermissionActions.Update, cancellation: default);
            if (!updatePermissions.Any())
            {
                throw new ForbiddenException();
            }

            // Trim all string fields
            settingsForSave.StructuralPreprocess();

            // Attribute Validation
            var meta = _metadataProvider.GetMetadata(TenantId, typeof(TSettingsForSave));
            ValidateEntity(settingsForSave, meta);

            // Start the transaction
            using var trx = TransactionFactory.ReadCommitted();

            // Persist
            await SaveExecute(settingsForSave, args);

            // If requested, return the updated entity
            TSettings res = default;
            Versioned<SettingsForClient> newSettingsForClient = default;

            if (args.ReturnEntities ?? false)
            {
                // Get the latest settings for client
                newSettingsForClient = await _settingsCache.GetSettings(
                    tenantId: TenantId,
                    version: "refresh", // Random string forces a new result from the DB
                    cancellation: default);

                // If requested, return the same response you would get from a GET
                res = await GetSettings(args, cancellation: default);
            }

            trx.Complete();
            return (res, newSettingsForClient);
        }

        #endregion

        #region Helpers

        protected override IServiceBehavior Behavior => _behavior;

        /// <summary>
        /// The view to use when checking the user permissions. <br/>
        /// If you override <see cref="UserPermissions"/> then the implementation of this property doesn't matter.
        /// </summary>
        protected abstract string View { get; }

        /// <summary>
        /// Retrieves the user permissions for the current view and the specified action.
        /// </summary>
        protected virtual async Task<IEnumerable<AbstractPermission>> UserPermissions(string action, CancellationToken cancellation)
        {
            return await _permissionsCache.PermissionsFromCache(
                        _behavior.TenantId, UserId, _behavior.PermissionsVersion, View, action, cancellation);
        }

        /// <summary>
        /// Implementations retrieve the settings object according to the specifications in <paramref name="args"/>.
        /// <para/>
        /// Note: The user is already trusted to have the necessary permissions.
        /// </summary>
        /// <param name="args">The specifications according to which the settings should be retrieved.</param>
        /// <param name="cancellation">The cancellation instruction.</param>
        protected abstract Task<TSettings> GetExecute(SelectExpandArguments args, CancellationToken cancellation);

        /// <summary>
        /// Performs any preprocessing on the <paramref name="settingsForSave"/> before they are saved. This method is optional.
        /// </summary>
        /// <param name="settingsForSave">The settings to preprocess.</param>
        /// <returns>The preprocessed settings.</returns>
        protected virtual Task<TSettingsForSave> SavePreprocess(TSettingsForSave settingsForSave)
        {
            return Task.FromResult(settingsForSave);
        }

        /// <summary>
        /// Implementations perform two steps:<br/>
        /// 1) Validate <paramref name="settingsForSave"/>.<br/>
        /// 2) If invalid: throws a <see cref="ValidationException"/> containing all the errors. <br/>
        /// 3) If valid: persists <paramref name="settingsForSave"/> in the store.
        /// <para/>
        /// Note: the call to this method is already wrapped inside a transaction, and the user is trusted
        /// to have the necessary permissions, and the attribute validation is already carried out.
        /// </summary>
        /// <param name="settingsForSave">The settings to save.</param>
        /// <param name="args">The specifications of the save operation.</param>
        protected abstract Task SaveExecute(TSettingsForSave settingsForSave, SelectExpandArguments args);

        /// <summary>
        /// Syntactic sugar that returns the <see cref="ApplicationRepository"/> from the behavior.
        /// </summary>
        protected ApplicationRepository Repository => _behavior.Repository;

        /// <summary>
        /// The current TenantId.
        /// </summary>
        protected new int TenantId => _behavior.TenantId;

        #endregion
    }
}
