using Microsoft.Extensions.Configuration;
using Microsoft.VisualBasic;
using System;
using Tellma.Controllers;
using Tellma.Controllers.ImportExport;
using Tellma.Controllers.Jobs;
using Tellma.Controllers.Templating;
using Tellma.Entities;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class BusinessServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the services that encapsulate the business logic of the system
        /// </summary>
        public static IServiceCollection AddBusinessServices(this IServiceCollection services, IConfiguration config)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            //// Bind
            //services.Configure<JobsOptions>(config.GetSection("Jobs"));

            //// Register background jobs
            //services = services
            //    .AddSingleton<InstanceInfoProvider>()
            //    .AddHostedService<HeartbeatJob>()
            //    .AddHostedService<OrphanCareJob>();

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
                .AddScoped<RelationsService>()
                .AddScoped<RelationsGenericService>()
                .AddScoped<RelationDefinitionsService>()
                .AddScoped<CustodiesService>()
                .AddScoped<CustodiesGenericService>()
                .AddScoped<CustodyDefinitionsService>()
                .AddScoped<CentersService>()
                .AddScoped<CompaniesService>()
                .AddScoped<CurrenciesService>()
                .AddScoped<AccountClassificationsService>()
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
                .AddScoped<ReconciliationService>()
                .AddScoped<SummaryEntriesService>()
                .AddScoped<UnitsService>()
                .AddScoped<ResourceDefinitionsService>()
                .AddScoped<LookupDefinitionsService>()
                .AddScoped<UsersService>()
                .AddScoped<DocumentDefinitionsService>()
                .AddScoped<LineDefinitionsService>();
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
        public static IFactServiceBase FactServiceByCollectionName(this IServiceProvider sp, string collection, int? definitionId = null)
        {
            return collection switch
            {
                nameof(VoucherBooklet) => sp.GetRequiredService<VoucherBookletsService>(),
                nameof(Account) => sp.GetRequiredService<AccountsService>(),
                nameof(AccountType) => sp.GetRequiredService<AccountTypesService>(),
                nameof(AdminUser) => sp.GetRequiredService<AdminUsersService>(),
                nameof(Agent) => sp.GetRequiredService<AgentsService>(),
                nameof(RelationDefinition) => sp.GetRequiredService<RelationDefinitionsService>(),
                nameof(Relation) => definitionId == null ? sp.GetRequiredService<RelationsGenericService>() : (IFactServiceBase)sp.GetRequiredService<RelationsService>().SetDefinitionId(definitionId.Value),
                nameof(CustodyDefinition) => sp.GetRequiredService<CustodyDefinitionsService>(),
                nameof(Custody) => definitionId == null ? sp.GetRequiredService<CustodiesGenericService>() : (IFactServiceBase)sp.GetRequiredService<CustodiesService>().SetDefinitionId(definitionId.Value),
                nameof(Center) => sp.GetRequiredService<CentersService>(),
                nameof(Currency) => sp.GetRequiredService<CurrenciesService>(),
                nameof(AccountClassification) => sp.GetRequiredService<AccountClassificationsService>(),
                nameof(DetailsEntry) => sp.GetRequiredService<DetailsEntriesService>(),
                nameof(Document) => definitionId == null ? sp.GetRequiredService<DocumentsGenericService>() : (IFactServiceBase)sp.GetRequiredService<DocumentsService>().SetDefinitionId(definitionId.Value),
                nameof(EntryType) => sp.GetRequiredService<EntryTypesService>(),
                nameof(ExchangeRate) => sp.GetRequiredService<ExchangeRatesService>(),
                nameof(IdentityServerUser) => sp.GetRequiredService<IdentityServerUsersService>(),
                nameof(IfrsConcept) => sp.GetRequiredService<IfrsConceptsService>(),
                nameof(InboxRecord) => sp.GetRequiredService<InboxService>(),
                nameof(LookupDefinition) => sp.GetRequiredService<LookupDefinitionsService>(),
                nameof(Lookup) => definitionId == null ? sp.GetRequiredService<LookupsGenericService>() : (IFactServiceBase)sp.GetRequiredService<LookupsService>().SetDefinitionId(definitionId.Value),
                nameof(MarkupTemplate) => sp.GetRequiredService<MarkupTemplatesService>(),
                nameof(OutboxRecord) => sp.GetRequiredService<OutboxService>(),
                nameof(ReportDefinition) => sp.GetRequiredService<ReportDefinitionsService>(),
                nameof(ResourceDefinition) => sp.GetRequiredService<ResourceDefinitionsService>(),
                nameof(Resource) => definitionId == null ? sp.GetRequiredService<ResourcesGenericService>() : (IFactServiceBase)sp.GetRequiredService<ResourcesService>().SetDefinitionId(definitionId.Value),
                nameof(Role) => sp.GetRequiredService<RolesService>(),
                nameof(SummaryEntry) => sp.GetRequiredService<SummaryEntriesService>(),
                nameof(Unit) => sp.GetRequiredService<UnitsService>(),
                nameof(User) => sp.GetRequiredService<UsersService>(),
                nameof(DocumentDefinition) => sp.GetRequiredService<DocumentDefinitionsService>(),
                nameof(LineDefinition) => sp.GetRequiredService<LineDefinitionsService>(),

                _ => throw new UnknownCollectionException($"Collection {collection} does not have a known {nameof(IFactServiceBase)} implementation")
            };
        }

        /// <summary>
        /// Retrieves the <see cref="IFactServiceBase"/> implementation associated with a certain <see cref="Entity"/> type
        /// </summary>
        public static IFactWithIdService FactWithIdServiceByEntityType(this IServiceProvider sp, string collection, int? definitionId = null)
        {
            return collection switch
            {
                nameof(VoucherBooklet) => sp.GetRequiredService<VoucherBookletsService>(),
                nameof(Account) => sp.GetRequiredService<AccountsService>(),
                nameof(AccountType) => sp.GetRequiredService<AccountTypesService>(),
                nameof(AdminUser) => sp.GetRequiredService<AdminUsersService>(),
                nameof(Agent) => sp.GetRequiredService<AgentsService>(),
                nameof(RelationDefinition) => sp.GetRequiredService<RelationDefinitionsService>(),
                nameof(Relation) => definitionId == null ? sp.GetRequiredService<RelationsGenericService>() : (IFactWithIdService)sp.GetRequiredService<RelationsService>().SetDefinitionId(definitionId.Value),
                nameof(CustodyDefinition) => sp.GetRequiredService<CustodyDefinitionsService>(),
                nameof(Custody) => definitionId == null ? sp.GetRequiredService<CustodiesGenericService>() : (IFactWithIdService)sp.GetRequiredService<CustodiesService>().SetDefinitionId(definitionId.Value),
                nameof(Center) => sp.GetRequiredService<CentersService>(),
                nameof(Currency) => sp.GetRequiredService<CurrenciesService>(),
                nameof(AccountClassification) => sp.GetRequiredService<AccountClassificationsService>(),
                nameof(DetailsEntry) => sp.GetRequiredService<DetailsEntriesService>(),
                nameof(Document) => definitionId == null ? sp.GetRequiredService<DocumentsGenericService>() : (IFactWithIdService)sp.GetRequiredService<DocumentsService>().SetDefinitionId(definitionId.Value),
                nameof(EntryType) => sp.GetRequiredService<EntryTypesService>(),
                nameof(ExchangeRate) => sp.GetRequiredService<ExchangeRatesService>(),
                nameof(IdentityServerUser) => sp.GetRequiredService<IdentityServerUsersService>(),
                nameof(IfrsConcept) => sp.GetRequiredService<IfrsConceptsService>(),
                nameof(InboxRecord) => sp.GetRequiredService<InboxService>(),
                nameof(LookupDefinition) => sp.GetRequiredService<LookupDefinitionsService>(),
                nameof(Lookup) => definitionId == null ? sp.GetRequiredService<LookupsGenericService>() : (IFactWithIdService)sp.GetRequiredService<LookupsService>().SetDefinitionId(definitionId.Value),
                nameof(MarkupTemplate) => sp.GetRequiredService<MarkupTemplatesService>(),
                nameof(OutboxRecord) => sp.GetRequiredService<OutboxService>(),
                nameof(ReportDefinition) => sp.GetRequiredService<ReportDefinitionsService>(),
                nameof(ResourceDefinition) => sp.GetRequiredService<ResourceDefinitionsService>(),
                nameof(Resource) => definitionId == null ? sp.GetRequiredService<ResourcesGenericService>() : (IFactWithIdService)sp.GetRequiredService<ResourcesService>().SetDefinitionId(definitionId.Value),
                nameof(Role) => sp.GetRequiredService<RolesService>(),
                nameof(Unit) => sp.GetRequiredService<UnitsService>(),
                nameof(User) => sp.GetRequiredService<UsersService>(),
                nameof(DocumentDefinition) => sp.GetRequiredService<DocumentDefinitionsService>(),
                nameof(LineDefinition) => sp.GetRequiredService<LineDefinitionsService>(),

                _ => throw new UnknownCollectionException($"Collection {collection} does not have a known {nameof(IFactWithIdService)} implementation")
            };
        }

        /// <summary>
        /// Retrieves the <see cref="IFactServiceBase"/> implementation associated with a certain <see cref="Entity"/> type
        /// </summary>
        public static IFactGetByIdServiceBase FactGetByIdServiceByCollectionName(this IServiceProvider sp, string collection, int? definitionId = null)
        {
            return collection switch
            {
                nameof(VoucherBooklet) => sp.GetRequiredService<VoucherBookletsService>(),
                nameof(Account) => sp.GetRequiredService<AccountsService>(),
                nameof(AccountType) => sp.GetRequiredService<AccountTypesService>(),
                nameof(AdminUser) => sp.GetRequiredService<AdminUsersService>(),
                nameof(Agent) => sp.GetRequiredService<AgentsService>(),
                nameof(RelationDefinition) => sp.GetRequiredService<RelationDefinitionsService>(),
                nameof(Relation) => definitionId != null ? sp.GetRequiredService<RelationsService>().SetDefinitionId(definitionId.Value) : throw new RequiredDefinitionIdException($"Collection {nameof(Relation)} requires a definition Id"),
                nameof(CustodyDefinition) => sp.GetRequiredService<CustodyDefinitionsService>(),
                nameof(Custody) => definitionId != null ? sp.GetRequiredService<CustodiesService>().SetDefinitionId(definitionId.Value) : throw new RequiredDefinitionIdException($"Collection {nameof(Custody)} requires a definition Id"),
                nameof(Center) => sp.GetRequiredService<CentersService>(),
                nameof(Currency) => sp.GetRequiredService<CurrenciesService>(),
                nameof(AccountClassification) => sp.GetRequiredService<AccountClassificationsService>(),
                nameof(Document) => definitionId != null ? sp.GetRequiredService<DocumentsService>().SetDefinitionId(definitionId.Value) : throw new RequiredDefinitionIdException($"Collection {nameof(Document)} requires a definition Id"),
                nameof(EntryType) => sp.GetRequiredService<EntryTypesService>(),
                nameof(ExchangeRate) => sp.GetRequiredService<ExchangeRatesService>(),
                nameof(IdentityServerUser) => sp.GetRequiredService<IdentityServerUsersService>(),
                nameof(IfrsConcept) => sp.GetRequiredService<IfrsConceptsService>(),
                nameof(LookupDefinition) => sp.GetRequiredService<LookupDefinitionsService>(),
                nameof(Lookup) => definitionId != null ? sp.GetRequiredService<LookupsService>().SetDefinitionId(definitionId.Value) : throw new RequiredDefinitionIdException($"Collection {nameof(Lookup)} requires a definition Id"),
                nameof(MarkupTemplate) => sp.GetRequiredService<MarkupTemplatesService>(),
                nameof(ReportDefinition) => sp.GetRequiredService<ReportDefinitionsService>(),
                nameof(ResourceDefinition) => sp.GetRequiredService<ResourceDefinitionsService>(),
                nameof(Resource) => definitionId != null ? sp.GetRequiredService<ResourcesService>().SetDefinitionId(definitionId.Value) : throw new RequiredDefinitionIdException($"Collection {nameof(Resource)} requires a definition Id"),
                nameof(Role) => sp.GetRequiredService<RolesService>(),
                nameof(Unit) => sp.GetRequiredService<UnitsService>(),
                nameof(User) => sp.GetRequiredService<UsersService>(),
                nameof(DocumentDefinition) => sp.GetRequiredService<DocumentDefinitionsService>(),
                nameof(LineDefinition) => sp.GetRequiredService<LineDefinitionsService>(),

                _ => throw new UnknownCollectionException($"Bug: Entity type {collection} does not have a known {nameof(IFactGetByIdServiceBase)} implementation")
            };
        }
    }

    public class UnknownCollectionException : Exception
    {
        public UnknownCollectionException(string msg): base(msg)
        {
        }
    }

    public class RequiredDefinitionIdException : Exception
    {
        public RequiredDefinitionIdException(string msg): base(msg)
        {
        }
    }
}

