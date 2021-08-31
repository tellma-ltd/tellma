using Tellma.Model.Admin;
using Tellma.Model.Application;

namespace Tellma.Client
{
    public class AccountsClient : CrudClientBase<AccountForSave, Account, int>
    {
        internal AccountsClient(IClientBehavior behavior) : base(behavior)
        {
        }

        protected override string ControllerPath => "accounts";
    }


    public class AdminUsersClient : CrudClientBase<AdminUserForSave, AdminUser, int>
    {
        internal AdminUsersClient(IClientBehavior behavior) : base(behavior)
        {
        }

        protected override string ControllerPath => "admin-users";
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
    }


    public class AgentsGenericClient : FactWithIdClientBase<Agent, int>
    {
        internal AgentsGenericClient(IClientBehavior behavior) : base(behavior)
        {
        }

        protected override string ControllerPath => "agents";
    }


    public class CurrenciesClient : CrudClientBase<CurrencyForSave, Currency, string>
    {
        internal CurrenciesClient(IClientBehavior behavior) : base(behavior)
        {
        }

        protected override string ControllerPath => "currencies";
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


    public class ExchangeRatesClient : CrudClientBase<ExchangeRateForSave, ExchangeRate, int>
    {
        internal ExchangeRatesClient(IClientBehavior behavior) : base(behavior)
        {
        }

        protected override string ControllerPath => "exchange-rates";
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
    }


    public class LookupsGenericClient : FactWithIdClientBase<Lookup, int>
    {
        internal LookupsGenericClient(IClientBehavior behavior) : base(behavior)
        {
        }

        protected override string ControllerPath => "lookups";
    }


    public class MarkupTemplatesClient : CrudClientBase<MarkupTemplateForSave, MarkupTemplate, int>
    {
        internal MarkupTemplatesClient(IClientBehavior behavior) : base(behavior)
        {
        }

        protected override string ControllerPath => "markup-templates";
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
    }


    public class UsersClient : CrudClientBase<UserForSave, User, int>
    {
        internal UsersClient(IClientBehavior behavior) : base(behavior)
        {
        }

        protected override string ControllerPath => "users";
    }
}
