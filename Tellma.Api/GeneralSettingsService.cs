using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api.Behaviors;
using Tellma.Api.Dto;
using Tellma.Api.Metadata;
using Tellma.Integration.Zatca;
using Tellma.Model.Application;
using Tellma.Model.Common;
using Tellma.Repository.Common;

namespace Tellma.Api
{
    public class GeneralSettingsService : ApplicationSettingsServiceBase<GeneralSettingsForSave, GeneralSettings>
    {
        private readonly IStringLocalizer _localizer;
        private readonly ZatcaService _zatcaService;
        private readonly ISettingsCache _settingsCache;
        private readonly ApplicationServiceBehavior _behavior;
        private static readonly EmailAddressAttribute emailAtt = new();

        public GeneralSettingsService(
            ApplicationSettingsServiceDependencies deps,
            IStringLocalizer<Strings> localizer,
            ZatcaService zatcaService,
            ISettingsCache settingsCache) : base(deps)
        {
            _localizer = localizer;
            _zatcaService = zatcaService;
            _settingsCache = settingsCache;
            _behavior = deps.Behavior;
        }

        protected override string View => "general-settings";

        public async Task<Versioned<SettingsForClient>> SettingsForClient(CancellationToken cancellation)
        {
            await Initialize(cancellation);
            return await _settingsCache.GetSettings(TenantId, _behavior.SettingsVersion, cancellation);
        }

        public async Task Ping(CancellationToken cancellation)
        {
            await Initialize(cancellation);
        }

        protected override async Task<GeneralSettings> GetExecute(SelectExpandArguments args, CancellationToken cancellation)
        {
            var ctx = new QueryContext(UserId, Today);
            var settings = await Repository.GeneralSettings
                .Select(args.Select)
                .Expand(args.Expand)
                .OrderBy("PrimaryLanguageId")
                .FirstOrDefaultAsync(ctx, cancellation) ?? throw new InvalidOperationException("Bug: Settings have not been initialized");

            // Upon read, JSON takes precedent
            try
            {
                settings.CustomFields = JsonConvert.DeserializeObject<GeneralSettings.Custom>(settings.CustomFieldsJson ?? "{}");
            }
            catch
            {
                // A way out in case the DB contains invalid JSON for some reason
                settings.CustomFields = new();
            }

            return settings;
        }

        protected override Task<GeneralSettingsForSave> SavePreprocess(GeneralSettingsForSave settingsForSave)
        {
            // Upon save, object takes precedent
            settingsForSave.CustomFieldsJson = JsonConvert.SerializeObject(settingsForSave.CustomFields ?? new());
            return base.SavePreprocess(settingsForSave);
        }

        protected override async Task SaveExecute(GeneralSettingsForSave settings, SelectExpandArguments args)
        {            // C# validation
            if (!string.IsNullOrWhiteSpace(settings.SecondaryLanguageId) || !string.IsNullOrWhiteSpace(settings.TernaryLanguageId))
            {
                if (string.IsNullOrWhiteSpace(settings.PrimaryLanguageSymbol))
                {
                    ModelState.AddError(nameof(settings.PrimaryLanguageSymbol),
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
                    ModelState.AddError(nameof(settings.SecondaryLanguageSymbol),
                        _localizer[ErrorMessages.Error_Field0IsRequired, _localizer["Settings_SecondaryLanguageSymbol"]]);
                }

                if (settings.SecondaryLanguageId == settings.PrimaryLanguageId)
                {
                    ModelState.AddError(nameof(settings.SecondaryLanguageId),
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
                    ModelState.AddError(nameof(settings.TernaryLanguageSymbol),
                        _localizer[ErrorMessages.Error_Field0IsRequired, _localizer["Settings_TernaryLanguageSymbol"]]);
                }

                if (settings.TernaryLanguageId == settings.PrimaryLanguageId)
                {
                    ModelState.AddError(nameof(settings.TernaryLanguageId),
                        _localizer["Error_TernaryLanguageCannotBeTheSameAsPrimaryLanguage"]);
                }

                if (settings.TernaryLanguageId == settings.SecondaryLanguageId)
                {
                    ModelState.AddError(nameof(settings.TernaryLanguageId),
                        _localizer["Error_TernaryLanguageCannotBeTheSameAsSecondaryLanguage"]);
                }
            }

            if (!string.IsNullOrWhiteSpace(settings.SecondaryCalendar))
            {
                if (settings.PrimaryCalendar == settings.SecondaryCalendar)
                {
                    ModelState.AddError(nameof(settings.SecondaryCalendar),
                        _localizer["Error_SecondaryCalendarCannotBeTheSameAsPrimaryCalendar"]);
                }
            }

            // Make sure the color is a valid HTML color
            // Credit: https://bit.ly/2ToV6x4
            if (!string.IsNullOrWhiteSpace(settings.BrandColor) && !Regex.IsMatch(settings.BrandColor, "^#(?:[0-9a-fA-F]{3}){1,2}$"))
            {
                ModelState.AddError(nameof(settings.BrandColor),
                    _localizer["Error_TheField0MustBeAValidColorFormat", _localizer["Settings_BrandColor"]]);
            }

            if (!string.IsNullOrWhiteSpace(settings.SupportEmails))
            {
                var emailAddresses = settings.SupportEmails
                    .Split(';')
                    .Where(e => !string.IsNullOrWhiteSpace(e))
                    .Select(e => e.Trim());

                if (emailAddresses.Any(e => !emailAtt.IsValid(e)))
                {
                    ModelState.AddError(nameof(settings.SupportEmails),
                        _localizer[ErrorMessages.Error_Field0IsNotValidEmail, _localizer["Settings_SupportEmails"]]);
                }
            }

            // Persist
            var result = await Repository.GeneralSettings__Save(
                    settingsForSave: settings,
                    validateOnly: ModelState.IsError,
                    top: ModelState.RemainingErrors,
                    userId: UserId);

            AddLocalizedErrors(result.Errors, _localizer);
            ModelState.ThrowIfInvalid();
        }


        public async Task OnboardWithZatca(string otp, string orgUnitName, string industry)
        {
            await Initialize();

            // Check permissions
            var updatePermissions = await UserPermissions(PermissionActions.Update, cancellation: default);
            if (!updatePermissions.Any())
            {
                throw new ForbiddenException();
            }

            // Validation
            if (string.IsNullOrWhiteSpace(otp))
            {
                throw new ServiceException("Please supply the otp parameter.");
            }

            if (string.IsNullOrWhiteSpace(orgUnitName))
            {
                throw new ServiceException("Please supply the orgUnitName parameter.");
            }

            if (string.IsNullOrWhiteSpace(industry))
            {
                throw new ServiceException("Please supply the industry parameter.");
            }

            var settings = (await _settingsCache.GetSettings(TenantId, _behavior.SettingsVersion)).Data;
            var vatNumber = settings.TaxIdentificationNumber;
            var orgName = settings.CompanyName;
            var env = settings.ZatcaEnvironment switch
            {
                "Sandbox" => Env.Sandbox,
                "Simulation" => Env.Simulation,
                "Production" => Env.Production,
                _ => throw new InvalidOperationException($"Unrecognized ZatcaEnvironment {settings.ZatcaEnvironment}"),
            };

            if (string.IsNullOrWhiteSpace(vatNumber))
            {
                throw new ServiceException("Please initialize the Tax Identification Number in the financial settings");
            }

            if (string.IsNullOrWhiteSpace(orgName))
            {
                throw new ServiceException("Please initialize the Company Name in the general settings");
            }

            // Onboard with ZATCA
            var secrets = await _zatcaService.Onboard(
                tenantId: TenantId,
                vatNumber: vatNumber,
                orgUnitName: orgUnitName,
                orgName: orgName,
                orgIndustry: industry,
                otp: otp,
                env: env);

            await Repository.Zatca__SaveSecrets(
                encryptedSecurityToken: secrets.EncryptedSecurityToken, 
                encryptedSecret: secrets.EncryptedSecret, 
                encryptedPrivateKey: secrets.EncryptedPrivateKey,
                encryptionKeyIndex: secrets.EncryptionKeyIndex);
        }
    }
}
