CREATE PROCEDURE [dal].[Definitions__Load]
AS

-- Get the version of it all
SELECT [DefinitionsVersion] FROM [dbo].[Settings]

-- Get the lookup definitions
SELECT * FROM [dbo].[LookupDefinitions]

-- Get the agent definitions
SELECT * FROM [dbo].[AgentDefinitions]

-- Get the resource definitions
SELECT * FROM [dbo].[ResourceDefinitions]

-- Get the report definitions
SELECT * FROM [map].[ReportDefinitions]()
SELECT * FROM [map].[ReportParameterDefinitions]() ORDER BY [Index]
SELECT * FROM [map].[ReportSelectDefinitions]() ORDER BY [Index]
SELECT * FROM [map].[ReportRowDefinitions]() ORDER BY [Index]
SELECT * FROM [map].[ReportColumnDefinitions]() ORDER BY [Index]
SELECT * FROM [map].[ReportMeasureDefinitions]() ORDER BY [Index]

-- Get the document definitions
SELECT * FROM [dbo].[DocumentDefinitions]
SELECT * FROM [dbo].[DocumentDefinitionLineDefinitions] ORDER BY [Index]

-- Get the line definitions
SELECT * FROM [dbo].[LineDefinitions]

SELECT LDE.*,
(Select IsResourceClassification FROM dbo.AccountTypes WHERE [Code] = LDE.[AccountTypeParentCode]) AS IsResourceClassification,
(Select [Id] FROM dbo.AccountTypes WHERE [Code] = LDE.[AccountTypeParentCode]) AS AccountTypeId
FROM [dbo].[LineDefinitionEntries] LDE

SELECT * FROM [dbo].[LineDefinitionColumns] ORDER BY [Index]
SELECT * FROM [dbo].[LineDefinitionStateReasons]