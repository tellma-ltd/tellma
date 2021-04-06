CREATE PROCEDURE [dal].[Reconciliation__Load_Unreconciled]
	@AccountId		INT, 
	@CustodyId		INT, 
	@AsOfDate		DATE, 
	@Top			INT, 
	@Skip			INT,
	@TopExternal	INT, 
	@SkipExternal	INT,
	@EntriesBalance						DECIMAL (19,4) OUTPUT,
	@UnreconciledEntriesBalance			DECIMAL (19,4) OUTPUT,
	@UnreconciledExternalEntriesBalance	DECIMAL (19,4) OUTPUT,
	@UnreconciledEntriesCount			INT OUTPUT,
	@UnreconciledExternalEntriesCount	INT OUTPUT
AS
	SELECT @EntriesBalance = SUM(E.[Direction] * E.[MonetaryValue])
	FROM dbo.Entries E
	JOIN dbo.Lines L ON E.[LineId] = L.[Id]
	WHERE E.[CustodyId] = @CustodyId
	AND E.[AccountId] = @AccountId
	AND L.[State] = 4
	AND L.[PostingDate] <= @AsOfDate;

	With SpecialEntries AS ( -- Internal Entries that were reconciled @AsOfDate
		SELECT DISTINCT RE.[EntryId]
		FROM dbo.ReconciliationEntries RE
		JOIN dbo.Reconciliations R ON RE.ReconciliationId = R.Id
		JOIN dbo.ReconciliationExternalEntries REE ON REE.ReconciliationId = R.Id
		JOIN dbo.ExternalEntries EE ON REE.ExternalEntryId = EE.Id
		WHERE EE.PostingDate <=  @AsOfDate	
		AND EE.[AccountId] = @AccountId
		AND EE.[CustodyId] = @CustodyId
	)
	SELECT
		@UnreconciledEntriesCount = COUNT(*),
		@UnreconciledEntriesBalance = SUM(
			IIF (L.[PostingDate] <= @AsOfDate , E.[Direction] * E.[MonetaryValue], -E.[Direction] * E.[MonetaryValue])
		)
	FROM dbo.Entries E
	JOIN dbo.Lines L ON E.[LineId] = L.[Id]
	WHERE E.[CustodyId] = @CustodyId
	AND E.[AccountId] = @AccountId
	AND L.[State] = 4
	AND (
		L.[PostingDate] > @AsOfDate -- was reconciled after @AsOfDate
		AND E.[Id] IN (SELECT EntryId FROM SpecialEntries)
		OR
		L.[PostingDate] <= @AsOfDate   -- or was not reconciled @AsOfDate
		AND E.[Id] NOT IN (SELECT EntryId FROM SpecialEntries)
	);

	SELECT @UnreconciledExternalEntriesCount = COUNT(*), @UnreconciledExternalEntriesBalance = SUM(E.[Direction] * E.[MonetaryValue])
	FROM dbo.ExternalEntries E
	WHERE E.[CustodyId] = @CustodyId
	AND E.[AccountId] = @AccountId
	AND E.[Id] NOT IN (SELECT [ExternalEntryId] FROM dbo.ReconciliationExternalEntries)
	AND E.[PostingDate] <= @AsOfDate
	
	SELECT E.[Id], L.[PostingDate], E.[Direction], E.[MonetaryValue], E.[ExternalReference], L.[DocumentId], D.[DefinitionId] AS [DocumentDefinitionId], D.[SerialNumber] AS [DocumentSerialNumber],
			IIF(E.[Id] IN (SELECT [EntryId] FROM dbo.ReconciliationEntries), 1, 0) AS IsReconciledLater
	FROM dbo.Entries E
	JOIN dbo.Lines L ON E.[LineId] = L.[Id]
	JOIN dbo.Documents D ON L.[DocumentId] = D.[Id]
	WHERE E.[CustodyId] = @CustodyId
	AND E.[AccountId] = @AccountId
	AND L.[State] = 4
	AND E.[Id] NOT IN (
		SELECT [EntryId] FROM dbo.ReconciliationEntries
	)
	AND L.[PostingDate] <= @AsOfDate
	ORDER BY L.[PostingDate], E.[MonetaryValue], E.[ExternalReference]
	OFFSET (@Skip) ROWS FETCH NEXT (@Top) ROWS ONLY;
	
	SELECT E.[Id], E.[PostingDate], E.[Direction], E.[MonetaryValue], E.[ExternalReference], E.[CreatedById], E.[CreatedAt], E.[ModifiedById], E.[ModifiedAt]
	FROM dbo.ExternalEntries E
	WHERE E.[CustodyId] = @CustodyId
	AND E.[AccountId] = @AccountId
	AND E.[Id] NOT IN (SELECT [ExternalEntryId] FROM dbo.ReconciliationExternalEntries)
	AND E.[PostingDate] <= @AsOfDate
	ORDER BY E.[PostingDate], E.[MonetaryValue], E.[ExternalReference]
	OFFSET (@SkipExternal) ROWS FETCH NEXT (@TopExternal) ROWS ONLY;