using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Tellma.Api.Base;
using Tellma.Api.Behaviors;
using Tellma.Api.Dto;
using Tellma.Model.Common;
using Tellma.Repository.Common;

namespace Tellma.Api
{
    /// <summary>
    /// Services inheriting from this class allow searching, aggregating and exporting a certain
    /// entity type using OData-like parameters.
    /// </summary>
    public abstract class ApplicationSettingsServiceBase<TSettingsForSave, TSettings> : ServiceBase
        where TSettings : Entity
        where TSettingsForSave : Entity
    {
        private readonly ISettingsCache _settingsCache;
        private readonly ApplicationVersions _versions;

        public ApplicationSettingsServiceBase(ApplicationSettingsServiceDependencies deps) : base(deps.Context)
        {
            _settingsCache = deps.SettingsCache;
            _versions = deps.Versions;
        }

        #region API

        /// <summary>
        /// Retrieves the company settings after authorization.
        /// </summary>
        public async Task<TSettings> GetSettings(SelectExpandArguments args, CancellationToken cancellation)
        {
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
            var updatePermissions = await UserPermissions(PermissionActions.Update, cancellation: default);
            if (!updatePermissions.Any())
            {
                throw new ForbiddenException();
            }

            // Trim all string fields
            settingsForSave.TrimStringProperties();

            // Start the transaction
            using var trx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

            // Persist
            await SaveExecute(settingsForSave, args);

            // If requested, return the updated entity
            TSettings res = default;
            Versioned<SettingsForClient> newSettings = default;

            if (args.ReturnEntities ?? false)
            {
                // Get the latest settings for client
                int tenantId = TenantId ?? throw new ServiceException($"TenantId not supplied.");
                newSettings = await _settingsCache.GetSettings(
                    tenantId: tenantId,
                    version: null, // Forces a new result from the DB
                    cancellation: default);

                _versions.SettingsVersion = newSettings.Version; // Lets the client know that any cached settings are now stale

                // If requested, return the same response you would get from a GET
                res = await GetSettings(args, cancellation: default);
            }

            trx.Complete();
            return (res, newSettings);
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Implementations retrieve the settings object according to the specifications in <paramref name="args"/>.
        /// </summary>
        /// <param name="args">The specifications according to which the settings should be retrieved.</param>
        /// <param name="cancellation">The cancellation instruction.</param>
        protected abstract Task<TSettings> GetExecute(SelectExpandArguments args, CancellationToken cancellation);

        /// <summary>
        /// Implementations perform three steps:<br/>
        /// 1) Preprocess <paramref name="settingsForSave"/> (optional).<br/>
        /// 2) Validate <paramref name="settingsForSave"/> and add the validation errors in the model state.<br/>
        /// 3) If valid: persists <paramref name="settingsForSave"/> in the store.
        /// <para/>
        /// Note: the call to this method is already wrapped inside a transaction, and the user is trusted
        /// to have the necessary permissions.
        /// </summary>
        /// <param name="settingsForSave">The settings to save.</param>
        /// <param name="args">The specifications of the save operation.</param>
        protected abstract Task SaveExecute(TSettingsForSave settingsForSave, SelectExpandArguments args);

        /// <summary>
        /// Retrieves the user permissions for the current view and the specified action.
        /// </summary>
        protected abstract Task<IEnumerable<AbstractPermission>> UserPermissions(string action, CancellationToken cancellation);

        #endregion
    }
}
