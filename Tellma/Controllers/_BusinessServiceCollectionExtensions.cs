using System;
using Tellma.Controllers;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class BusinessServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the services that encapsulate the business logic of the system
        /// </summary>
        public static IServiceCollection AddBusinessServices(this IServiceCollection services)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            return services
                .AddScoped<VoucherBookletsService>()
                .AddScoped<AccountsService>()
                .AddScoped<AccountTypesService>()
                .AddScoped<AdminPermissionsService>()
                .AddScoped<AdminSettingsService>()
                .AddScoped<AdminUsersService>()
                .AddScoped<AgentsService>()
                .AddScoped<AgentsGenericService>()
                .AddScoped<CentersService>()
                .AddScoped<CompaniesService>()
                .AddScoped<CurrenciesService>()
                .AddScoped<CustomClassificationsService>()
                .AddScoped<DefinitionsService>()
                .AddScoped<DetailsEntriesService>()
                .AddScoped<DocumentsService>()
                .AddScoped<DocumentsGenericService>()
                .AddScoped<EntryTypesService>()
                .AddScoped<ExchangeRatesService>()
                // .AddScoped<GlobalSettingsService>()
                .AddScoped<IdentityServerUsersService>()
                .AddScoped<IfrsConceptsService>()
                .AddScoped<InboxService>()
                .AddScoped<LookupsService>()
                .AddScoped<LookupsGenericService>()
                .AddScoped<MarkupTemplatesService>()
                .AddScoped<OutboxService>()
                .AddScoped<PermissionsService>()
                .AddScoped<ReportDefinitionsService>()
                .AddScoped<ResourcesService>()
                .AddScoped<ResourcesGenericService>()
                .AddScoped<RolesService>()
                .AddScoped<SettingsService>()
                .AddScoped<SummaryEntriesService>()
                .AddScoped<UnitsService>()
                .AddScoped<UsersService>();
        }
    }
}

