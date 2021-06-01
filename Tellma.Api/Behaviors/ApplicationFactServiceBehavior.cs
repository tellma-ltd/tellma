using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api.Base;
using Tellma.Api.Dto;
using Tellma.Api.Metadata;
using Tellma.Api.Templating;
using Tellma.Model.Application;
using Tellma.Model.Common;
using Tellma.Repository.Admin;
using Tellma.Repository.Application;
using Tellma.Repository.Common;
using Tellma.Utilities.Common;

namespace Tellma.Api.Behaviors
{
    public class ApplicationFactServiceBehavior : ApplicationServiceBehavior, IFactServiceBehavior
    {
        private static readonly ConcurrentDictionary<int, ApplicationMetadataOverridesProvider> _overridesCache = new();

        private readonly IDefinitionsCache _definitionsCache;
        private readonly ISettingsCache _settingsCache;
        private readonly IUserSettingsCache _userSettingsCache;
        private readonly IStringLocalizer<ApplicationFactServiceBehavior> _localizer;

        private 

        protected int? DefinitionId { get; private set; }

        public ApplicationFactServiceBehavior(
            IServiceContextAccessor context,
            IApplicationRepositoryFactory factory,
            AdminRepository adminRepo,
            ILogger<ApplicationServiceBehavior> logger,
            IDefinitionsCache definitionsCache,
            ISettingsCache settingsCache,
            IUserSettingsCache userSettingsCache,
            IStringLocalizer<ApplicationFactServiceBehavior> localizer) : base(context, factory, adminRepo, logger)
        {
            _definitionsCache = definitionsCache;
            _settingsCache = settingsCache;
            _userSettingsCache = userSettingsCache;
            _localizer = localizer;
        }

        public IQueryFactory QueryFactory<TEntity>() where TEntity : Entity
        {
            if (DefinitionId != null)
            {
                return new FilteredQueryFactory<TEntity>(
                    Repository, $"DefinitionId eq {DefinitionId}");
            }
            else
            {
                return Repository;
            }
        }

        public async Task<IMetadataOverridesProvider> GetMetadataOverridesProvider(CancellationToken cancellation)
        {
            var tenantId = TenantId;
            var settings = (await _settingsCache.GetSettings(tenantId, SettingsVersion, cancellation)).Data;
            var definitions = (await _definitionsCache.GetDefinitions(tenantId, SettingsVersion, cancellation)).Data;

            var provider = _overridesCache.GetOrAdd(tenantId, 
                _ => new ApplicationMetadataOverridesProvider(_localizer, definitions, settings));

            if (provider.Definitions != definitions || provider.Settings != settings)
            {
                _overridesCache.TryRemove(tenantId, out _);
                return await GetMetadataOverridesProvider(cancellation);
            }

            return provider;
        }

        public async Task<AbstractMarkupTemplate> GetMarkupTemplate<TEntity>(int templateId, CancellationToken cancellation) where TEntity : Entity
        {
            var collection = typeof(TEntity).Name;
            var defId = DefinitionId;
            var repo = QueryFactory<TEntity>();

            var template = await repo.EntityQuery<MarkupTemplate>()
                .FilterByIds(new int[] { templateId })
                .FirstOrDefaultAsync(new QueryContext(UserId), cancellation);

            if (template == null)
            {
                // Shouldn't happen in theory cause of previous check, but just to be extra safe
                throw new ServiceException($"The {nameof(MarkupTemplate)} with Id {templateId} does not exist.");
            }

            if (!(template.IsDeployed ?? false))
            {
                // A proper UI will only allow the user to use supported template
                throw new ServiceException($"The {nameof(MarkupTemplate)} with Id {templateId} is not deployed.");
            }

            //// The errors below should be prevented through SQL validation, but just to be safe
            //if (template.Usage != MarkupTemplateConst.QueryByFilter)
            //{
            //    throw new ServiceException($"The {nameof(MarkupTemplate)} with Id {templateId} does not have the proper usage.");
            //}

            if (template.MarkupLanguage != MimeTypes.Html)
            {
                throw new ServiceException($"The {nameof(MarkupTemplate)} with Id {templateId} is not an HTML template.");
            }

            if (template.Collection != collection)
            {
                throw new ServiceException($"The {nameof(MarkupTemplate)} with Id {templateId} does not have Collection = '{collection}'.");
            }

            if (template.DefinitionId != null && template.DefinitionId != defId)
            {
                throw new ServiceException($"The {nameof(MarkupTemplate)} with Id {templateId} has an incompatible DefinitionId to '{defId}'.");
            }

            return new AbstractMarkupTemplate(template.Body, template.DownloadName, template.MarkupLanguage);
        }

        public async Task SetMarkupVariables(
            Dictionary<string, EvaluationVariable> localVars,
            Dictionary<string, EvaluationVariable> globalVars, 
            CancellationToken cancellation)
        {
            globalVars.Add("$UserEmail", new EvaluationVariable(UserEmail));

            var settings = (await _settingsCache.GetSettings(TenantId, SettingsVersion, cancellation)).Data;
            globalVars.Add("$ShortCompanyName", new EvaluationVariable(settings.ShortCompanyName));
            globalVars.Add("$ShortCompanyName2", new EvaluationVariable(settings.ShortCompanyName2));
            globalVars.Add("$ShortCompanyName3", new EvaluationVariable(settings.ShortCompanyName3));
            globalVars.Add("$TaxIdentificationNumber", new EvaluationVariable(settings.TaxIdentificationNumber));

            var userSettings = (await _userSettingsCache.GetUserSettings(UserId, TenantId, UserSettingsVersion, cancellation)).Data;
            globalVars.Add("$UserName", new EvaluationVariable(userSettings.Name));
            globalVars.Add("$UserName2", new EvaluationVariable(userSettings.Name2));
            globalVars.Add("$UserName3", new EvaluationVariable(userSettings.Name3));
        }

        public Task SetMarkupFunctions(Dictionary<string, EvaluationFunction> localFuncs, Dictionary<string, EvaluationFunction> globalFuncs, CancellationToken cancellation)
        {
            globalFuncs.Add(nameof(Localize), Localize());
            return Task.CompletedTask;
        }

        public void SetDefinitionId(int definitionId) => DefinitionId = definitionId;

        #region Localize

        private EvaluationFunction Localize() => new(functionAsync: LocalizeImpl);

        private async Task<object> LocalizeImpl(object[] args, EvaluationContext _)
        {
            int minArgCount = 2;
            int maxArgCount = 3;
            if (args.Length < minArgCount || args.Length > maxArgCount)
            {
                throw new TemplateException($"Function '{nameof(Localize)}' expects at least {minArgCount} and at most {maxArgCount} arguments.");
            }

            int i = 0;

            object sObj = args[i++];
            object sObj2 = args[i++];
            object sObj3 = args.Length > i ? args[i++] : null;

            string s = null;
            if (sObj is null || sObj is string)
            {
                s = sObj as string;
            }
            else
            {
                throw new TemplateException($"Function '{nameof(Localize)}' expects a 1st argument of type string.");
            }

            string s2 = null;
            if (sObj2 is null || sObj2 is string)
            {
                s2 = sObj2 as string;
            }
            else
            {
                throw new TemplateException($"Function '{nameof(Localize)}' expects a 2nd argument of type string.");
            }

            string s3 = null;
            if (sObj3 is null || sObj3 is string)
            {
                s3 = sObj3 as string;
            }
            else
            {
                throw new TemplateException($"Function '{nameof(Localize)}' expects a 3rd argument of type string.");
            }

            var settings = (await _settingsCache.GetSettings(TenantId, SettingsVersion, Cancellation)).Data;
            return settings.Localize(s, s2, s3);
        }

        #endregion
    }
}
