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

        public async Task<IList<DynamicRow>> GetAggregate(string collection, int? definitionId, string select, string filter, string having, string orderby, int? top, CancellationToken cancellation)
        {
            using var scope = _services.CreateScope();

            var service = FactServiceByCollectionName(collection, definitionId);
            var (data, _) = await service.GetAggregate(new GetAggregateArguments
            {
                Select = select,
                Filter = filter,
                Having = having,
                OrderBy = orderby,
                Top = top ?? int.MaxValue
            },
            cancellation);

            return data;
        }

        public async Task<IList<Entity>> GetEntities(string collection, int? definitionId, string select, string filter, string orderby, int? top, int? skip, CancellationToken cancellation)
        {
            using var scope = _services.CreateScope();

            var service = FactServiceByCollectionName(collection, definitionId);
            var (data, _, _) = await service.GetEntities(new GetArguments
            {
                Select = select,
                Filter = filter,
                OrderBy = orderby,
                Top = top ?? int.MaxValue,
                Skip = skip ?? 0
            },
            cancellation);

            return data;
        }

        public async Task<IList<EntityWithKey>> GetEntitiesByIds(string collection, int? definitionId, string select, IList ids, CancellationToken cancellation)
        {
            using var scope = _services.CreateScope();

            var service = FactWithIdServiceByCollectionName(collection, definitionId);
            var (data, _) = await service.GetByIds(ids.Cast<object>().ToList(), new SelectExpandArguments
            {
                Select = select
            },
            cancellation);

            return data;
        }

        public async Task<IList<EntityWithKey>> GetEntitiesByPropertyValues(string collection, int? definitionId, string propName, IEnumerable<object> values, CancellationToken cancellation)
        {
            using var scope = _services.CreateScope();

            var service = FactWithIdServiceByCollectionName(collection, definitionId);
            var (data, _) = await service.GetByPropertyValues(propName, values, new SelectExpandArguments
            {
                Select = "Id," + propName
            },
            cancellation);

            return data;
        }

        public async Task<EntityWithKey> GetEntityById(string collection, int? definitionId, string select, object id, CancellationToken cancellation)
        {
            using var scope = _services.CreateScope();

            var service = FactGetByIdServiceByCollectionName(collection, definitionId);
            var (data, _) = await service.GetById(id, new GetByIdArguments
            {
                Select = select
            },
            cancellation);

            return data;
        }

        public async Task<IList<DynamicRow>> GetFact(string collection, int? definitionId, string select, string filter, string orderby, int? top, int? skip, CancellationToken cancellation)
        {
            using var scope = _services.CreateScope();

            var service = FactServiceByCollectionName(collection, definitionId);
            var (data, _) = await service.GetFact(new GetArguments
            {
                Select = select,
                Filter = filter,
                OrderBy = orderby,
                Top = top ?? int.MaxValue,
                Skip = skip ?? 0
            },
            cancellation);

            return data.ToList();
        }

        #region Helpers

        /// <summary>
        /// Retrieves the <see cref="IFactService"/> implementation associated with a certain <see cref="Entity"/> type.
        /// </summary>
        private IFactService FactServiceByCollectionName(string collection, int? definitionId = null)
        {
            return collection switch
            {
                nameof(Account) => _services.GetRequiredService<AccountsService>(),
                nameof(AccountType) => _services.GetRequiredService<AccountTypesService>(),
                nameof(AdminUser) => _services.GetRequiredService<AdminUsersService>(),
                nameof(Agent) => _services.GetRequiredService<AgentsService>(),
                nameof(RelationDefinition) => _services.GetRequiredService<RelationDefinitionsService>(),
                nameof(Relation) => definitionId == null ? _services.GetRequiredService<RelationsGenericService>() : _services.GetRequiredService<RelationsService>().SetDefinitionId(definitionId.Value),
                nameof(Center) => _services.GetRequiredService<CentersService>(),
                nameof(Currency) => _services.GetRequiredService<CurrenciesService>(),
                nameof(AccountClassification) => _services.GetRequiredService<AccountClassificationsService>(),
                nameof(DetailsEntry) => _services.GetRequiredService<DetailsEntriesService>(),
                nameof(Document) => definitionId == null ? _services.GetRequiredService<DocumentsGenericService>() : _services.GetRequiredService<DocumentsService>().SetDefinitionId(definitionId.Value),
                nameof(EntryType) => _services.GetRequiredService<EntryTypesService>(),
                nameof(ExchangeRate) => _services.GetRequiredService<ExchangeRatesService>(),
                nameof(IfrsConcept) => _services.GetRequiredService<IfrsConceptsService>(),
                nameof(InboxRecord) => _services.GetRequiredService<InboxService>(),
                nameof(LookupDefinition) => _services.GetRequiredService<LookupDefinitionsService>(),
                nameof(Lookup) => definitionId == null ? _services.GetRequiredService<LookupsGenericService>() : _services.GetRequiredService<LookupsService>().SetDefinitionId(definitionId.Value),
                nameof(MarkupTemplate) => _services.GetRequiredService<MarkupTemplatesService>(),
                nameof(OutboxRecord) => _services.GetRequiredService<OutboxService>(),
                nameof(ReportDefinition) => _services.GetRequiredService<ReportDefinitionsService>(),
                nameof(DashboardDefinition) => _services.GetRequiredService<DashboardDefinitionsService>(),
                nameof(ResourceDefinition) => _services.GetRequiredService<ResourceDefinitionsService>(),
                nameof(Resource) => definitionId == null ? _services.GetRequiredService<ResourcesGenericService>() : _services.GetRequiredService<ResourcesService>().SetDefinitionId(definitionId.Value),
                nameof(Role) => _services.GetRequiredService<RolesService>(),
                nameof(Unit) => _services.GetRequiredService<UnitsService>(),
                nameof(User) => _services.GetRequiredService<UsersService>(),
                nameof(DocumentDefinition) => _services.GetRequiredService<DocumentDefinitionsService>(),
                nameof(LineDefinition) => _services.GetRequiredService<LineDefinitionsService>(),
                nameof(EmailForQuery) => _services.GetRequiredService<EmailsService>(),
                nameof(SmsMessageForQuery) => _services.GetRequiredService<SmsMessagesService>(),

                _ => throw new UnknownCollectionException($"Collection {collection} does not have a known {nameof(IFactService)} implementation")
            };
        }

        /// <summary>
        /// Retrieves the <see cref="IFactWithIdService"/> implementation associated with a certain <see cref="Entity"/> type.
        /// </summary>
        private IFactWithIdService FactWithIdServiceByCollectionName(string collection, int? definitionId = null)
        {
            return collection switch
            {
                nameof(Account) => _services.GetRequiredService<AccountsService>(),
                nameof(AccountType) => _services.GetRequiredService<AccountTypesService>(),
                nameof(AdminUser) => _services.GetRequiredService<AdminUsersService>(),
                nameof(Agent) => _services.GetRequiredService<AgentsService>(),
                nameof(RelationDefinition) => _services.GetRequiredService<RelationDefinitionsService>(),
                nameof(Relation) => definitionId == null ? _services.GetRequiredService<RelationsGenericService>() : _services.GetRequiredService<RelationsService>().SetDefinitionId(definitionId.Value),
                nameof(Center) => _services.GetRequiredService<CentersService>(),
                nameof(Currency) => _services.GetRequiredService<CurrenciesService>(),
                nameof(AccountClassification) => _services.GetRequiredService<AccountClassificationsService>(),
                nameof(DetailsEntry) => _services.GetRequiredService<DetailsEntriesService>(),
                nameof(Document) => definitionId == null ? _services.GetRequiredService<DocumentsGenericService>() : _services.GetRequiredService<DocumentsService>().SetDefinitionId(definitionId.Value),
                nameof(EntryType) => _services.GetRequiredService<EntryTypesService>(),
                nameof(ExchangeRate) => _services.GetRequiredService<ExchangeRatesService>(),
                nameof(IfrsConcept) => _services.GetRequiredService<IfrsConceptsService>(),
                nameof(InboxRecord) => _services.GetRequiredService<InboxService>(),
                nameof(LookupDefinition) => _services.GetRequiredService<LookupDefinitionsService>(),
                nameof(Lookup) => definitionId == null ? _services.GetRequiredService<LookupsGenericService>() : _services.GetRequiredService<LookupsService>().SetDefinitionId(definitionId.Value),
                nameof(MarkupTemplate) => _services.GetRequiredService<MarkupTemplatesService>(),
                nameof(OutboxRecord) => _services.GetRequiredService<OutboxService>(),
                nameof(ReportDefinition) => _services.GetRequiredService<ReportDefinitionsService>(),
                nameof(DashboardDefinition) => _services.GetRequiredService<DashboardDefinitionsService>(),
                nameof(ResourceDefinition) => _services.GetRequiredService<ResourceDefinitionsService>(),
                nameof(Resource) => definitionId == null ? _services.GetRequiredService<ResourcesGenericService>() : _services.GetRequiredService<ResourcesService>().SetDefinitionId(definitionId.Value),
                nameof(Role) => _services.GetRequiredService<RolesService>(),
                nameof(Unit) => _services.GetRequiredService<UnitsService>(),
                nameof(User) => _services.GetRequiredService<UsersService>(),
                nameof(DocumentDefinition) => _services.GetRequiredService<DocumentDefinitionsService>(),
                nameof(LineDefinition) => _services.GetRequiredService<LineDefinitionsService>(),
                nameof(EmailForQuery) => _services.GetRequiredService<EmailsService>(),
                nameof(SmsMessageForQuery) => _services.GetRequiredService<SmsMessagesService>(),

                _ => throw new UnknownCollectionException($"Collection {collection} does not have a known {nameof(IFactWithIdService)} implementation")
            };
        }

        /// <summary>
        /// Retrieves the <see cref="IFactGetByIdServiceBase"/> implementation associated with a certain <see cref="Entity"/> type.
        /// </summary>
        private IFactGetByIdServiceBase FactGetByIdServiceByCollectionName(string collection, int? definitionId = null)
        {
            return collection switch
            {
                nameof(Account) => _services.GetRequiredService<AccountsService>(),
                nameof(AccountType) => _services.GetRequiredService<AccountTypesService>(),
                nameof(AdminUser) => _services.GetRequiredService<AdminUsersService>(),
                nameof(Agent) => _services.GetRequiredService<AgentsService>(),
                nameof(RelationDefinition) => _services.GetRequiredService<RelationDefinitionsService>(),
                nameof(Relation) => definitionId != null ? _services.GetRequiredService<RelationsService>().SetDefinitionId(definitionId.Value) : throw new RequiredDefinitionIdException($"Collection {nameof(Relation)} requires a definition Id"),
                nameof(Center) => _services.GetRequiredService<CentersService>(),
                nameof(Currency) => _services.GetRequiredService<CurrenciesService>(),
                nameof(AccountClassification) => _services.GetRequiredService<AccountClassificationsService>(),
                nameof(Document) => definitionId != null ? _services.GetRequiredService<DocumentsService>().SetDefinitionId(definitionId.Value) : throw new RequiredDefinitionIdException($"Collection {nameof(Document)} requires a definition Id"),
                nameof(EntryType) => _services.GetRequiredService<EntryTypesService>(),
                nameof(ExchangeRate) => _services.GetRequiredService<ExchangeRatesService>(),
                nameof(IfrsConcept) => _services.GetRequiredService<IfrsConceptsService>(),
                nameof(LookupDefinition) => _services.GetRequiredService<LookupDefinitionsService>(),
                nameof(Lookup) => definitionId != null ? _services.GetRequiredService<LookupsService>().SetDefinitionId(definitionId.Value) : throw new RequiredDefinitionIdException($"Collection {nameof(Lookup)} requires a definition Id"),
                nameof(MarkupTemplate) => _services.GetRequiredService<MarkupTemplatesService>(),
                nameof(ReportDefinition) => _services.GetRequiredService<ReportDefinitionsService>(),
                nameof(DashboardDefinition) => _services.GetRequiredService<DashboardDefinitionsService>(),
                nameof(ResourceDefinition) => _services.GetRequiredService<ResourceDefinitionsService>(),
                nameof(Resource) => definitionId != null ? _services.GetRequiredService<ResourcesService>().SetDefinitionId(definitionId.Value) : throw new RequiredDefinitionIdException($"Collection {nameof(Resource)} requires a definition Id"),
                nameof(Role) => _services.GetRequiredService<RolesService>(),
                nameof(Unit) => _services.GetRequiredService<UnitsService>(),
                nameof(User) => _services.GetRequiredService<UsersService>(),
                nameof(DocumentDefinition) => _services.GetRequiredService<DocumentDefinitionsService>(),
                nameof(LineDefinition) => _services.GetRequiredService<LineDefinitionsService>(),
                nameof(EmailForQuery) => _services.GetRequiredService<EmailsService>(),
                nameof(SmsMessageForQuery) => _services.GetRequiredService<SmsMessagesService>(),

                _ => throw new UnknownCollectionException($"Bug: Entity type {collection} does not have a known {nameof(IFactGetByIdServiceBase)} implementation")
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
