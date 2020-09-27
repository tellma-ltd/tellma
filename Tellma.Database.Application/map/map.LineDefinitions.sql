CREATE FUNCTION [map].[LineDefinitions]()
RETURNS TABLE
AS
RETURN (
	WITH LineDefinitionParticipants AS (
		SELECT DISTINCT LD.[Id], ATC.[ParticipantDefinitionId]
		FROM dbo.LineDefinitions LD
		JOIN dbo.LineDefinitionEntries LDE ON LDE.[LineDefinitionId] = LD.[Id]
		JOIN dbo.AccountTypes ATP ON LDE.[ParentAccountTypeId] = ATP.[Id]
		JOIN dbo.AccountTypes ATC ON (ATC.[Node].IsDescendantOf(ATP.[Node]) = 1)
		JOIN dbo.LineDefinitionColumns LDC ON LDC.LineDefinitionId = LD.[Id]
		WHERE ATC.[ParticipantDefinitionId] IS NOT NULL
		AND LDC.ColumnName = N'NotedRelationId'
		AND LDC.[InheritsFromHeader] = 2
	),
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
		LD.[GenerateLabel],
		LD.[GenerateLabel2],
		LD.[GenerateLabel3],
		LD.[GenerateScript],
		LD.[PreprocessScript],
		LD.[ValidateScript],
		LD.[SavedById],
		LD.[ValidFrom],
		LD.[ValidTo],
		LDP.[ParticipantDefinitionId],
		IIF(WLD.[LineDefinitionId] IS NULL, 0, 1) AS HasWorkflow
	FROM [dbo].[LineDefinitions] LD
	LEFT JOIN LineDefinitionParticipants LDP ON LD.[Id] = LDP.[Id]
	LEFT JOIN WorkflowLineDefinitions WLD ON WLD.LineDefinitionId = LD.[Id]
);