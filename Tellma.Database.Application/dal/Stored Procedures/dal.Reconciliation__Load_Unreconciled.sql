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
WITH RECOMPILE
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
	AND E.[Id] NOT IN (
		SELECT DISTINCT RE.[EntryId]
		FROM dbo.ReconciliationEntries RE
		JOIN dbo.Reconciliations R ON RE.ReconciliationId = R.Id
		-- LEFT JOIN to handle the case where an entry was added and reversed by the book side only
		LEFT JOIN dbo.ReconciliationExternalEntries REE ON REE.ReconciliationId = R.Id
		LEFT JOIN dbo.ExternalEntries EE ON REE.ExternalEntryId = EE.Id
		WHERE (EE.PostingDate IS NULL OR EE.PostingDate <=  @AsOfDate)
		AND (EE.[AccountId] IS NULL OR EE.[AccountId] = @AccountId)
		AND (EE.[CustodyId] IS NULL OR EE.[CustodyId] = @CustodyId)
	)
	AND L.[PostingDate] <= @AsOfDate

	SELECT @UnreconciledExternalEntriesCount = COUNT(*),
		@UnreconciledExternalEntriesBalance = SUM(
			IIF (E.[PostingDate] <= @AsOfDate, E.[Direction] * E.[MonetaryValue], -E.[Direction] * E.[MonetaryValue])
		)
	FROM dbo.ExternalEntries E
	WHERE E.[CustodyId] = @CustodyId
	AND E.[AccountId] = @AccountId
	AND E.[Id] NOT IN (
		SELECT DISTINCT REE.[ExternalEntryId]
		FROM dbo.ReconciliationExternalEntries REE
		JOIN dbo.Reconciliations R ON REE.ReconciliationId = R.Id
		-- LEFT JOIN to handle the case where an entry was added and reversed by the bank side only
		LEFT JOIN dbo.ReconciliationEntries RE ON RE.ReconciliationId = R.Id
		LEFT JOIN dbo.Entries E ON RE.EntryId = E.Id
		LEFT JOIN dbo.Lines L ON E.LineId = L.Id
		WHERE (L.PostingDate IS NULL OR L.PostingDate <= @AsOfDate)
		AND (E.[AccountId] IS NULL OR E.[AccountId] = @AccountId)
		AND (E.[CustodyId] IS NULL OR E.[CustodyId] = @CustodyId)
	)
	AND E.[PostingDate] <= @AsOfDate
	
	SELECT E.[Id], L.[PostingDate], E.[Direction], E.[MonetaryValue],
--		E.[ExternalReference],
		IIF([Direction] = 1, E.[NotedAgentName], E.[InternalReference]) AS ExternalReference,
		L.[DocumentId], D.[DefinitionId] AS [DocumentDefinitionId], D.[SerialNumber] AS [DocumentSerialNumber],
		CAST(IIF(E.[Id] IN (SELECT [EntryId] FROM dbo.ReconciliationEntries), 1, 0) AS BIT) AS IsReconciledLater
	FROM dbo.Entries E
	JOIN dbo.Lines L ON E.[LineId] = L.[Id]
	JOIN dbo.Documents D ON L.[DocumentId] = D.[Id]
	WHERE E.[CustodyId] = @CustodyId
	AND E.[AccountId] = @AccountId
	AND L.[State] = 4
	AND E.[Id] NOT IN (
		SELECT DISTINCT RE.[EntryId]
		FROM dbo.ReconciliationEntries RE
		JOIN dbo.Reconciliations R ON RE.ReconciliationId = R.Id
		-- LEFT JOIN to handle the case where an entry was added and reversed by the book side only
		LEFT JOIN dbo.ReconciliationExternalEntries REE ON REE.ReconciliationId = R.Id
		LEFT JOIN dbo.ExternalEntries EE ON REE.ExternalEntryId = EE.Id
		WHERE (EE.PostingDate IS NULL OR EE.PostingDate <=  @AsOfDate)
		AND (EE.[AccountId] IS NULL OR EE.[AccountId] = @AccountId)
		AND (EE.[CustodyId] IS NULL OR EE.[CustodyId] = @CustodyId)
	)
	AND L.[PostingDate] <= @AsOfDate
	ORDER BY L.[PostingDate], E.[MonetaryValue], E.[ExternalReference]
	OFFSET (@Skip) ROWS FETCH NEXT (@Top) ROWS ONLY;
	
	SELECT E.[Id], E.[PostingDate], E.[Direction], E.[MonetaryValue], E.[ExternalReference], E.[CreatedById], E.[CreatedAt], E.[ModifiedById], E.[ModifiedAt],
	CAST(IIF(E.[Id] IN (SELECT [ExternalEntryId] FROM dbo.ReconciliationExternalEntries), 1, 0) AS BIT) AS IsReconciledLater
	FROM dbo.ExternalEntries E
	WHERE E.[CustodyId] = @CustodyId
	AND E.[AccountId] = @AccountId
	AND E.[Id] NOT IN (
		SELECT DISTINCT REE.[ExternalEntryId]
		FROM dbo.ReconciliationExternalEntries REE
		JOIN dbo.Reconciliations R ON REE.ReconciliationId = R.Id
		-- LEFT JOIN to handle the case where an entry was added and reversed by the bank side only
		LEFT JOIN dbo.ReconciliationEntries RE ON RE.ReconciliationId = R.Id
		LEFT JOIN dbo.Entries E ON RE.EntryId = E.Id
		LEFT JOIN dbo.Lines L ON E.LineId = L.Id
		WHERE (L.PostingDate IS NULL OR L.PostingDate <= @AsOfDate)
		AND (E.[AccountId] IS NULL OR E.[AccountId] = @AccountId)
		AND (E.[CustodyId] IS NULL OR E.[CustodyId] = @CustodyId)
	)
	AND E.[PostingDate] <= @AsOfDate
	ORDER BY E.[PostingDate], E.[MonetaryValue], E.[ExternalReference]
	OFFSET (@SkipExternal) ROWS FETCH NEXT (@TopExternal) ROWS ONLY;