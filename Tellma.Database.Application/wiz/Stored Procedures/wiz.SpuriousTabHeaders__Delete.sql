CREATE PROCEDURE [wiz].[SpuriousTabHeaders__Delete]
AS
	DELETE FROM dbo.DocumentLineDefinitionEntries
	WHERE [Id] IN (
		SELECT DLDE.Id
		FROM dbo.DocumentLineDefinitionEntries DLDE
		JOIN dbo.Documents D ON DLDE.[DocumentId] = D.[Id]
		LEFT JOIN dbo.Lines L ON
			DLDE.[DocumentId] = L.[DocumentId] AND
			DLDE.[LineDefinitionId] = L.[DefinitionId]
		WHERE L.[DefinitionId] IS NULL
	)