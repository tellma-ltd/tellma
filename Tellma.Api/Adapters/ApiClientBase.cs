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
    /// Base class for generic API clients that provide utility methods.
    /// </summary>
    public abstract class ApiClientBase
    {
        #region Helpers

        /// <summary>
        /// Retrieves the <see cref="IFactService"/> implementation associated with a certain <see cref="Entity"/> type.
        /// </summary>
        protected virtual IFactService FactServiceByCollectionName(IServiceProvider services, string collection, int? definitionId = null)
        {
            return collection switch
            {
                nameof(Account) => services.GetRequiredService<AccountsService>(),
                nameof(AccountType) => services.GetRequiredService<AccountTypesService>(),
                nameof(AdminUser) => services.GetRequiredService<AdminUsersService>(),
                nameof(AgentDefinition) => services.GetRequiredService<AgentDefinitionsService>(),
                nameof(Agent) => definitionId == null ? services.GetRequiredService<AgentsGenericService>() : services.GetRequiredService<AgentsService>().SetDefinitionId(definitionId.Value),
                nameof(Center) => services.GetRequiredService<CentersService>(),
                nameof(Currency) => services.GetRequiredService<CurrenciesService>(),
                nameof(AccountClassification) => services.GetRequiredService<AccountClassificationsService>(),
                nameof(DetailsEntry) => services.GetRequiredService<DetailsEntriesService>(),
                nameof(Document) => definitionId == null ? services.GetRequiredService<DocumentsGenericService>() : services.GetRequiredService<DocumentsService>().SetDefinitionId(definitionId.Value),
                nameof(EntryType) => services.GetRequiredService<EntryTypesService>(),
                nameof(ExchangeRate) => services.GetRequiredService<ExchangeRatesService>(),
                nameof(IdentityServerClient) => services.GetRequiredService<IdentityServerClientsService>(),
                nameof(IfrsConcept) => services.GetRequiredService<IfrsConceptsService>(),
                nameof(InboxRecord) => services.GetRequiredService<InboxService>(),
                nameof(LookupDefinition) => services.GetRequiredService<LookupDefinitionsService>(),
                nameof(Lookup) => definitionId == null ? services.GetRequiredService<LookupsGenericService>() : services.GetRequiredService<LookupsService>().SetDefinitionId(definitionId.Value),
                nameof(MessageCommand) => services.GetRequiredService<MessageCommandsService>(),
                nameof(MessageTemplate) => services.GetRequiredService<MessageTemplatesService>(),
                nameof(EmailCommand) => services.GetRequiredService<EmailCommandsService>(),
                nameof(EmailTemplate) => services.GetRequiredService<EmailTemplatesService>(),
                nameof(PrintingTemplate) => services.GetRequiredService<PrintingTemplatesService>(),
                nameof(OutboxRecord) => services.GetRequiredService<OutboxService>(),
                nameof(ReportDefinition) => services.GetRequiredService<ReportDefinitionsService>(),
                nameof(DashboardDefinition) => services.GetRequiredService<DashboardDefinitionsService>(),
                nameof(ResourceDefinition) => services.GetRequiredService<ResourceDefinitionsService>(),
                nameof(Resource) => definitionId == null ? services.GetRequiredService<ResourcesGenericService>() : services.GetRequiredService<ResourcesService>().SetDefinitionId(definitionId.Value),
                nameof(Role) => services.GetRequiredService<RolesService>(),
                nameof(Unit) => services.GetRequiredService<UnitsService>(),
                nameof(User) => services.GetRequiredService<UsersService>(),
                nameof(DocumentDefinition) => services.GetRequiredService<DocumentDefinitionsService>(),
                nameof(LineDefinition) => services.GetRequiredService<LineDefinitionsService>(),
                nameof(EmailForQuery) => services.GetRequiredService<EmailsService>(),
                nameof(MessageForQuery) => services.GetRequiredService<MessagesService>(),

                _ => throw new UnknownCollectionException($"Collection {collection} does not have a known {nameof(IFactService)} implementation.")
            };
        }

        /// <summary>
        /// Retrieves the <see cref="IFactWithIdService"/> implementation associated with a certain <see cref="Entity"/> type.
        /// </summary>
        protected virtual IFactWithIdService FactWithIdServiceByCollectionName(IServiceProvider services, string collection, int? definitionId = null)
        {
            return collection switch
            {
                nameof(Account) => services.GetRequiredService<AccountsService>(),
                nameof(AccountType) => services.GetRequiredService<AccountTypesService>(),
                nameof(AdminUser) => services.GetRequiredService<AdminUsersService>(),
                nameof(AgentDefinition) => services.GetRequiredService<AgentDefinitionsService>(),
                nameof(Agent) => definitionId == null ? services.GetRequiredService<AgentsGenericService>() : services.GetRequiredService<AgentsService>().SetDefinitionId(definitionId.Value),
                nameof(Center) => services.GetRequiredService<CentersService>(),
                nameof(Currency) => services.GetRequiredService<CurrenciesService>(),
                nameof(AccountClassification) => services.GetRequiredService<AccountClassificationsService>(),
                nameof(DetailsEntry) => services.GetRequiredService<DetailsEntriesService>(),
                nameof(Document) => definitionId == null ? services.GetRequiredService<DocumentsGenericService>() : services.GetRequiredService<DocumentsService>().SetDefinitionId(definitionId.Value),
                nameof(EntryType) => services.GetRequiredService<EntryTypesService>(),
                nameof(ExchangeRate) => services.GetRequiredService<ExchangeRatesService>(),
                nameof(IdentityServerClient) => services.GetRequiredService<IdentityServerClientsService>(),
                nameof(IfrsConcept) => services.GetRequiredService<IfrsConceptsService>(),
                nameof(InboxRecord) => services.GetRequiredService<InboxService>(),
                nameof(LookupDefinition) => services.GetRequiredService<LookupDefinitionsService>(),
                nameof(Lookup) => definitionId == null ? services.GetRequiredService<LookupsGenericService>() : services.GetRequiredService<LookupsService>().SetDefinitionId(definitionId.Value),
                nameof(MessageCommand) => services.GetRequiredService<MessageCommandsService>(),
                nameof(MessageTemplate) => services.GetRequiredService<MessageTemplatesService>(),
                nameof(EmailCommand) => services.GetRequiredService<EmailCommandsService>(),
                nameof(EmailTemplate) => services.GetRequiredService<EmailTemplatesService>(),
                nameof(PrintingTemplate) => services.GetRequiredService<PrintingTemplatesService>(),
                nameof(OutboxRecord) => services.GetRequiredService<OutboxService>(),
                nameof(ReportDefinition) => services.GetRequiredService<ReportDefinitionsService>(),
                nameof(DashboardDefinition) => services.GetRequiredService<DashboardDefinitionsService>(),
                nameof(ResourceDefinition) => services.GetRequiredService<ResourceDefinitionsService>(),
                nameof(Resource) => definitionId == null ? services.GetRequiredService<ResourcesGenericService>() : services.GetRequiredService<ResourcesService>().SetDefinitionId(definitionId.Value),
                nameof(Role) => services.GetRequiredService<RolesService>(),
                nameof(Unit) => services.GetRequiredService<UnitsService>(),
                nameof(User) => services.GetRequiredService<UsersService>(),
                nameof(DocumentDefinition) => services.GetRequiredService<DocumentDefinitionsService>(),
                nameof(LineDefinition) => services.GetRequiredService<LineDefinitionsService>(),
                nameof(EmailForQuery) => services.GetRequiredService<EmailsService>(),
                nameof(MessageForQuery) => services.GetRequiredService<MessagesService>(),

                _ => throw new UnknownCollectionException($"Collection {collection} does not have a known {nameof(IFactWithIdService)} implementation.")
            };
        }

        /// <summary>
        /// Retrieves the <see cref="IFactGetByIdServiceBase"/> implementation associated with a certain <see cref="Entity"/> type.
        /// </summary>
        protected virtual IFactGetByIdServiceBase FactGetByIdServiceByCollectionName(IServiceProvider services, string collection, int? definitionId = null)
        {
            return collection switch
            {
                nameof(Account) => services.GetRequiredService<AccountsService>(),
                nameof(AccountType) => services.GetRequiredService<AccountTypesService>(),
                nameof(AdminUser) => services.GetRequiredService<AdminUsersService>(),
                nameof(AgentDefinition) => services.GetRequiredService<AgentDefinitionsService>(),
                nameof(Agent) => definitionId != null ? services.GetRequiredService<AgentsService>().SetDefinitionId(definitionId.Value) : throw new RequiredDefinitionIdException($"Collection {nameof(Agent)} requires a definition Id"),
                nameof(Center) => services.GetRequiredService<CentersService>(),
                nameof(Currency) => services.GetRequiredService<CurrenciesService>(),
                nameof(AccountClassification) => services.GetRequiredService<AccountClassificationsService>(),
                nameof(Document) => definitionId != null ? services.GetRequiredService<DocumentsService>().SetDefinitionId(definitionId.Value) : throw new RequiredDefinitionIdException($"Collection {nameof(Document)} requires a definition Id"),
                nameof(EntryType) => services.GetRequiredService<EntryTypesService>(),
                nameof(ExchangeRate) => services.GetRequiredService<ExchangeRatesService>(),
                nameof(IdentityServerClient) => services.GetRequiredService<IdentityServerClientsService>(),
                nameof(IfrsConcept) => services.GetRequiredService<IfrsConceptsService>(),
                nameof(LookupDefinition) => services.GetRequiredService<LookupDefinitionsService>(),
                nameof(Lookup) => definitionId != null ? services.GetRequiredService<LookupsService>().SetDefinitionId(definitionId.Value) : throw new RequiredDefinitionIdException($"Collection {nameof(Lookup)} requires a definition Id"),
                nameof(MessageCommand) => services.GetRequiredService<MessageCommandsService>(),
                nameof(MessageTemplate) => services.GetRequiredService<MessageTemplatesService>(),
                nameof(EmailCommand) => services.GetRequiredService<EmailCommandsService>(),
                nameof(EmailTemplate) => services.GetRequiredService<EmailTemplatesService>(),
                nameof(PrintingTemplate) => services.GetRequiredService<PrintingTemplatesService>(),
                nameof(ReportDefinition) => services.GetRequiredService<ReportDefinitionsService>(),
                nameof(DashboardDefinition) => services.GetRequiredService<DashboardDefinitionsService>(),
                nameof(ResourceDefinition) => services.GetRequiredService<ResourceDefinitionsService>(),
                nameof(Resource) => definitionId != null ? services.GetRequiredService<ResourcesService>().SetDefinitionId(definitionId.Value) : throw new RequiredDefinitionIdException($"Collection {nameof(Resource)} requires a definition Id"),
                nameof(Role) => services.GetRequiredService<RolesService>(),
                nameof(Unit) => services.GetRequiredService<UnitsService>(),
                nameof(User) => services.GetRequiredService<UsersService>(),
                nameof(DocumentDefinition) => services.GetRequiredService<DocumentDefinitionsService>(),
                nameof(LineDefinition) => services.GetRequiredService<LineDefinitionsService>(),
                nameof(EmailForQuery) => services.GetRequiredService<EmailsService>(),
                nameof(MessageForQuery) => services.GetRequiredService<MessagesService>(),

                _ => throw new UnknownCollectionException($"Bug: Entity type {collection} does not have a known {nameof(IFactGetByIdServiceBase)} implementation.")
            };
        }

        /// <summary>
        /// Retrieves the <see cref="IImageGetter"/> implementation associated with a certain <see cref="Entity"/> type.
        /// </summary>
        protected virtual IImageGetter ImageGetterByCollectionName(IServiceProvider services, string collection, int? definitionId = null)
        {
            return collection switch
            {
                nameof(Agent) => definitionId != null ? services.GetRequiredService<AgentsService>().SetDefinitionId(definitionId.Value) : throw new RequiredDefinitionIdException($"Collection {nameof(Agent)} requires a definition Id"),
                nameof(Resource) => definitionId != null ? services.GetRequiredService<ResourcesService>().SetDefinitionId(definitionId.Value) : throw new RequiredDefinitionIdException($"Collection {nameof(Resource)} requires a definition Id"),
                nameof(User) => services.GetRequiredService<UsersService>(),

                _ => throw new UnknownCollectionException($"Bug: Entity type {collection} does not have a known {nameof(IImageGetter)} implementation.")
            };
        }

        /// <summary>
        /// Retrieves the <see cref="DocumentsService"/> associated with a given <paramref name="definitionId"/>.
        /// </summary>
        protected virtual DocumentsService DocumentService(IServiceProvider services, int definitionId)
        {
            var result = services.GetRequiredService<DocumentsService>();
            result.SetDefinitionId(definitionId);
            return result;
        }

        #endregion
    }

    /// <summary>
    /// Implementation of <see cref="IApiClientForImport"/> that provides access to application and 
    /// admin API services. This service is registered in the DI as a singleton.
    /// </summary>

    public class ApiClientForImport : ApiClientBase, IApiClientForImport
    {
        #region Lifecycle

        private readonly IServiceProvider _services;

        public ApiClientForImport(IServiceProvider services)
        {
            _services = services;
        }

        #endregion

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
    }

    /// <summary>
    /// Implementation of <see cref="IApiClientForTemplating"/> that provides access to application and 
    /// admin API services. This service is registered in the DI as a singleton.
    /// </summary>
    public class ApiClientForTemplating : ApiClientBase, IApiClientForTemplating
    {
        #region Lifecycle

        private readonly IServiceProvider _services;

        public ApiClientForTemplating(IServiceProvider services)
        {
            _services = services;
        }

        #endregion

        public async Task<IReadOnlyList<DynamicRow>> GetAggregate(string collection, int? definitionId, string select, string filter, string having, string orderby, int? top, DateTimeOffset? now, CancellationToken cancellation)
        {
            using var scope = _services.CreateScope();

            var service = FactServiceByCollectionName(scope.ServiceProvider, collection, definitionId);
            var result = await service.GetAggregate(new GetAggregateArguments
            {
                Select = select,
                Filter = filter,
                Having = having,
                OrderBy = orderby,
                Top = top ?? int.MaxValue,
                Now = now,
            },
            cancellation);

            return result.Data;
        }

        public async Task<IReadOnlyList<Entity>> GetEntities(string collection, int? definitionId, string select, string filter, string orderby, int? top, int? skip, DateTimeOffset? now, CancellationToken cancellation)
        {
            using var scope = _services.CreateScope();

            var service = FactServiceByCollectionName(scope.ServiceProvider, collection, definitionId);
            var result = await service.GetEntities(new GetArguments
            {
                Select = select,
                Filter = filter,
                OrderBy = orderby,
                Top = top ?? int.MaxValue,
                Skip = skip ?? 0,
                Now = now,
            },
            cancellation);

            return result.Data;
        }

        public async Task<IReadOnlyList<EntityWithKey>> GetEntitiesByIds(string collection, int? definitionId, string select, IList ids, DateTimeOffset? now, CancellationToken cancellation)
        {
            using var scope = _services.CreateScope();

            var service = FactWithIdServiceByCollectionName(scope.ServiceProvider, collection, definitionId);
            var result = await service.GetByIds(ids.Cast<object>().ToList(), new SelectExpandArguments
            {
                Select = select,
                Now = now,
            },
            cancellation);

            return result.Data;
        }

        public async Task<EntityWithKey> GetEntityById(string collection, int? definitionId, string select, object id, DateTimeOffset? now, CancellationToken cancellation)
        {
            using var scope = _services.CreateScope();

            var service = FactGetByIdServiceByCollectionName(scope.ServiceProvider, collection, definitionId);
            var result = await service.GetById(id, new GetByIdArguments
            {
                Select = select,
                Now = now,
            },
            cancellation);

            return result.Entity;
        }

        public async Task<IReadOnlyList<DynamicRow>> GetFact(string collection, int? definitionId, string select, string filter, string orderby, int? top, int? skip, DateTimeOffset? now, CancellationToken cancellation)
        {
            using var scope = _services.CreateScope();

            var service = FactServiceByCollectionName(scope.ServiceProvider, collection, definitionId);
            var result = await service.GetFact(new FactArguments
            {
                Select = select,
                Filter = filter,
                OrderBy = orderby,
                Top = top ?? int.MaxValue,
                Skip = skip ?? 0,
                Now = now,
            },
            cancellation);

            return result.Data;
        }

        public async Task<ImageResult> GetImage(string collection, int? definitionId, int entityId, CancellationToken cancellation)
        {
            using var scope = _services.CreateScope();

            var service = ImageGetterByCollectionName(scope.ServiceProvider, collection, definitionId);
            var result = await service.GetImage(entityId, cancellation);

            return result;
        }

        public async Task<FileResult> GetXmlInvoice(int docDefId, int docId, CancellationToken cancellation)
        {
            using var scope = _services.CreateScope();

            var service = DocumentService(scope.ServiceProvider, docDefId);
            var result = await service.GetInvoiceXml(docId, cancellation);

            return result;
        }
    }

    /// <summary>
    /// Implementation of <see cref="IApiClientForTemplating"/> that accesses the API in userless mode.
    /// </summary>
    public class UserlessApiClientForTemplating : ApiClientForTemplating
    {
        private readonly int? _tenantId;

        public UserlessApiClientForTemplating(int? tenantId, IServiceProvider services) : base(services)
        {
            _tenantId = tenantId;
        }

        protected override IFactGetByIdServiceBase FactGetByIdServiceByCollectionName(IServiceProvider services, string collection, int? definitionId = null)
        {
            var service = base.FactGetByIdServiceByCollectionName(services, collection, definitionId);
            return ToUserlessService(service);
        }

        protected override IFactService FactServiceByCollectionName(IServiceProvider services, string collection, int? definitionId = null)
        {
            var service = base.FactServiceByCollectionName(services, collection, definitionId);
            return ToUserlessService(service);
        }

        protected override IFactWithIdService FactWithIdServiceByCollectionName(IServiceProvider services, string collection, int? definitionId = null)
        {
            var service = base.FactWithIdServiceByCollectionName(services, collection, definitionId);
            return ToUserlessService(service);
        }

        protected override IImageGetter ImageGetterByCollectionName(IServiceProvider services, string collection, int? definitionId = null)
        {
            var service = base.ImageGetterByCollectionName(services, collection, definitionId);
            return ToUserlessService(service);
        }

        protected override DocumentsService DocumentService(IServiceProvider services, int definitionId)
        {
            var service = base.DocumentService(services, definitionId);
            return ToUserlessService(service);
        }

        private T ToUserlessService<T>(T service) where T : IServiceBase
        {
            service.SetContext(new UserlessContextAccessor(_tenantId));
            return service;
        }
    }

    /// <summary>
    /// A singleton service responsible for creating all implemenations <see cref="IApiClientForTemplating"/>.
    /// </summary>
    public class ApiClientForTemplatingFactory
    {
        private readonly IServiceProvider _services;

        public ApiClientForTemplatingFactory(IServiceProvider services)
        {
            _services = services;
        }

        /// <summary>
        /// Returns the default service which reads the user information from the HTTP context.
        /// </summary>
        public IApiClientForTemplating GetDefault()
        {
            return new ApiClientForTemplating(_services);
        }

        /// <summary>
        /// Returns an implementation that uses a userless service context (no access control).
        /// </summary>
        public IApiClientForTemplating GetUserless(int tenantId)
        {
            return new UserlessApiClientForTemplating(tenantId, _services);
        }
    }

    /// <summary>
    /// An implementation of <see cref="IServiceContextAccessor"/> that instructs the API to
    /// operate in userless mode (e.g. for background jobs that are running without a user)
    /// </summary>
    public class UserlessContextAccessor : IServiceContextAccessor
    {
        private readonly int? _tenantId;

        public UserlessContextAccessor(int? tenantId)
        {
            _tenantId = tenantId;
        }

        public bool IsAnonymous => true;

        public bool IsServiceAccount => throw new InvalidOperationException($"[Bug] Should not access {nameof(IsServiceAccount)} of a userless client.");

        public string ExternalUserId => throw new InvalidOperationException($"[Bug] Should not access {nameof(ExternalUserId)} of a userless client.");

        public string ExternalEmail => throw new InvalidOperationException($"[Bug] Should not access {nameof(ExternalEmail)} of a userless client.");

        public string ExternalClientId => throw new InvalidOperationException($"[Bug] Should not access {nameof(ExternalClientId)} of a userless client.");

        public int? TenantId => _tenantId;

        public bool IsSilent => true;

        public DateTime Today => DateTime.Today;

        public string Calendar => "gr";
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
