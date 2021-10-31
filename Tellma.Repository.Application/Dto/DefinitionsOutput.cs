using System;
using System.Collections.Generic;
using Tellma.Model.Application;

namespace Tellma.Repository.Application
{
    public class DefinitionsOutput
    {
        public DefinitionsOutput(Guid version, string referenceSourceDefinitionCodes,
            IEnumerable<LookupDefinition> lookupDefinitions,
            IEnumerable<AgentDefinition> agentDefinitions,
            IEnumerable<ResourceDefinition> resourceDefinitions,
            IEnumerable<ReportDefinition> reportDefinitions,
            IEnumerable<DashboardDefinition> dashboardDefinitions,
            IEnumerable<DocumentDefinition> documentDefinitions,
            IEnumerable<LineDefinition> lineDefinitions,
            IEnumerable<PrintingTemplate> printingTemplates,
            IEnumerable<NotificationTemplate> notificationTemplates,
            IReadOnlyDictionary<int, List<int>> entryAgentDefinitionIds,
            IReadOnlyDictionary<int, List<int>> entryResourceDefinitionIds,
            IReadOnlyDictionary<int, List<int>> entryNotedAgentDefinitionIds,
            IReadOnlyDictionary<int, List<int>> entryNotedResourceDefinitionIds)
        {
            Version = version;
            ReferenceSourceDefinitionCodes = referenceSourceDefinitionCodes;
            LookupDefinitions = lookupDefinitions;
            AgentDefinitions = agentDefinitions;
            ResourceDefinitions = resourceDefinitions;
            ReportDefinitions = reportDefinitions;
            DashboardDefinitions = dashboardDefinitions;
            DocumentDefinitions = documentDefinitions;
            LineDefinitions = lineDefinitions;
            PrintingTemplates = printingTemplates;
            NotificationTemplates = notificationTemplates;
            EntryAgentDefinitionIds = entryAgentDefinitionIds;
            EntryResourceDefinitionIds = entryResourceDefinitionIds;
            EntryNotedAgentDefinitionIds = entryNotedAgentDefinitionIds;
            EntryNotedResourceDefinitionIds = entryNotedResourceDefinitionIds;
        }

        public Guid Version { get; }
        public string ReferenceSourceDefinitionCodes { get; }
        public IEnumerable<LookupDefinition> LookupDefinitions { get; }
        public IEnumerable<AgentDefinition> AgentDefinitions { get; }
        public IEnumerable<ResourceDefinition> ResourceDefinitions { get; }
        public IEnumerable<ReportDefinition> ReportDefinitions { get; }
        public IEnumerable<DashboardDefinition> DashboardDefinitions { get; }
        public IEnumerable<DocumentDefinition> DocumentDefinitions { get; }
        public IEnumerable<LineDefinition> LineDefinitions { get; }
        public IEnumerable<PrintingTemplate> PrintingTemplates { get; }
        public IEnumerable<NotificationTemplate> NotificationTemplates { get; }
        public IReadOnlyDictionary<int, List<int>> EntryAgentDefinitionIds { get; }
        public IReadOnlyDictionary<int, List<int>> EntryResourceDefinitionIds { get; }
        public IReadOnlyDictionary<int, List<int>> EntryNotedAgentDefinitionIds { get; }
        public IReadOnlyDictionary<int, List<int>> EntryNotedResourceDefinitionIds { get; }
    }
}
