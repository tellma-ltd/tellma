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

	With ReconciledEntriesAsOfDate AS (
		SELECT DISTINCT RE.[EntryId]
		FROM dbo.ReconciliationEntries RE
		JOIN dbo.Reconciliations R ON RE.ReconciliationId = R.Id
		JOIN dbo.ReconciliationExternalEntries REE ON REE.ReconciliationId = R.Id
		JOIN dbo.ExternalEntries EE ON REE.ExternalEntryId = EE.Id
		WHERE EE.PostingDate <=  @AsOfDate	
		AND EE.[AccountId] = @AccountId
		AND EE.[CustodyId] = @CustodyId
	),
	WhollyReversedEntriesAsOfDate AS (
		SELECT DISTINCT RE.[ReconciliationId]
		FROM dbo.ReconciliationEntries RE
		JOIN dbo.Entries E ON RE.EntryId = E.[Id]
		JOIN dbo.Lines L ON E.[LineId] = L.[Id]
		WHERE L.PostingDate <= @AsOfDate
		AND	E.[AccountId] = @AccountId
		AND E.[CustodyId] = @CustodyId
		AND RE.ReconciliationId NOT IN (SELECT ReconciliationId FROM dbo.ReconciliationExternalEntries)
		EXCEPT
		SELECT DISTINCT RE.[ReconciliationId]
		FROM dbo.ReconciliationEntries RE
		JOIN dbo.Entries E ON RE.EntryId = E.[Id]
		JOIN dbo.Lines L ON E.[LineId] = L.[Id]
		WHERE L.PostingDate > @AsOfDate
		AND	E.[AccountId] = @AccountId
		AND E.[CustodyId] = @CustodyId
	)
	SELECT
		@UnreconciledEntriesCount = COUNT(*),
		@UnreconciledEntriesBalance = SUM(E.[Direction] * E.[MonetaryValue])
	FROM dbo.Entries E
	JOIN dbo.Lines L ON E.[LineId] = L.[Id]
	WHERE E.[CustodyId] = @CustodyId
	AND E.[AccountId] = @AccountId
	AND L.[State] = 4
	AND	L.[PostingDate] <= @AsOfDate
	-- Exclude if it was reconciled with an external entry before AsOfDate
	AND E.[Id] NOT IN (SELECT EntryId FROM ReconciledEntriesAsOfDate)
	-- Exclude if it was reversed with an internal entry before AsOfDate
	AND E.[Id] NOT IN (
		SELECT EntryId FROM dbo.ReconciliationEntries 
		WHERE ReconciliationId IN (SELECT [ReconciliationId] FROM WhollyReversedEntriesAsOfDate)
	);

	With ReconciledExternalEntriesAsOfDate AS (
		SELECT DISTINCT REE.[ExternalEntryId]
		FROM dbo.ReconciliationExternalEntries REE
		JOIN dbo.Reconciliations R ON REE.ReconciliationId = R.Id
		JOIN dbo.ReconciliationEntries RE ON RE.ReconciliationId = R.Id
		JOIN dbo.Entries E ON RE.EntryId = E.Id
		JOIN dbo.Lines L ON L.[Id] = E.[LineId]
		WHERE L.PostingDate <=  @AsOfDate	
		AND E.[AccountId] = @AccountId
		AND E.[CustodyId] = @CustodyId
	),
	WhollyReversedExternalEntriesAsOfDate AS (
		SELECT DISTINCT REE.[ReconciliationId]
		FROM dbo.ReconciliationExternalEntries REE
		JOIN dbo.ExternalEntries EE ON REE.ExternalEntryId = EE.[Id]
		WHERE EE.PostingDate <= @AsOfDate
		AND	EE.[AccountId] = @AccountId
		AND EE.[CustodyId] = @CustodyId
		AND REE.ReconciliationId NOT IN (SELECT ReconciliationId FROM dbo.ReconciliationEntries)
		EXCEPT
		SELECT DISTINCT REE.[ReconciliationId]
		FROM dbo.ReconciliationExternalEntries REE
		JOIN dbo.ExternalEntries EE ON REE.ExternalEntryId = EE.[Id]
		WHERE EE.PostingDate > @AsOfDate
		AND	EE.[AccountId] = @AccountId
		AND EE.[CustodyId] = @CustodyId
	)
	SELECT
		@UnreconciledExternalEntriesCount = COUNT(*),
		@UnreconciledExternalEntriesBalance = SUM(EE.[Direction] * EE.[MonetaryValue])
	FROM dbo.ExternalEntries EE
	WHERE EE.[CustodyId] = @CustodyId
	AND EE.[AccountId] = @AccountId
	AND	EE.[PostingDate] <= @AsOfDate
	AND EE.[Id] NOT IN (SELECT [ExternalEntryId] FROM ReconciledExternalEntriesAsOfDate)
	AND EE.[Id] NOT IN (
		SELECT ExternalEntryId FROM dbo.ReconciliationExternalEntries 
		WHERE ReconciliationId IN (SELECT [ReconciliationId] FROM WhollyReversedExternalEntriesAsOfDate)
	);
	
	With ReconciledEntriesAsOfDate AS (
		SELECT DISTINCT RE.[EntryId]
		FROM dbo.ReconciliationEntries RE
		JOIN dbo.Reconciliations R ON RE.ReconciliationId = R.Id
		JOIN dbo.ReconciliationExternalEntries REE ON REE.ReconciliationId = R.Id
		JOIN dbo.ExternalEntries EE ON REE.ExternalEntryId = EE.Id
		WHERE EE.PostingDate <=  @AsOfDate	
		AND EE.[AccountId] = @AccountId
		AND EE.[CustodyId] = @CustodyId
	),
	WhollyReversedEntriesAsOfDate AS (
		SELECT DISTINCT RE.[ReconciliationId]
		FROM dbo.ReconciliationEntries RE
		JOIN dbo.Entries E ON RE.EntryId = E.[Id]
		JOIN dbo.Lines L ON E.[LineId] = L.[Id]
		WHERE L.PostingDate <= @AsOfDate
		AND	E.[AccountId] = @AccountId
		AND E.[CustodyId] = @CustodyId
		AND RE.ReconciliationId NOT IN (SELECT ReconciliationId FROM dbo.ReconciliationExternalEntries)
		EXCEPT
		SELECT DISTINCT RE.[ReconciliationId]
		FROM dbo.ReconciliationEntries RE
		JOIN dbo.Entries E ON RE.EntryId = E.[Id]
		JOIN dbo.Lines L ON E.[LineId] = L.[Id]
		WHERE L.PostingDate > @AsOfDate
		AND	E.[AccountId] = @AccountId
		AND E.[CustodyId] = @CustodyId
	)
	SELECT E.[Id], L.[PostingDate], E.[Direction], E.[MonetaryValue],
		IIF([Direction] = 1, E.[NotedAgentName], E.[InternalReference]) AS ExternalReference,
		L.[DocumentId], D.[DefinitionId] AS [DocumentDefinitionId], D.[SerialNumber] AS [DocumentSerialNumber],
			CAST(IIF(E.[Id] IN (SELECT [EntryId] FROM dbo.ReconciliationEntries), 1, 0) AS BIT) AS IsReconciledLater
	FROM dbo.Entries E
	JOIN dbo.Lines L ON E.[LineId] = L.[Id]
	JOIN dbo.Documents D ON L.[DocumentId] = D.[Id]
	WHERE E.[CustodyId] = @CustodyId
	AND E.[AccountId] = @AccountId
	AND L.[State] = 4
	AND L.[PostingDate] <= @AsOfDate
	-- Exclude if it was reconciled with an external entry before AsOfDate
	AND E.[Id] NOT IN (SELECT EntryId FROM ReconciledEntriesAsOfDate)
	-- Exclude if it was reversed with an internal entry before AsOfDate
	AND E.[Id] NOT IN (
			SELECT EntryId FROM dbo.ReconciliationEntries 
			WHERE ReconciliationId IN (SELECT [ReconciliationId] FROM WhollyReversedEntriesAsOfDate)
	)
	ORDER BY L.[PostingDate], E.[MonetaryValue], E.[ExternalReference]
	OFFSET (@Skip) ROWS FETCH NEXT (@Top) ROWS ONLY;

	With ReconciledExternalEntriesAsOfDate AS (
		SELECT DISTINCT REE.[ExternalEntryId]
		FROM dbo.ReconciliationExternalEntries REE
		JOIN dbo.Reconciliations R ON REE.ReconciliationId = R.Id
		JOIN dbo.ReconciliationEntries RE ON RE.ReconciliationId = R.Id
		JOIN dbo.Entries E ON RE.EntryId = E.Id
		JOIN dbo.Lines L ON L.[Id] = E.[LineId]
		WHERE L.PostingDate <=  @AsOfDate
		AND E.[AccountId] = @AccountId
		AND E.[CustodyId] = @CustodyId
	),
	WhollyReversedExternalEntriesAsOfDate AS (
		SELECT DISTINCT REE.[ReconciliationId]
		FROM dbo.ReconciliationExternalEntries REE
		JOIN dbo.ExternalEntries EE ON REE.ExternalEntryId = EE.[Id]
		WHERE EE.PostingDate <= @AsOfDate
		AND	EE.[AccountId] = @AccountId
		AND EE.[CustodyId] = @CustodyId
		AND REE.ReconciliationId NOT IN (SELECT ReconciliationId FROM dbo.ReconciliationEntries)
		EXCEPT
		SELECT DISTINCT REE.[ReconciliationId]
		FROM dbo.ReconciliationExternalEntries REE
		JOIN dbo.ExternalEntries EE ON REE.ExternalEntryId = EE.[Id]
		WHERE EE.PostingDate > @AsOfDate
		AND	EE.[AccountId] = @AccountId
		AND EE.[CustodyId] = @CustodyId
	)
	SELECT EE.[Id], EE.[PostingDate], EE.[Direction], EE.[MonetaryValue],
		EE.[ExternalReference],
		EE.[CreatedById], EE.[CreatedAt], EE.[ModifiedById], EE.[ModifiedAt],
		CAST(IIF(EE.[Id] IN (SELECT [ExternalEntryId] FROM dbo.ReconciliationExternalEntries), 1, 0) AS BIT) AS IsReconciledLater
	FROM dbo.ExternalEntries EE
	WHERE EE.[CustodyId] = @CustodyId
	AND EE.[AccountId] = @AccountId
	AND EE.[AccountId] = @AccountId
	AND	EE.[PostingDate] <= @AsOfDate
	-- Exclude if it was reconciled with internal entry before as of date
	AND EE.[Id] NOT IN (SELECT [ExternalEntryId] FROM ReconciledExternalEntriesAsOfDate)
	-- exclude if it was reconciled with external 
	AND EE.[Id] NOT IN (
		SELECT ExternalEntryId FROM dbo.ReconciliationExternalEntries 
		WHERE ReconciliationId IN (SELECT [ReconciliationId] FROM WhollyReversedExternalEntriesAsOfDate)
	)
	ORDER BY EE.[PostingDate], EE.[MonetaryValue], EE.[ExternalReference]
	OFFSET (@SkipExternal) ROWS FETCH NEXT (@TopExternal) ROWS ONLY;