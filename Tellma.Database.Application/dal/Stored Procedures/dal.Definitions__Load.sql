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
SELECT * FROM [map].[ReportParameterDefinitions]()
SELECT * FROM [map].[ReportSelectDefinitions]()
SELECT * FROM [map].[ReportRowDefinitions]()
SELECT * FROM [map].[ReportColumnDefinitions]()
SELECT * FROM [map].[ReportMeasureDefinitions]()
