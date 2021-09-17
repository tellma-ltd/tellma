CREATE PROCEDURE [dal].[Definitions__Load]
AS

-- Get the version of it all
SELECT [DefinitionsVersion], [ReferenceSourceAgentDefinitionCodes] FROM [dbo].[Settings];

-- Get the lookup definitions
SELECT * FROM [map].[LookupDefinitions]() WHERE [State] <> N'Hidden';
SELECT * FROM [map].[LookupDefinitionReportDefinitions]() WHERE [LookupDefinitionId] IN (SELECT [Id] FROM [map].[LookupDefinitions]() WHERE [State] <> N'Hidden') ORDER BY [Index];

-- Get the Agent definitions
SELECT * FROM [map].[AgentDefinitions]() WHERE [State] <> N'Hidden';
SELECT * FROM [map].[AgentDefinitionReportDefinitions]() WHERE [AgentDefinitionId] IN (SELECT [Id] FROM [map].[AgentDefinitions]() WHERE [State] <> N'Hidden') ORDER BY [Index];

-- Get the resource definitions
SELECT * FROM [map].[ResourceDefinitions]() WHERE [State] <> N'Hidden';
SELECT * FROM [map].[ResourceDefinitionReportDefinitions]() WHERE [ResourceDefinitionId] IN (SELECT [Id] FROM [map].[ResourceDefinitions]() WHERE [State] <> N'Hidden') ORDER BY [Index];

-- Get the report definitions
SELECT * FROM [map].[ReportDefinitions]()
SELECT * FROM [map].[ReportDefinitionParameters]() ORDER BY [Index];
SELECT * FROM [map].[ReportDefinitionSelects]() ORDER BY [Index];
SELECT * FROM [map].[ReportDefinitionRows]() ORDER BY [Index];
SELECT * FROM [map].[ReportDefinitionColumns]() ORDER BY [Index];
SELECT * FROM [map].[ReportDefinitionDimensionAttributes]() ORDER BY [Index];
SELECT * FROM [map].[ReportDefinitionMeasures]() ORDER BY [Index];

-- Get the dashboard definitions
SELECT * FROM [map].[DashboardDefinitions]() WHERE [ShowInMainMenu] = 1;
SELECT * FROM [map].[DashboardDefinitionWidgets]() WHERE [DashboardDefinitionId] IN (SELECT [Id] FROM [map].[DashboardDefinitions]() WHERE [ShowInMainMenu] = 1)

-- Get the document definitions
DECLARE @DocDefIds [dbo].[IdList];
INSERT INTO @DocDefIds ([Id]) SELECT [Id] FROM [map].[DocumentDefinitions]() WHERE [State] <> N'Hidden' OR [Code] = N'ManualJournalVoucher'

SELECT * FROM [map].[DocumentDefinitions]() WHERE [Id] IN (SELECT [Id] FROM @DocDefIds);
SELECT * FROM [dbo].[DocumentDefinitionLineDefinitions] WHERE [DocumentDefinitionId] IN (SELECT [Id] FROM @DocDefIds) ORDER BY [Index];

-- Load relevant information from Account Types
SELECT T.[Id], T.[EntryTypeParentId] FROM [map].[AccountTypes]() T 
WHERE T.[Id] IN (SELECT [ParentAccountTypeId] FROM [map].[LineDefinitionEntries]())

-- Get the line definitions
SELECT * FROM [map].[LineDefinitions]();

SELECT * FROM [map].[LineDefinitionEntries]() ORDER BY [Index];
SELECT * FROM [dbo].[LineDefinitionColumns] ORDER BY [Index];
SELECT * FROM [dbo].[LineDefinitionStateReasons] WHERE [IsActive] = 1;
SELECT * FROM [dbo].[LineDefinitionGenerateParameters] ORDER BY [Index];
	
-- Get the Agent definitions of the line definition entries
WITH NonHiddenAgentDefinitions AS (
	SELECT [Id] FROM dbo.[AgentDefinitions] WHERE [State] <> N'Hidden'
)
SELECT [LineDefinitionEntryId], [AgentDefinitionId]
FROM [dbo].[LineDefinitionEntryAgentDefinitions]
WHERE [AgentDefinitionId] IN (SELECT [Id] FROM NonHiddenAgentDefinitions)
UNION -- automatically remove duplicates
SELECT DISTINCT LDE.[Id] AS LineDefinitionEntryId, ATCD.[AgentDefinitionId]
FROM dbo.LineDefinitionEntries LDE
JOIN dbo.AccountTypes ATP ON LDE.[ParentAccountTypeId] = ATP.[Id]
JOIN dbo.AccountTypes ATC ON (ATC.[Node].IsDescendantOf(ATP.[Node]) = 1)
JOIN dbo.[AccountTypeAgentDefinitions] ATCD ON ATC.[Id] = ATCD.[AccountTypeId]
WHERE ATCD.[AgentDefinitionId] IN (SELECT [Id] FROM NonHiddenAgentDefinitions);

-- Get the resource definitions of the line definition entries
-- Todo: If the corrected logic works, copy it to Related and Noted Agent
WITH NonHiddenResourceDefinitions AS (
	SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [State] <> N'Hidden'
),
ExplicitDefinitions AS (
	SELECT [LineDefinitionEntryId], [ResourceDefinitionId]
	FROM [dbo].[LineDefinitionEntryResourceDefinitions]
	WHERE [ResourceDefinitionId] IN (SELECT [Id] FROM NonHiddenResourceDefinitions)
)
SELECT [LineDefinitionEntryId], [ResourceDefinitionId] FROM ExplicitDefinitions
UNION
SELECT DISTINCT LDE.[Id] AS LineDefinitionEntryId, ATCD.[ResourceDefinitionId]
FROM dbo.LineDefinitionEntries LDE
JOIN dbo.AccountTypes ATP ON LDE.[ParentAccountTypeId] = ATP.[Id]
JOIN dbo.AccountTypes ATC ON (ATC.[Node].IsDescendantOf(ATP.[Node]) = 1)
JOIN dbo.AccountTypeResourceDefinitions ATCD ON ATC.[Id] = ATCD.[AccountTypeId]
WHERE  ATCD.[ResourceDefinitionId] IN (SELECT [Id] FROM NonHiddenResourceDefinitions)
AND NOT EXISTS(SELECT * FROM ExplicitDefinitions);

-- Get the NotedAgent definitions of the line definition entries
WITH NonHiddenAgentDefinitions AS (
	SELECT [Id] FROM dbo.[AgentDefinitions] WHERE [State] <> N'Hidden'
)
SELECT [LineDefinitionEntryId], [NotedAgentDefinitionId]
FROM [dbo].[LineDefinitionEntryNotedAgentDefinitions]
WHERE [NotedAgentDefinitionId] IN (SELECT [Id] FROM NonHiddenAgentDefinitions)
UNION
SELECT DISTINCT LDE.[Id] AS LineDefinitionEntryId, ATCD.[NotedAgentDefinitionId]
FROM dbo.LineDefinitionEntries LDE
JOIN dbo.AccountTypes ATP ON LDE.[ParentAccountTypeId] = ATP.[Id]
JOIN dbo.AccountTypes ATC ON (ATC.[Node].IsDescendantOf(ATP.[Node]) = 1)
JOIN dbo.[AccountTypeNotedAgentDefinitions] ATCD ON ATC.[Id] = ATCD.[AccountTypeId]
WHERE ATCD.[NotedAgentDefinitionId] IN (SELECT [Id] FROM NonHiddenAgentDefinitions)

-- Get deployed printing templates
SELECT 
	[Id],
	[Name],
	[Name2],
	[Name3],
	[SupportsPrimaryLanguage],
	[SupportsSecondaryLanguage],
	[SupportsTernaryLanguage],
	[Usage],
	[Collection],
	[DefinitionId]
FROM [dbo].[PrintingTemplates] WHERE [IsDeployed] = 1; -- TODO: Only the ones for printing and reports