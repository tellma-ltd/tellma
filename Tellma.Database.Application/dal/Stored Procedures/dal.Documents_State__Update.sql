CREATE PROCEDURE [dal].[Documents_State__Update]
	@Ids dbo.IdList READONLY,
	@State SMALLINT
AS
	DECLARE @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));
	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();

	UPDATE dbo.Documents
	SET
		[State]	= @State,
		[StateAt] = @Now,
		[ModifiedById] = @UserId,
		[ModifiedAt] = @Now
	WHERE Id IN (SELECT [Id] FROM @Ids)
	AND [State] <> @State;

	-- Make sure Non-workflow lines are updated
	UPDATE L
	SET L.[State] = D.[LastLineState] * @State
	FROM dbo.Lines L
	JOIN map.Documents() D ON L.[DocumentId] = D.[Id]
	WHERE L.[DocumentId] IN (SELECT [Id] FROM @Ids)
	AND L.[State] <> D.[LastLineState] * @State
	AND L.[DefinitionId] IN (
		SELECT [Id]
		FROM map.[LineDefinitions]()
		WHERE [HasWorkflow] = 0
	);