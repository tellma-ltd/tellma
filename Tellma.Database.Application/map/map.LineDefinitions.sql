CREATE FUNCTION [map].[LineDefinitions]()
RETURNS TABLE
AS
RETURN (
	WITH 
	WorkflowLineDefinitions AS (
		Select DISTINCT LineDefinitionId
		FROM dbo.Workflows
	)
	SELECT
		LD.[Id],
		LD.[Code],
		LD.[Description],
		LD.[Description2],
		LD.[Description3],
		LD.[TitleSingular],
		LD.[TitleSingular2],
		LD.[TitleSingular3],
		LD.[TitlePlural],
		LD.[TitlePlural2],
		LD.[TitlePlural3],
		LD.[AllowSelectiveSigning],
		LD.[ViewDefaultsToForm],
		LD.[BarcodeColumnIndex],
		LD.[BarcodeProperty],
		LD.[BarcodeExistingItemHandling],
		LD.[BarcodeBeepsEnabled],
		LD.[GenerateLabel],
		LD.[GenerateLabel2],
		LD.[GenerateLabel3],
		LD.[GenerateScript],
		LD.[PreprocessScript],
		LD.[ValidateScript],
		LD.[SavedById],
		LD.[ValidFrom],
		LD.[ValidTo],
		IIF(WLD.[LineDefinitionId] IS NULL, 0, 1) AS HasWorkflow
	FROM [dbo].[LineDefinitions] LD
	LEFT JOIN WorkflowLineDefinitions WLD ON WLD.LineDefinitionId = LD.[Id]
);