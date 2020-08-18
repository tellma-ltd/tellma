CREATE FUNCTION [map].[LineDefinitions]()
RETURNS TABLE
AS
RETURN (
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
		LD.[GenerateLabel],
		LD.[GenerateLabel2],
		LD.[GenerateLabel3],
		LD.[GenerateScript],
		LD.[PreprocessScript],
		LD.[ValidateScript],
		LD.[SavedById],
		LD.[ValidFrom],
		LD.[ValidTo],
		IIF(W.[LineDefinitionId] IS NULL, 0, 1) AS HasWorkflow
	FROM [dbo].[LineDefinitions] LD
	LEFT JOIN (
		Select DISTINCT LineDefinitionId
		FROM dbo.Workflows
	) W ON W.LineDefinitionId = LD.[Id]
);