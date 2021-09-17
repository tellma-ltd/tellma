using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api.Dto;
using Tellma.Model.Admin;
using Tellma.Model.Application;

namespace Tellma.Client
{
    public class AccountClassificationsClient : CrudClientBase<AccountClassificationForSave, AccountClassification, int>
    {
        internal AccountClassificationsClient(IClientBehavior behavior) : base(behavior)
        {
        }

        protected override string ControllerPath => "account-classifications";

        public async Task<EntitiesResult<AccountClassification>> Activate(List<int> ids, Request<ActivateArguments> request = null, CancellationToken cancellation = default)
        {
            return await ActivateImpl(ids, request, cancellation).ConfigureAwait(false);
        }

        public async Task<EntitiesResult<AccountClassification>> Deactivate(List<int> ids, Request<DeactivateArguments> request = null, CancellationToken cancellation = default)
        {
            return await DeactivateImpl(ids, request, cancellation).ConfigureAwait(false);
        }
    }


    public class AccountsClient : CrudClientBase<AccountForSave, Account, int>
    {
        internal AccountsClient(IClientBehavior behavior) : base(behavior)
        {
        }

        protected override string ControllerPath => "accounts";

        public async Task<EntitiesResult<Account>> Activate(List<int> ids, Request<ActivateArguments> request = null, CancellationToken cancellation = default)
        {
            return await ActivateImpl(ids, request, cancellation).ConfigureAwait(false);
        }

        public async Task<EntitiesResult<Account>> Deactivate(List<int> ids, Request<DeactivateArguments> request = null, CancellationToken cancellation = default)
        {
            return await DeactivateImpl(ids, request, cancellation).ConfigureAwait(false);
        }
    }


    public class AccountTypesClient : CrudClientBase<AccountTypeForSave, AccountType, int>
    {
        internal AccountTypesClient(IClientBehavior behavior) : base(behavior)
        {
        }

        protected override string ControllerPath => "account-types";

        public async Task<EntitiesResult<AccountType>> Activate(List<int> ids, Request<ActivateArguments> request = null, CancellationToken cancellation = default)
        {
            return await ActivateImpl(ids, request, cancellation).ConfigureAwait(false);
        }

        public async Task<EntitiesResult<AccountType>> Deactivate(List<int> ids, Request<DeactivateArguments> request = null, CancellationToken cancellation = default)
        {
            return await DeactivateImpl(ids, request, cancellation).ConfigureAwait(false);
        }
    }


    public class AdminUsersClient : CrudClientBase<AdminUserForSave, AdminUser, int>
    {
        internal AdminUsersClient(IClientBehavior behavior) : base(behavior)
        {
        }

        protected override string ControllerPath => "admin-users";

        public async Task<EntitiesResult<AdminUser>> Activate(List<int> ids, Request<ActivateArguments> request = null, CancellationToken cancellation = default)
        {
            return await ActivateImpl(ids, request, cancellation).ConfigureAwait(false);
        }

        public async Task<EntitiesResult<AdminUser>> Deactivate(List<int> ids, Request<DeactivateArguments> request = null, CancellationToken cancellation = default)
        {
            return await DeactivateImpl(ids, request, cancellation).ConfigureAwait(false);
        }
    }


    public class AgentDefinitionsClient : CrudClientBase<AgentDefinitionForSave, AgentDefinition, int>
    {
        internal AgentDefinitionsClient(IClientBehavior behavior) : base(behavior)
        {
        }

        protected override string ControllerPath => "agent-definitions";
    }


    public class AgentsClient : CrudClientBase<AgentForSave, Agent, int>
    {
        private readonly int _definitionId;

        internal AgentsClient(int definitionId, IClientBehavior behavior) : base(behavior)
        {
            _definitionId = definitionId;
        }

        protected override string ControllerPath => $"agents/{_definitionId}";

        public async Task<EntitiesResult<Agent>> Activate(List<int> ids, Request<ActivateArguments> request = null, CancellationToken cancellation = default)
        {
            return await ActivateImpl(ids, request, cancellation).ConfigureAwait(false);
        }

        public async Task<EntitiesResult<Agent>> Deactivate(List<int> ids, Request<DeactivateArguments> request = null, CancellationToken cancellation = default)
        {
            return await DeactivateImpl(ids, request, cancellation).ConfigureAwait(false);
        }
    }


    public class AgentsGenericClient : FactWithIdClientBase<Agent, int>
    {
        internal AgentsGenericClient(IClientBehavior behavior) : base(behavior)
        {
        }

        protected override string ControllerPath => "agents";
    }


    public class CentersClient : CrudClientBase<CenterForSave, Center, int>
    {
        internal CentersClient(IClientBehavior behavior) : base(behavior)
        {
        }

        protected override string ControllerPath => "centers";

        public async Task<EntitiesResult<Center>> Activate(List<int> ids, Request<ActivateArguments> request = null, CancellationToken cancellation = default)
        {
            return await ActivateImpl(ids, request, cancellation).ConfigureAwait(false);
        }

        public async Task<EntitiesResult<Center>> Deactivate(List<int> ids, Request<DeactivateArguments> request = null, CancellationToken cancellation = default)
        {
            return await DeactivateImpl(ids, request, cancellation).ConfigureAwait(false);
        }
    }


    public class CurrenciesClient : CrudClientBase<CurrencyForSave, Currency, string>
    {
        internal CurrenciesClient(IClientBehavior behavior) : base(behavior)
        {
        }

        protected override string ControllerPath => "currencies";

        public async Task<EntitiesResult<Currency>> Activate(List<string> ids, Request<ActivateArguments> request = null, CancellationToken cancellation = default)
        {
            return await ActivateImpl(ids, request, cancellation).ConfigureAwait(false);
        }

        public async Task<EntitiesResult<Currency>> Deactivate(List<string> ids, Request<DeactivateArguments> request = null, CancellationToken cancellation = default)
        {
            return await DeactivateImpl(ids, request, cancellation).ConfigureAwait(false);
        }
    }


    public class DashboardDefinitionsClient : CrudClientBase<DashboardDefinitionForSave, DashboardDefinition, int>
    {
        internal DashboardDefinitionsClient(IClientBehavior behavior) : base(behavior)
        {
        }

        protected override string ControllerPath => "dashboard-definitions";
    }


    public class DetailsEntriesClient : FactWithIdClientBase<DetailsEntry, int>
    {
        internal DetailsEntriesClient(IClientBehavior behavior) : base(behavior)
        {
        }

        protected override string ControllerPath => "details-entries";
    }


    public class DocumentDefinitionsClient : CrudClientBase<DocumentDefinitionForSave, DocumentDefinition, int>
    {
        internal DocumentDefinitionsClient(IClientBehavior behavior) : base(behavior)
        {
        }

        protected override string ControllerPath => "document-definitions";
    }


    public class DocumentsClient : CrudClientBase<DocumentForSave, Document, int>
    {
        private readonly int _definitionId;

        internal DocumentsClient(int definitionId, IClientBehavior behavior) : base(behavior)
        {
            _definitionId = definitionId;
        }

        protected override string ControllerPath => $"documents/{_definitionId}";
    }


    public class DocumentsGenericClient : FactWithIdClientBase<Document, int>
    {
        internal DocumentsGenericClient(IClientBehavior behavior) : base(behavior)
        {
        }

        protected override string ControllerPath => "documents";
    }


    public class EmailsClient : FactGetByIdClientBase<EmailForQuery, int>
    {
        internal EmailsClient(IClientBehavior behavior) : base(behavior)
        {
        }

        protected override string ControllerPath => "emails";
    }


    public class EntryTypesClient : CrudClientBase<EntryTypeForSave, EntryType, int>
    {
        internal EntryTypesClient(IClientBehavior behavior) : base(behavior)
        {
        }

        protected override string ControllerPath => "entry-types";

        public async Task<EntitiesResult<EntryType>> Activate(List<int> ids, Request<ActivateArguments> request = null, CancellationToken cancellation = default)
        {
            return await ActivateImpl(ids, request, cancellation).ConfigureAwait(false);
        }

        public async Task<EntitiesResult<EntryType>> Deactivate(List<int> ids, Request<DeactivateArguments> request = null, CancellationToken cancellation = default)
        {
            return await DeactivateImpl(ids, request, cancellation).ConfigureAwait(false);
        }
    }


    public class ExchangeRatesClient : CrudClientBase<ExchangeRateForSave, ExchangeRate, int>
    {
        internal ExchangeRatesClient(IClientBehavior behavior) : base(behavior)
        {
        }

        protected override string ControllerPath => "exchange-rates";
    }


    public class FinancialSettingsClient : ClientBase
    {
        protected override string ControllerPath => "financial-settings";

        public FinancialSettingsClient(IClientBehavior behavior) : base(behavior)
        {
        }
    }


    public class GeneralSettingsClient : ClientBase
    {
        protected override string ControllerPath => "general-settings";

        public GeneralSettingsClient(IClientBehavior behavior) : base(behavior)
        {
        }

        public async Task Ping(Request req = default, CancellationToken cancellation = default)
        {
            // Prepare the request
            var urlBldr = GetActionUrlBuilder("ping");
            var method = HttpMethod.Get;
            var msg = new HttpRequestMessage(method, urlBldr.Uri);

            // Send the request
            using var httpResponse = await SendAsync(msg, req, cancellation).ConfigureAwait(false);
            await httpResponse.EnsureSuccess(cancellation).ConfigureAwait(false);
        }
    }


    public class IdentityServerClientsClient : CrudClientBase<IdentityServerClientForSave, IdentityServerClient, int>
    {
        internal IdentityServerClientsClient(IClientBehavior behavior) : base(behavior)
        {
        }

        protected override string ControllerPath => "identity-server-clients";
    }


    public class IdentityServerUsersClient : FactGetByIdClientBase<IdentityServerUser, string>
    {
        internal IdentityServerUsersClient(IClientBehavior behavior) : base(behavior)
        {
        }

        protected override string ControllerPath => "identity-server-users";
    }


    public class IfrsConceptsClient : FactGetByIdClientBase<IfrsConcept, int>
    {
        internal IfrsConceptsClient(IClientBehavior behavior) : base(behavior)
        {
        }

        protected override string ControllerPath => "ifrs-concepts";
    }


    public class LineDefinitionsClient : CrudClientBase<LineDefinitionForSave, LineDefinition, int>
    {
        internal LineDefinitionsClient(IClientBehavior behavior) : base(behavior)
        {
        }

        protected override string ControllerPath => "line-definitions";
    }


    public class LookupDefinitionsClient : CrudClientBase<LookupDefinitionForSave, LookupDefinition, int>
    {
        internal LookupDefinitionsClient(IClientBehavior behavior) : base(behavior)
        {
        }

        protected override string ControllerPath => "lookup-definitions";
    }


    public class LookupsClient : CrudClientBase<LookupForSave, Lookup, int>
    {
        private readonly int _definitionId;

        internal LookupsClient(int definitionId, IClientBehavior behavior) : base(behavior)
        {
            _definitionId = definitionId;
        }

        protected override string ControllerPath => $"lookups/{_definitionId}";

        public async Task<EntitiesResult<Lookup>> Activate(List<int> ids, Request<ActivateArguments> request = null, CancellationToken cancellation = default)
        {
            return await ActivateImpl(ids, request, cancellation).ConfigureAwait(false);
        }

        public async Task<EntitiesResult<Lookup>> Deactivate(List<int> ids, Request<DeactivateArguments> request = null, CancellationToken cancellation = default)
        {
            return await DeactivateImpl(ids, request, cancellation).ConfigureAwait(false);
        }
    }


    public class LookupsGenericClient : FactWithIdClientBase<Lookup, int>
    {
        internal LookupsGenericClient(IClientBehavior behavior) : base(behavior)
        {
        }

        protected override string ControllerPath => "lookups";
    }


    public class PrintingTemplatesClient : CrudClientBase<PrintingTemplateForSave, PrintingTemplate, int>
    {
        internal PrintingTemplatesClient(IClientBehavior behavior) : base(behavior)
        {
        }

        protected override string ControllerPath => "printing-templates";
    }


    public class OutboxClient : FactWithIdClientBase<OutboxRecord, int>
    {
        internal OutboxClient(IClientBehavior behavior) : base(behavior)
        {
        }

        protected override string ControllerPath => "outbox";
    }


    public class ReportDefinitionsClient : CrudClientBase<ReportDefinitionForSave, ReportDefinition, int>
    {
        internal ReportDefinitionsClient(IClientBehavior behavior) : base(behavior)
        {
        }

        protected override string ControllerPath => "report-definitions";
    }


    public class ResourceDefinitionsClient : CrudClientBase<ResourceDefinitionForSave, ResourceDefinition, int>
    {
        internal ResourceDefinitionsClient(IClientBehavior behavior) : base(behavior)
        {
        }

        protected override string ControllerPath => "resource-definitions";
    }


    public class ResourcesClient : CrudClientBase<ResourceForSave, Resource, int>
    {
        private readonly int _definitionId;

        internal ResourcesClient(int definitionId, IClientBehavior behavior) : base(behavior)
        {
            _definitionId = definitionId;
        }

        protected override string ControllerPath => $"resources/{_definitionId}";

        public async Task<EntitiesResult<Resource>> Activate(List<int> ids, Request<ActivateArguments> request = null, CancellationToken cancellation = default)
        {
            return await ActivateImpl(ids, request, cancellation).ConfigureAwait(false);
        }

        public async Task<EntitiesResult<Resource>> Deactivate(List<int> ids, Request<DeactivateArguments> request = null, CancellationToken cancellation = default)
        {
            return await DeactivateImpl(ids, request, cancellation).ConfigureAwait(false);
        }
    }


    public class ResourcesGenericClient : FactWithIdClientBase<Resource, int>
    {
        internal ResourcesGenericClient(IClientBehavior behavior) : base(behavior)
        {
        }

        protected override string ControllerPath => "resources";
    }


    public class RolesClient : CrudClientBase<RoleForSave, Role, int>
    {
        internal RolesClient(IClientBehavior behavior) : base(behavior)
        {
        }

        protected override string ControllerPath => "roles";

        public async Task<EntitiesResult<Role>> Activate(List<int> ids, Request<ActivateArguments> request = null, CancellationToken cancellation = default)
        {
            return await ActivateImpl(ids, request, cancellation).ConfigureAwait(false);
        }

        public async Task<EntitiesResult<Role>> Deactivate(List<int> ids, Request<DeactivateArguments> request = null, CancellationToken cancellation = default)
        {
            return await DeactivateImpl(ids, request, cancellation).ConfigureAwait(false);
        }
    }


    public class SmsMessagesClient : FactGetByIdClientBase<SmsMessageForQuery, int>
    {
        internal SmsMessagesClient(IClientBehavior behavior) : base(behavior)
        {
        }

        protected override string ControllerPath => "sms-messages";
    }


    public class UnitsClient : CrudClientBase<UnitForSave, Unit, int>
    {
        internal UnitsClient(IClientBehavior behavior) : base(behavior)
        {
        }

        protected override string ControllerPath => "units";

        public async Task<EntitiesResult<Unit>> Activate(List<int> ids, Request<ActivateArguments> request = null, CancellationToken cancellation = default)
        {
            return await ActivateImpl(ids, request, cancellation).ConfigureAwait(false);
        }

        public async Task<EntitiesResult<Unit>> Deactivate(List<int> ids, Request<DeactivateArguments> request = null, CancellationToken cancellation = default)
        {
            return await DeactivateImpl(ids, request, cancellation).ConfigureAwait(false);
        }
    }


    public class UsersClient : CrudClientBase<UserForSave, User, int>
    {
        internal UsersClient(IClientBehavior behavior) : base(behavior)
        {
        }

        protected override string ControllerPath => "users";
        public async Task<EntitiesResult<User>> Activate(List<int> ids, Request<ActivateArguments> request = null, CancellationToken cancellation = default)
        {
            return await ActivateImpl(ids, request, cancellation).ConfigureAwait(false);
        }

        public async Task<EntitiesResult<User>> Deactivate(List<int> ids, Request<DeactivateArguments> request = null, CancellationToken cancellation = default)
        {
            return await DeactivateImpl(ids, request, cancellation).ConfigureAwait(false);
        }
    }
}
