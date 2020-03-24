CREATE PROCEDURE [dal].[Documents_State__Update]
	@Ids dbo.IdList READONLY,
	@State SMALLINT
AS
UPDATE dbo.Documents
SET
	[State]	= @State,
	[StateAt] = SYSDATETIMEOFFSET()
WHERE Id IN (SELECT [Id] FROM @Ids)
AND [State] <> @State;

-- Make sure Non-workflow lines are updated
UPDATE dbo.Lines
SET [State] = 4 * @State
WHERE [DocumentId] IN (SELECT [Id] FROM @Ids)
AND [State] <> 4 * @State
AND [DefinitionId] IN (
	SELECT [Id]
	FROM map.[LineDefinitions]()
	WHERE [HasWorkflow] = 0
);