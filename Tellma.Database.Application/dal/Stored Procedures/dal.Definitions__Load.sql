CREATE PROCEDURE [dal].[Definitions__Load]
AS

-- Get the version of it all
SELECT [DefinitionsVersion] FROM [dbo].[Settings];

-- Get the lookup definitions
SELECT * FROM [map].[LookupDefinitions]() WHERE [State] <> N'Hidden';

-- Get the agent definitions
SELECT * FROM [map].[ContractDefinitions]() WHERE [State] <> N'Hidden';

-- Get the resource definitions
SELECT * FROM [map].[ResourceDefinitions]() WHERE [State] <> N'Hidden';

-- Get the report definitions
SELECT * FROM [map].[ReportDefinitions]()
SELECT * FROM [map].[ReportParameterDefinitions]() ORDER BY [Index];
SELECT * FROM [map].[ReportSelectDefinitions]() ORDER BY [Index];
SELECT * FROM [map].[ReportRowDefinitions]() ORDER BY [Index];
SELECT * FROM [map].[ReportColumnDefinitions]() ORDER BY [Index];
SELECT * FROM [map].[ReportMeasureDefinitions]() ORDER BY [Index];

-- Get the document definitions
SELECT * FROM [map].[DocumentDefinitions]();
SELECT * FROM [dbo].[DocumentDefinitionLineDefinitions] ORDER BY [Index];
SELECT * FROM [dbo].[MarkupTemplates] WHERE [Id] IN (SELECT [MarkupTemplateId] FROM [dbo].[DocumentDefinitionMarkupTemplates])
SELECT * FROM [dbo].[DocumentDefinitionMarkupTemplates] ORDER BY [Index];

-- Load relevant information from Account Types
SELECT T.[Id], T.[EntryTypeParentId] FROM [map].[AccountTypes]() T 
WHERE T.[Id] IN (SELECT [AccountTypeId] FROM [map].[LineDefinitionEntries]())

-- Get the line definitions
SELECT * FROM [map].[LineDefinitions]();

SELECT * FROM [map].[LineDefinitionEntries]() ORDER BY [Index];
SELECT * FROM [dbo].[LineDefinitionColumns] ORDER BY [Index];
SELECT * FROM [dbo].[LineDefinitionStateReasons] WHERE [IsActive] = 1;
SELECT * FROM [dbo].[LineDefinitionGenerateParameters] ORDER BY [Index];
	
-- Get the contract definitions of the line definition entries
SELECT [LineDefinitionEntryId], [ContractDefinitionId] FROM [dbo].[LineDefinitionEntryContractDefinitions]
UNION
SELECT DISTINCT LDE.[Id] AS LineDefinitionEntryId, [ContractdefinitionId]
FROM dbo.LineDefinitionEntries LDE
JOIN dbo.AccountTypes ATP ON LDE.AccountTypeId = ATP.[Id]
JOIN dbo.AccountTypes ATC ON (ATC.[Node].IsDescendantOf(ATP.[Node]) = 1)
JOIN dbo.AccountTypeContractDefinitions ATCD ON ATC.[Id] = ATCD.[AccountTypeId]
WHERE LDE.[Id] NOT IN (SELECT LineDefinitionEntryId FROM [LineDefinitionEntryContractDefinitions])
	
-- Get the noted contract definitions of the line definition entries
SELECT [LineDefinitionEntryId], [NotedContractDefinitionId] FROM [dbo].[LineDefinitionEntryNotedContractDefinitions]
UNION
SELECT DISTINCT LDE.[Id] AS LineDefinitionEntryId, [NotedContractdefinitionId]
FROM dbo.LineDefinitionEntries LDE
JOIN dbo.AccountTypes ATP ON LDE.AccountTypeId = ATP.[Id]
JOIN dbo.AccountTypes ATC ON (ATC.[Node].IsDescendantOf(ATP.[Node]) = 1)
JOIN dbo.AccountTypeNotedContractDefinitions ATCD ON ATC.[Id] = ATCD.[AccountTypeId]
WHERE LDE.[Id] NOT IN (SELECT LineDefinitionEntryId FROM [LineDefinitionEntryNotedContractDefinitions])
	
-- Get the resource definitions of the line definition entries
SELECT [LineDefinitionEntryId], [ResourceDefinitionId] FROM [dbo].[LineDefinitionEntryResourceDefinitions]
UNION
SELECT DISTINCT LDE.[Id] AS LineDefinitionEntryId, [ResourcedefinitionId]
FROM dbo.LineDefinitionEntries LDE
JOIN dbo.AccountTypes ATP ON LDE.AccountTypeId = ATP.[Id]
JOIN dbo.AccountTypes ATC ON (ATC.[Node].IsDescendantOf(ATP.[Node]) = 1)
JOIN dbo.AccountTypeResourceDefinitions ATCD ON ATC.[Id] = ATCD.[AccountTypeId]
WHERE LDE.[Id] NOT IN (SELECT LineDefinitionEntryId FROM [LineDefinitionEntryResourceDefinitions])