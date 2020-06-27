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
	
-- Get the contract definitions of the line definition entries
SELECT [Id], [LineDefinitionEntryId], [ContractDefinitionId] FROM [dbo].[LineDefinitionEntryContractDefinitions];
	
-- Get the noted contract definitions of the line definition entries
SELECT [Id], [LineDefinitionEntryId], [NotedContractDefinitionId] FROM [dbo].[LineDefinitionEntryNotedContractDefinitions];
	
-- Get the resource definitions of the line definition entries
SELECT [Id], [LineDefinitionEntryId], [ResourceDefinitionId] FROM [dbo].[LineDefinitionEntryResourceDefinitions];