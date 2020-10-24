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
	AND L.[PostingDate] <= @AsOfDate

	SELECT @UnreconciledEntriesCount = COUNT(*), @UnreconciledEntriesBalance = SUM(E.[Direction] * E.[MonetaryValue])
	FROM dbo.Entries E
	JOIN dbo.Lines L ON E.[LineId] = L.[Id]
	WHERE E.[CustodyId] = @CustodyId
	AND E.[AccountId] = @AccountId
	AND L.[State] = 4
	AND E.[Id] NOT IN (SELECT [EntryId] FROM dbo.ReconciliationEntries)
	AND L.[PostingDate] <= @AsOfDate

	SELECT @UnreconciledExternalEntriesCount = COUNT(*), @UnreconciledExternalEntriesBalance = SUM(E.[Direction] * E.[MonetaryValue])
	FROM dbo.ExternalEntries E
	WHERE E.[CustodyId] = @CustodyId
	AND E.[AccountId] = @AccountId
	AND E.[Id] NOT IN (SELECT [ExternalEntryId] FROM dbo.ReconciliationExternalEntries)
	AND E.[PostingDate] <= @AsOfDate
	
	SELECT E.[Id], L.[PostingDate], E.[Direction], E.[MonetaryValue], E.[ExternalReference], L.[DocumentId], D.[DefinitionId] AS [DocumentDefinitionId], D.[SerialNumber] AS [DocumentSerialNumber]
	FROM dbo.Entries E
	JOIN dbo.Lines L ON E.[LineId] = L.[Id]
	JOIN dbo.Documents D ON L.[DocumentId] = D.[Id]
	WHERE E.[CustodyId] = @CustodyId
	AND E.[AccountId] = @AccountId
	AND L.[State] = 4
	AND E.[Id] NOT IN (SELECT [EntryId] FROM dbo.ReconciliationEntries)
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