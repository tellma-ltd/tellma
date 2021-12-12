CREATE PROCEDURE [dal].[Documents_State__Update]
	@DefinitionId INT, -- TODO: Restrict the operation to a single document definition at a time
	@Ids [dbo].[IndexedIdList] READONLY,
	@State SMALLINT,
	@UserId INT
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();

	UPDATE [dbo].[Documents]
	SET
		[State]	= @State,
		[StateAt] = @Now,
		[ModifiedById] = @UserId,
		[ModifiedAt] = @Now
	WHERE [Id] IN (SELECT [Id] FROM @Ids)
	AND [State] <> @State; 

	-- Make sure Non-workflow lines are updated
	UPDATE L
	SET L.[State] = LD.[LastLineState] * @State
	FROM dbo.Lines L
	JOIN map.LineDefinitions LD ON LD.[Id] = L.[DefinitionId]
	WHERE L.[DocumentId] IN (SELECT [Id] FROM @Ids)
	AND L.[State] <> LD.[LastLineState] * @State
	AND LD.[HasWorkflow] = 0;

	-- Delete reconciliation ...
	IF @State < 1 -- opening a document with workflow - less lines
	IF EXISTS(
		SELECT *
		FROM @Ids FE -- some of the lines
		JOIN [dbo].[Lines] L ON L.[DocumentId] = FE.[Id]
		JOIN [dbo].[Entries] E ON E.[LineId] = L.[Id]
		JOIN [dbo].[ReconciliationEntries] RE ON E.[Id] = RE.[EntryId] -- have been reconciled
		WHERE L.[DefinitionId] IN (
			SELECT [Id]
			FROM [map].[LineDefinitions]()
			WHERE [HasWorkflow] = 0
		)
	)
		DELETE FROM [dbo].[Reconciliations] -- so, delete their reconciliation entries...
		WHERE [Id] IN (
			SELECT RE.[ReconciliationId]
			FROM [ReconciliationEntries] RE
			JOIN [dbo].[Entries] E ON RE.[EntryId] = E.[Id]
			JOIN [dbo].[Lines] L ON L.[Id] = E.[LineId]
			JOIN @Ids FE ON L.[DocumentId] = FE.[Id]
			WHERE L.[DefinitionId] IN (
				SELECT [Id]
				FROM [map].[LineDefinitions]()
				WHERE [HasWorkflow] = 0
			)
		);
END;