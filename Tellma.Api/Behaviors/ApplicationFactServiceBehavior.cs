using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tellma.Api.Base;
using Tellma.Api.Dto;
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
        private readonly ISettingsCache _settings;
        private readonly IUserSettingsCache _userSettings;

        protected int? DefinitionId { get; private set; }

        public ApplicationFactServiceBehavior(
            IServiceContextAccessor context,
            IApplicationRepositoryFactory factory,
            AdminRepository adminRepo,
            ILogger<ApplicationServiceBehavior> logger,
            ISettingsCache settings,
            IUserSettingsCache userSettings) : base(context, factory, adminRepo, logger)
        {
            _settings = settings;
            _userSettings = userSettings;
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

        public async Task<AbstractMarkupTemplate> GetMarkupTemplate<TEntity>(int templateId) where TEntity : Entity
        {
            var collection = typeof(TEntity).Name;
            var defId = DefinitionId;
            var repo = QueryFactory<TEntity>();

            var template = await repo.EntityQuery<MarkupTemplate>()
                .FilterByIds(new int[] { templateId })
                .FirstOrDefaultAsync(new QueryContext(UserId), Cancellation);

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

            // The errors below should be prevented through SQL validation, but just to be safe
            if (template.Usage != MarkupTemplateConst.QueryByFilter)
            {
                throw new ServiceException($"The {nameof(MarkupTemplate)} with Id {templateId} does not have the proper usage.");
            }

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

        public async Task SetMarkupVariables(Dictionary<string, EvaluationVariable> localVars, Dictionary<string, EvaluationVariable> globalVars)
        {
            globalVars.Add("$UserEmail", new EvaluationVariable(UserEmail));

            var settings = (await _settings.GetSettings(TenantId, SettingsVersion, Cancellation)).Data;
            globalVars.Add("$ShortCompanyName", new EvaluationVariable(settings.ShortCompanyName));
            globalVars.Add("$ShortCompanyName2", new EvaluationVariable(settings.ShortCompanyName2));
            globalVars.Add("$ShortCompanyName3", new EvaluationVariable(settings.ShortCompanyName3));
            globalVars.Add("$TaxIdentificationNumber", new EvaluationVariable(settings.TaxIdentificationNumber));

            var userSettings = (await _userSettings.GetUserSettings(UserId, TenantId, UserSettingsVersion, Cancellation)).Data;
            globalVars.Add("$UserName", new EvaluationVariable(userSettings.Name));
            globalVars.Add("$UserName2", new EvaluationVariable(userSettings.Name2));
            globalVars.Add("$UserName3", new EvaluationVariable(userSettings.Name3));
        }

        public Task SetMarkupFunctions(Dictionary<string, EvaluationFunction> localFuncs, Dictionary<string, EvaluationFunction> globalFuncs)
        {
            globalFuncs.Add(nameof(Localize), Localize());
            return Task.CompletedTask;
        }

        public void SetDefinitionId(int definitionId) => DefinitionId = definitionId;

        #region Localize

        private EvaluationFunction Localize() => new EvaluationFunction(functionAsync: LocalizeImpl);

        private async Task<object> LocalizeImpl(object[] args, EvaluationContext _)
        {
            int minArgCount = 2;
            int maxArgCount = 3;
            if (args.Length < minArgCount || args.Length > maxArgCount)
            {
                throw new TemplateException($"Function '{nameof(Localize)}' expects at least {minArgCount} and at most {maxArgCount} arguments.");
            }

            object sObj = args[0];
            object sObj2 = args[1];
            object sObj3 = args.Length > 2 ? args[2] : null;

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

            var settings = (await _settings.GetSettings(TenantId, SettingsVersion, Cancellation)).Data;
            return settings.Localize(s, s2, s3);
        }

        #endregion
    }

    public static class SettingsForClientExtensions
    {
        public static string Localize(this SettingsForClient settings, string s, string s2, string s3)
        {
            var cultureName = System.Globalization.CultureInfo.CurrentUICulture.Name;

            var currentLangIndex = cultureName == settings.TernaryLanguageId ? 3 : cultureName == settings.SecondaryLanguageId ? 2 : 1;
            return currentLangIndex == 3 ? (s3 ?? s) : currentLangIndex == 2 ? (s2 ?? s) : s;
        }
    }
}
