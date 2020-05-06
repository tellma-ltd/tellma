CREATE PROCEDURE [dal].[Definitions__Load]
AS

-- Get the version of it all
SELECT [DefinitionsVersion] FROM [dbo].[Settings];

-- Get the lookup definitions
SELECT * FROM [dbo].[LookupDefinitions];

-- Get the agent definitions
SELECT * FROM [dbo].[ContractDefinitions];

-- Get the resource definitions
SELECT * FROM [dbo].[ResourceDefinitions];

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

-- Get the line definitions
SELECT * FROM [map].[LineDefinitions]();

SELECT * FROM [map].[LineDefinitionEntries]() ORDER BY [Index];
SELECT * FROM [dbo].[LineDefinitionColumns] ORDER BY [Index];
SELECT * FROM [dbo].[LineDefinitionStateReasons] WHERE [IsActive] = 1;

-- Get the Account Types that are used by LineDefinitionEntries
SELECT DISTINCT T.[Id],
	--T.[IsResourceClassification],
	T.[EntryTypeParentId]
	--T.[ResourceDefinitionId],
	--T.[AgentDefinitionId],
	--T.[NotedAgentDefinitionId],
	--T.[DueDateLabel],
	--T.[DueDateLabel2],
	--T.[DueDateLabel3],
	--T.[Time1Label],
	--T.[Time1Label2],
	--T.[Time1Label3],
	--T.[Time2Label],
	--T.[Time2Label2],
	--T.[Time2Label3],
	--T.[ExternalReferenceLabel],
	--T.[ExternalReferenceLabel2],
	--T.[ExternalReferenceLabel3],
	--T.[AdditionalReferenceLabel],
	--T.[AdditionalReferenceLabel2],
	--T.[AdditionalReferenceLabel3],
	--T.[NotedAgentNameLabel],
	--T.[NotedAgentNameLabel2],
	--T.[NotedAgentNameLabel3],
	--T.[NotedAmountLabel],
	--T.[NotedAmountLabel2],
	--T.[NotedAmountLabel3],
	--T.[NotedDateLabel],
	--T.[NotedDateLabel2],
	--T.[NotedDateLabel3]
	FROM [dbo].[AccountTypes] T
	JOIN [map].[LineDefinitionEntries]() E ON T.[Id] = E.[AccountDefinitionId]