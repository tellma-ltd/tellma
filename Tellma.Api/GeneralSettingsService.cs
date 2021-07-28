using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api.Behaviors;
using Tellma.Api.Dto;
using Tellma.Api.Metadata;
using Tellma.Model.Application;
using Tellma.Repository.Common;

namespace Tellma.Api
{
    public class GeneralSettingsService : ApplicationSettingsServiceBase<GeneralSettingsForSave, GeneralSettings>
    {
        private readonly IStringLocalizer _localizer;
        private readonly ISettingsCache _settingsCache;
        private readonly ApplicationServiceBehavior _behavior;

        public GeneralSettingsService(
            ApplicationSettingsServiceDependencies deps,
            IStringLocalizer<FinancialSettingsService> localizer,
            ISettingsCache settingsCache) : base(deps)
        {
            _localizer = localizer;
            _settingsCache = settingsCache;
            _behavior = deps.Behavior;
        }
        
        protected override string View => "general-settings";

        public async Task<Versioned<SettingsForClient>> SettingsForClient(CancellationToken cancellation)
        {
            await Initialize(cancellation);
            return await _settingsCache.GetSettings(TenantId, _behavior.SettingsVersion, cancellation);
        }

        protected override async Task<GeneralSettings> GetExecute(SelectExpandArguments args, CancellationToken cancellation)
        {
            var ctx = new QueryContext(UserId, Today);
            var settings = await Repository.GeneralSettings
                .Select(args.Select)
                .Expand(args.Expand)
                .OrderBy("PrimaryLanguageId")
                .FirstOrDefaultAsync(ctx, cancellation);

            if (settings == null)
            {
                // Programmer mistake
                throw new InvalidOperationException("Bug: Settings have not been initialized");
            }

            return settings;
        }

        protected override Task<GeneralSettingsForSave> SavePreprocess(GeneralSettingsForSave settingsForSave)
        {
            return base.SavePreprocess(settingsForSave);
        }

        protected override async Task SaveExecute(GeneralSettingsForSave settings, SelectExpandArguments args)
        {            // C# validation
            if (!string.IsNullOrWhiteSpace(settings.SecondaryLanguageId) || !string.IsNullOrWhiteSpace(settings.TernaryLanguageId))
            {
                if (string.IsNullOrWhiteSpace(settings.PrimaryLanguageSymbol))
                {
                    ModelState.AddModelError(nameof(settings.PrimaryLanguageSymbol),
                        _localizer[ErrorMessages.Error_Field0IsRequired, _localizer["Settings_PrimaryLanguageSymbol"]]);
                }
            }

            if (string.IsNullOrWhiteSpace(settings.SecondaryLanguageId))
            {
                settings.SecondaryLanguageSymbol = null;
            }
            else
            {
                if (string.IsNullOrWhiteSpace(settings.SecondaryLanguageSymbol))
                {
                    ModelState.AddModelError(nameof(settings.SecondaryLanguageSymbol),
                        _localizer[ErrorMessages.Error_Field0IsRequired, _localizer["Settings_SecondaryLanguageSymbol"]]);
                }

                if (settings.SecondaryLanguageId == settings.PrimaryLanguageId)
                {
                    ModelState.AddModelError(nameof(settings.SecondaryLanguageId),
                        _localizer["Error_SecondaryLanguageCannotBeTheSameAsPrimaryLanguage"]);
                }
            }

            if (string.IsNullOrWhiteSpace(settings.TernaryLanguageId))
            {
                settings.TernaryLanguageSymbol = null;
            }
            else
            {
                if (string.IsNullOrWhiteSpace(settings.TernaryLanguageSymbol))
                {
                    ModelState.AddModelError(nameof(settings.TernaryLanguageSymbol),
                        _localizer[ErrorMessages.Error_Field0IsRequired, _localizer["Settings_TernaryLanguageSymbol"]]);
                }

                if (settings.TernaryLanguageId == settings.PrimaryLanguageId)
                {
                    ModelState.AddModelError(nameof(settings.TernaryLanguageId),
                        _localizer["Error_TernaryLanguageCannotBeTheSameAsPrimaryLanguage"]);
                }

                if (settings.TernaryLanguageId == settings.SecondaryLanguageId)
                {
                    ModelState.AddModelError(nameof(settings.TernaryLanguageId),
                        _localizer["Error_TernaryLanguageCannotBeTheSameAsSecondaryLanguage"]);
                }
            }

            if (!string.IsNullOrWhiteSpace(settings.SecondaryCalendar))
            {
                if (settings.PrimaryCalendar == settings.SecondaryCalendar)
                {
                    ModelState.AddModelError(nameof(settings.SecondaryCalendar),
                        _localizer["Error_SecondaryCalendarCannotBeTheSameAsPrimaryCalendar"]);
                }
            }

            // Make sure the color is a valid HTML color
            // Credit: https://bit.ly/2ToV6x4
            if (!string.IsNullOrWhiteSpace(settings.BrandColor) && !Regex.IsMatch(settings.BrandColor, "^#(?:[0-9a-fA-F]{3}){1,2}$"))
            {
                ModelState.AddModelError(nameof(settings.BrandColor),
                    _localizer["Error_TheField0MustBeAValidColorFormat", _localizer["Settings_BrandColor"]]);
            }

            if (!ModelState.IsValid)
            {
                return;
            }

            // Persist
            await Repository.GeneralSettings__Save(settings, UserId);
        }
    }
}
