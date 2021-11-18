using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api.Base;
using Tellma.Api.Dto;
using Tellma.Api.ImportExport;
using Tellma.Api.Templating;
using Tellma.Model.Admin;
using Tellma.Model.Application;
using Tellma.Model.Common;

namespace Tellma.Api
{
    /// <summary>
    /// Implementation of <see cref="IApiClientForTemplating"/> and <see cref="IApiClientForImport"/> 
    /// that provides access to application and admin API services.
    /// </summary>
    public class ApiClient : IApiClientForTemplating, IApiClientForImport
    {
        private readonly IServiceProvider _services;

        public ApiClient(IServiceProvider services)
        {
            _services = services;
        }

        public async Task<IReadOnlyList<DynamicRow>> GetAggregate(string collection, int? definitionId, string select, string filter, string having, string orderby, int? top, CancellationToken cancellation)
        {
            using var scope = _services.CreateScope();

            var service = FactServiceByCollectionName(scope.ServiceProvider, collection, definitionId);
            var result = await service.GetAggregate(new GetAggregateArguments
            {
                Select = select,
                Filter = filter,
                Having = having,
                OrderBy = orderby,
                Top = top ?? int.MaxValue
            },
            cancellation);

            return result.Data;
        }

        public async Task<IReadOnlyList<Entity>> GetEntities(string collection, int? definitionId, string select, string filter, string orderby, int? top, int? skip, CancellationToken cancellation)
        {
            using var scope = _services.CreateScope();

            var service = FactServiceByCollectionName(scope.ServiceProvider, collection, definitionId);
            var result = await service.GetEntities(new GetArguments
            {
                Select = select,
                Filter = filter,
                OrderBy = orderby,
                Top = top ?? int.MaxValue,
                Skip = skip ?? 0
            },
            cancellation);

            return result.Data;
        }

        public async Task<IReadOnlyList<EntityWithKey>> GetEntitiesByIds(string collection, int? definitionId, string select, IList ids, CancellationToken cancellation)
        {
            using var scope = _services.CreateScope();

            var service = FactWithIdServiceByCollectionName(scope.ServiceProvider, collection, definitionId);
            var result = await service.GetByIds(ids.Cast<object>().ToList(), new SelectExpandArguments
            {
                Select = select
            },
            cancellation);

            return result.Data;
        }

        public async Task<IReadOnlyList<EntityWithKey>> GetEntitiesByPropertyValues(string collection, int? definitionId, string propName, IEnumerable<object> values, CancellationToken cancellation)
        {
            using var scope = _services.CreateScope();

            var service = FactWithIdServiceByCollectionName(scope.ServiceProvider, collection, definitionId);
            var result = await service.GetByPropertyValues(propName, values, new SelectExpandArguments
            {
                Select = "Id," + propName
            },
            cancellation);

            return result.Data;
        }

        public async Task<EntityWithKey> GetEntityById(string collection, int? definitionId, string select, object id, CancellationToken cancellation)
        {
            using var scope = _services.CreateScope();

            var service = FactGetByIdServiceByCollectionName(scope.ServiceProvider, collection, definitionId);
            var result = await service.GetById(id, new GetByIdArguments
            {
                Select = select
            },
            cancellation);

            return result.Entity;
        }

        public async Task<IReadOnlyList<DynamicRow>> GetFact(string collection, int? definitionId, string select, string filter, string orderby, int? top, int? skip, CancellationToken cancellation)
        {
            using var scope = _services.CreateScope();

            var service = FactServiceByCollectionName(scope.ServiceProvider, collection, definitionId);
            var result = await service.GetFact(new FactArguments
            {
                Select = select,
                Filter = filter,
                OrderBy = orderby,
                Top = top ?? int.MaxValue,
                Skip = skip ?? 0
            },
            cancellation);

            return result.Data;
        }

        #region Helpers

        /// <summary>
        /// Retrieves the <see cref="IFactService"/> implementation associated with a certain <see cref="Entity"/> type.
        /// </summary>
        private static IFactService FactServiceByCollectionName(IServiceProvider provider, string collection, int? definitionId = null)
        {
            return collection switch
            {
                nameof(Account) => provider.GetRequiredService<AccountsService>(),
                nameof(AccountType) => provider.GetRequiredService<AccountTypesService>(),
                nameof(AdminUser) => provider.GetRequiredService<AdminUsersService>(),
                nameof(AgentDefinition) => provider.GetRequiredService<AgentDefinitionsService>(),
                nameof(Agent) => definitionId == null ? provider.GetRequiredService<AgentsGenericService>() : provider.GetRequiredService<AgentsService>().SetDefinitionId(definitionId.Value),
                nameof(Center) => provider.GetRequiredService<CentersService>(),
                nameof(Currency) => provider.GetRequiredService<CurrenciesService>(),
                nameof(AccountClassification) => provider.GetRequiredService<AccountClassificationsService>(),
                nameof(DetailsEntry) => provider.GetRequiredService<DetailsEntriesService>(),
                nameof(Document) => definitionId == null ? provider.GetRequiredService<DocumentsGenericService>() : provider.GetRequiredService<DocumentsService>().SetDefinitionId(definitionId.Value),
                nameof(EntryType) => provider.GetRequiredService<EntryTypesService>(),
                nameof(ExchangeRate) => provider.GetRequiredService<ExchangeRatesService>(),
                nameof(IdentityServerClient) => provider.GetRequiredService<IdentityServerClientsService>(),
                nameof(IfrsConcept) => provider.GetRequiredService<IfrsConceptsService>(),
                nameof(InboxRecord) => provider.GetRequiredService<InboxService>(),
                nameof(LookupDefinition) => provider.GetRequiredService<LookupDefinitionsService>(),
                nameof(Lookup) => definitionId == null ? provider.GetRequiredService<LookupsGenericService>() : provider.GetRequiredService<LookupsService>().SetDefinitionId(definitionId.Value),
                nameof(NotificationCommand) => provider.GetRequiredService<NotificationCommandsService>(),
                nameof(NotificationTemplate) => provider.GetRequiredService<NotificationTemplatesService>(),
                nameof(PrintingTemplate) => provider.GetRequiredService<PrintingTemplatesService>(),
                nameof(OutboxRecord) => provider.GetRequiredService<OutboxService>(),
                nameof(ReportDefinition) => provider.GetRequiredService<ReportDefinitionsService>(),
                nameof(DashboardDefinition) => provider.GetRequiredService<DashboardDefinitionsService>(),
                nameof(ResourceDefinition) => provider.GetRequiredService<ResourceDefinitionsService>(),
                nameof(Resource) => definitionId == null ? provider.GetRequiredService<ResourcesGenericService>() : provider.GetRequiredService<ResourcesService>().SetDefinitionId(definitionId.Value),
                nameof(Role) => provider.GetRequiredService<RolesService>(),
                nameof(Unit) => provider.GetRequiredService<UnitsService>(),
                nameof(User) => provider.GetRequiredService<UsersService>(),
                nameof(DocumentDefinition) => provider.GetRequiredService<DocumentDefinitionsService>(),
                nameof(LineDefinition) => provider.GetRequiredService<LineDefinitionsService>(),
                nameof(EmailForQuery) => provider.GetRequiredService<EmailsService>(),
                nameof(SmsMessageForQuery) => provider.GetRequiredService<SmsMessagesService>(),

                _ => throw new UnknownCollectionException($"Collection {collection} does not have a known {nameof(IFactService)} implementation.")
            };
        }

        /// <summary>
        /// Retrieves the <see cref="IFactWithIdService"/> implementation associated with a certain <see cref="Entity"/> type.
        /// </summary>
        private static IFactWithIdService FactWithIdServiceByCollectionName(IServiceProvider provider, string collection, int? definitionId = null)
        {
            return collection switch
            {
                nameof(Account) => provider.GetRequiredService<AccountsService>(),
                nameof(AccountType) => provider.GetRequiredService<AccountTypesService>(),
                nameof(AdminUser) => provider.GetRequiredService<AdminUsersService>(),
                nameof(AgentDefinition) => provider.GetRequiredService<AgentDefinitionsService>(),
                nameof(Agent) => definitionId == null ? provider.GetRequiredService<AgentsGenericService>() : provider.GetRequiredService<AgentsService>().SetDefinitionId(definitionId.Value),
                nameof(Center) => provider.GetRequiredService<CentersService>(),
                nameof(Currency) => provider.GetRequiredService<CurrenciesService>(),
                nameof(AccountClassification) => provider.GetRequiredService<AccountClassificationsService>(),
                nameof(DetailsEntry) => provider.GetRequiredService<DetailsEntriesService>(),
                nameof(Document) => definitionId == null ? provider.GetRequiredService<DocumentsGenericService>() : provider.GetRequiredService<DocumentsService>().SetDefinitionId(definitionId.Value),
                nameof(EntryType) => provider.GetRequiredService<EntryTypesService>(),
                nameof(ExchangeRate) => provider.GetRequiredService<ExchangeRatesService>(),
                nameof(IdentityServerClient) => provider.GetRequiredService<IdentityServerClientsService>(),
                nameof(IfrsConcept) => provider.GetRequiredService<IfrsConceptsService>(),
                nameof(InboxRecord) => provider.GetRequiredService<InboxService>(),
                nameof(LookupDefinition) => provider.GetRequiredService<LookupDefinitionsService>(),
                nameof(Lookup) => definitionId == null ? provider.GetRequiredService<LookupsGenericService>() : provider.GetRequiredService<LookupsService>().SetDefinitionId(definitionId.Value),
                nameof(NotificationCommand) => provider.GetRequiredService<NotificationCommandsService>(),
                nameof(NotificationTemplate) => provider.GetRequiredService<NotificationTemplatesService>(),
                nameof(PrintingTemplate) => provider.GetRequiredService<PrintingTemplatesService>(),
                nameof(OutboxRecord) => provider.GetRequiredService<OutboxService>(),
                nameof(ReportDefinition) => provider.GetRequiredService<ReportDefinitionsService>(),
                nameof(DashboardDefinition) => provider.GetRequiredService<DashboardDefinitionsService>(),
                nameof(ResourceDefinition) => provider.GetRequiredService<ResourceDefinitionsService>(),
                nameof(Resource) => definitionId == null ? provider.GetRequiredService<ResourcesGenericService>() : provider.GetRequiredService<ResourcesService>().SetDefinitionId(definitionId.Value),
                nameof(Role) => provider.GetRequiredService<RolesService>(),
                nameof(Unit) => provider.GetRequiredService<UnitsService>(),
                nameof(User) => provider.GetRequiredService<UsersService>(),
                nameof(DocumentDefinition) => provider.GetRequiredService<DocumentDefinitionsService>(),
                nameof(LineDefinition) => provider.GetRequiredService<LineDefinitionsService>(),
                nameof(EmailForQuery) => provider.GetRequiredService<EmailsService>(),
                nameof(SmsMessageForQuery) => provider.GetRequiredService<SmsMessagesService>(),

                _ => throw new UnknownCollectionException($"Collection {collection} does not have a known {nameof(IFactWithIdService)} implementation.")
            };
        }

        /// <summary>
        /// Retrieves the <see cref="IFactGetByIdServiceBase"/> implementation associated with a certain <see cref="Entity"/> type.
        /// </summary>
        private static IFactGetByIdServiceBase FactGetByIdServiceByCollectionName(IServiceProvider provider, string collection, int? definitionId = null)
        {
            return collection switch
            {
                nameof(Account) => provider.GetRequiredService<AccountsService>(),
                nameof(AccountType) => provider.GetRequiredService<AccountTypesService>(),
                nameof(AdminUser) => provider.GetRequiredService<AdminUsersService>(),
                nameof(AgentDefinition) => provider.GetRequiredService<AgentDefinitionsService>(),
                nameof(Agent) => definitionId != null ? provider.GetRequiredService<AgentsService>().SetDefinitionId(definitionId.Value) : throw new RequiredDefinitionIdException($"Collection {nameof(Agent)} requires a definition Id"),
                nameof(Center) => provider.GetRequiredService<CentersService>(),
                nameof(Currency) => provider.GetRequiredService<CurrenciesService>(),
                nameof(AccountClassification) => provider.GetRequiredService<AccountClassificationsService>(),
                nameof(Document) => definitionId != null ? provider.GetRequiredService<DocumentsService>().SetDefinitionId(definitionId.Value) : throw new RequiredDefinitionIdException($"Collection {nameof(Document)} requires a definition Id"),
                nameof(EntryType) => provider.GetRequiredService<EntryTypesService>(),
                nameof(ExchangeRate) => provider.GetRequiredService<ExchangeRatesService>(),
                nameof(IdentityServerClient) => provider.GetRequiredService<IdentityServerClientsService>(),
                nameof(IfrsConcept) => provider.GetRequiredService<IfrsConceptsService>(),
                nameof(LookupDefinition) => provider.GetRequiredService<LookupDefinitionsService>(),
                nameof(Lookup) => definitionId != null ? provider.GetRequiredService<LookupsService>().SetDefinitionId(definitionId.Value) : throw new RequiredDefinitionIdException($"Collection {nameof(Lookup)} requires a definition Id"),
                nameof(NotificationCommand) => provider.GetRequiredService<NotificationCommandsService>(),
                nameof(NotificationTemplate) => provider.GetRequiredService<NotificationTemplatesService>(),
                nameof(PrintingTemplate) => provider.GetRequiredService<PrintingTemplatesService>(),
                nameof(ReportDefinition) => provider.GetRequiredService<ReportDefinitionsService>(),
                nameof(DashboardDefinition) => provider.GetRequiredService<DashboardDefinitionsService>(),
                nameof(ResourceDefinition) => provider.GetRequiredService<ResourceDefinitionsService>(),
                nameof(Resource) => definitionId != null ? provider.GetRequiredService<ResourcesService>().SetDefinitionId(definitionId.Value) : throw new RequiredDefinitionIdException($"Collection {nameof(Resource)} requires a definition Id"),
                nameof(Role) => provider.GetRequiredService<RolesService>(),
                nameof(Unit) => provider.GetRequiredService<UnitsService>(),
                nameof(User) => provider.GetRequiredService<UsersService>(),
                nameof(DocumentDefinition) => provider.GetRequiredService<DocumentDefinitionsService>(),
                nameof(LineDefinition) => provider.GetRequiredService<LineDefinitionsService>(),
                nameof(EmailForQuery) => provider.GetRequiredService<EmailsService>(),
                nameof(SmsMessageForQuery) => provider.GetRequiredService<SmsMessagesService>(),

                _ => throw new UnknownCollectionException($"Bug: Entity type {collection} does not have a known {nameof(IFactGetByIdServiceBase)} implementation.")
            };
        }

        #endregion
    }

    public class UnknownCollectionException : Exception
    {
        public UnknownCollectionException(string msg) : base(msg)
        {
        }
    }

    public class RequiredDefinitionIdException : Exception
    {
        public RequiredDefinitionIdException(string msg) : base(msg)
        {
        }
    }
}
