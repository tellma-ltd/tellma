﻿using System;
using Tellma.Controllers;
using Tellma.Controllers.ImportExport;
using Tellma.Controllers.Templating;
using Tellma.Entities;

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

            // These are helper services that business services rely on
            services = services
                .AddSingleton<IDefinitionsCache, DefinitionsCache>()
                .AddSingleton<ISettingsCache, SettingsCache>()
                .AddScoped<MetadataProvider>()
                .AddScoped<TemplateService>()
                .AddScoped<DataParser>();

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

    /// <summary>
    /// Helper methods for resolving business services
    /// </summary>
    public static class BusinessServiceExtensions
    {
        /// <summary>
        /// Retrieves the <see cref="IFactServiceBase"/> implementation associated with a certain <see cref="Entity"/> type
        /// </summary>
        public static IFactServiceBase FactServiceByEntityType(this IServiceProvider sp, Type entityType, string definitionId = null)
        {
            if (entityType is null)
            {
                throw new ArgumentNullException(nameof(entityType));
            }

            return entityType.Name switch
            {
                nameof(VoucherBooklet) => sp.GetRequiredService<VoucherBookletsService>(),
                nameof(Account) => sp.GetRequiredService<AccountsService>(),
                nameof(AccountType) => sp.GetRequiredService<AccountTypesService>(),
                nameof(AdminUser) => sp.GetRequiredService<AdminUsersService>(),
                nameof(Agent) => definitionId == null ? sp.GetRequiredService<AgentsGenericService>() : (IFactServiceBase)sp.GetRequiredService<AgentsService>().SetDefinitionId(definitionId),
                nameof(Center) => sp.GetRequiredService<CentersService>(),
                nameof(Currency) => sp.GetRequiredService<CurrenciesService>(),
                nameof(CustomClassification) => sp.GetRequiredService<CustomClassificationsService>(),
                nameof(DetailsEntry) => sp.GetRequiredService<DetailsEntriesService>(),
                nameof(Document) => definitionId == null ? sp.GetRequiredService<DocumentsGenericService>() : (IFactServiceBase)sp.GetRequiredService<DocumentsService>().SetDefinitionId(definitionId),
                nameof(EntryType) => sp.GetRequiredService<EntryTypesService>(),
                nameof(ExchangeRate) => sp.GetRequiredService<ExchangeRatesService>(),
                nameof(IdentityServerUser) => sp.GetRequiredService<IdentityServerUsersService>(),
                nameof(IfrsConcept) => sp.GetRequiredService<IfrsConceptsService>(),
                nameof(InboxRecord) => sp.GetRequiredService<InboxService>(),
                nameof(Lookup) => definitionId == null ? sp.GetRequiredService<LookupsGenericService>() : (IFactServiceBase)sp.GetRequiredService<LookupsService>().SetDefinitionId(definitionId),
                nameof(MarkupTemplate) => sp.GetRequiredService<MarkupTemplatesService>(),
                nameof(OutboxRecord) => sp.GetRequiredService<OutboxService>(),
                nameof(ReportDefinition) => sp.GetRequiredService<ReportDefinitionsService>(),
                nameof(Resource) => definitionId == null ? sp.GetRequiredService<ResourcesGenericService>() : (IFactServiceBase)sp.GetRequiredService<ResourcesService>().SetDefinitionId(definitionId),
                nameof(Role) => sp.GetRequiredService<RolesService>(),
                nameof(SummaryEntry) => sp.GetRequiredService<SummaryEntriesService>(),
                nameof(Unit) => sp.GetRequiredService<UnitsService>(),
                nameof(User) => sp.GetRequiredService<UsersService>(),

                _ => throw new InvalidOperationException($"Bug: Entity type {entityType.Name} does not have a known {nameof(IFactServiceBase)} implementation")
            };
        }


        /// <summary>
        /// Retrieves the <see cref="IFactServiceBase"/> implementation associated with a certain <see cref="Entity"/> type
        /// </summary>
        public static IFactWithIdService FactWithIdServiceByEntityType(this IServiceProvider sp, Type entityType, string definitionId = null)
        {
            if (entityType is null)
            {
                throw new ArgumentNullException(nameof(entityType));
            }

            return entityType.Name switch
            {
                nameof(VoucherBooklet) => sp.GetRequiredService<VoucherBookletsService>(),
                nameof(Account) => sp.GetRequiredService<AccountsService>(),
                nameof(AccountType) => sp.GetRequiredService<AccountTypesService>(),
                nameof(AdminUser) => sp.GetRequiredService<AdminUsersService>(),
                nameof(Agent) => definitionId == null ? sp.GetRequiredService<AgentsGenericService>() : (IFactWithIdService)sp.GetRequiredService<AgentsService>().SetDefinitionId(definitionId),
                nameof(Center) => sp.GetRequiredService<CentersService>(),
                nameof(Currency) => sp.GetRequiredService<CurrenciesService>(),
                nameof(CustomClassification) => sp.GetRequiredService<CustomClassificationsService>(),
                nameof(DetailsEntry) => sp.GetRequiredService<DetailsEntriesService>(),
                nameof(Document) => definitionId == null ? sp.GetRequiredService<DocumentsGenericService>() : (IFactWithIdService)sp.GetRequiredService<DocumentsService>().SetDefinitionId(definitionId),
                nameof(EntryType) => sp.GetRequiredService<EntryTypesService>(),
                nameof(ExchangeRate) => sp.GetRequiredService<ExchangeRatesService>(),
                nameof(IdentityServerUser) => sp.GetRequiredService<IdentityServerUsersService>(),
                nameof(IfrsConcept) => sp.GetRequiredService<IfrsConceptsService>(),
                nameof(InboxRecord) => sp.GetRequiredService<InboxService>(),
                nameof(Lookup) => definitionId == null ? sp.GetRequiredService<LookupsGenericService>() : (IFactWithIdService)sp.GetRequiredService<LookupsService>().SetDefinitionId(definitionId),
                nameof(MarkupTemplate) => sp.GetRequiredService<MarkupTemplatesService>(),
                nameof(OutboxRecord) => sp.GetRequiredService<OutboxService>(),
                nameof(ReportDefinition) => sp.GetRequiredService<ReportDefinitionsService>(),
                nameof(Resource) => definitionId == null ? sp.GetRequiredService<ResourcesGenericService>() : (IFactWithIdService)sp.GetRequiredService<ResourcesService>().SetDefinitionId(definitionId),
                nameof(Role) => sp.GetRequiredService<RolesService>(),
                nameof(Unit) => sp.GetRequiredService<UnitsService>(),
                nameof(User) => sp.GetRequiredService<UsersService>(),

                _ => throw new InvalidOperationException($"Bug: Entity type {entityType.Name} does not have a known {nameof(IFactWithIdService)} implementation")
            };
        }
    }
}

