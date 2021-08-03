using System;
using System.Collections.Generic;
using Tellma.Model.Application;

namespace Tellma.Repository.Application
{
    public class DefinitionsResult
    {
        public DefinitionsResult(Guid version, string referenceSourceDefinitionCodes,
            IEnumerable<LookupDefinition> lookupDefinitions,
            IEnumerable<RelationDefinition> relationDefinitions,
            IEnumerable<ResourceDefinition> resourceDefinitions,
            IEnumerable<ReportDefinition> reportDefinitions,
            IEnumerable<DashboardDefinition> dashboardDefinitions,
            IEnumerable<DocumentDefinition> documentDefinitions,
            IEnumerable<LineDefinition> lineDefinitions,
            IEnumerable<MarkupTemplate> markupDefinitions,
            IReadOnlyDictionary<int, List<int>> entryCustodianDefinitionIds,
            IReadOnlyDictionary<int, List<int>> entryRelationDefinitionIds,
            IReadOnlyDictionary<int, List<int>> entryResourceDefinitionIds,
            IReadOnlyDictionary<int, List<int>> entryNotedRelationDefinitionIds)
        {
            Version = version;
            ReferenceSourceDefinitionCodes = referenceSourceDefinitionCodes;
            LookupDefinitions = lookupDefinitions;
            RelationDefinitions = relationDefinitions;
            ResourceDefinitions = resourceDefinitions;
            ReportDefinitions = reportDefinitions;
            DashboardDefinitions = dashboardDefinitions;
            DocumentDefinitions = documentDefinitions;
            LineDefinitions = lineDefinitions;
            MarkupDefinitions = markupDefinitions;
            EntryCustodianDefinitionIds = entryCustodianDefinitionIds;
            EntryRelationDefinitionIds = entryRelationDefinitionIds;
            EntryResourceDefinitionIds = entryResourceDefinitionIds;
            EntryNotedRelationDefinitionIds = entryNotedRelationDefinitionIds;
        }

        public Guid Version { get; }
        public string ReferenceSourceDefinitionCodes { get; }
        public IEnumerable<LookupDefinition> LookupDefinitions { get; }
        public IEnumerable<RelationDefinition> RelationDefinitions { get; }
        public IEnumerable<ResourceDefinition> ResourceDefinitions { get; }
        public IEnumerable<ReportDefinition> ReportDefinitions { get; }
        public IEnumerable<DashboardDefinition> DashboardDefinitions { get; }
        public IEnumerable<DocumentDefinition> DocumentDefinitions { get; }
        public IEnumerable<LineDefinition> LineDefinitions { get; }
        public IEnumerable<MarkupTemplate> MarkupDefinitions { get; }
        public IReadOnlyDictionary<int, List<int>> EntryCustodianDefinitionIds { get; }
        public IReadOnlyDictionary<int, List<int>> EntryRelationDefinitionIds { get; }
        public IReadOnlyDictionary<int, List<int>> EntryResourceDefinitionIds { get; }
        public IReadOnlyDictionary<int, List<int>> EntryNotedRelationDefinitionIds { get; }
    }
}
