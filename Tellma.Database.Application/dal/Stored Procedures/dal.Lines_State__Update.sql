CREATE PROCEDURE [dal].[Lines_State__Update]
	@Ids [dbo].[IdList] READONLY,
	@ToState SMALLINT --NVARCHAR (30)
AS
BEGIN
	IF EXISTS(
		SELECT *
		FROM @Ids FE -- some of the lines
		JOIN dbo.Entries E ON E.[LineId] = FE.[Id]
		JOIN dbo.ReconciliationEntries RE ON E.[Id] = RE.[EntryId] -- have been reconciled
		AND (@ToState BETWEEN 0 AND 3) --  but we are now unposting them
	)
		DELETE FROM dbo.Reconciliations -- so, delete their reconciliation entries...
		WHERE [Id] IN (
			SELECT RE.[ReconciliationId]
			FROM [ReconciliationEntries] RE
			JOIN dbo.Entries E ON RE.[EntryId] = E.[Id]
			JOIN @Ids FE ON E.[LineId] = FE.[Id]
		)

	UPDATE dbo.[Lines]
	SET
		[State] = @ToState
	Where [Id] IN (
		SELECT [Id] FROM @Ids
	);
END;