CREATE PROCEDURE [dal].[Reconciliation__Load_Reconciled]
	@AccountId					INT, 
	@AgentId					INT, 
	@FromDate					DATE,
	@ToDate						DATE,
	@FromAmount					DECIMAL (19, 4),
	@ToAmount					DECIMAL (19, 4),
	@ExternalReferenceContains	NVARCHAR (50),
	@Top						INT, 
	@Skip						INT,
	@ReconciledCount			INT OUTPUT
AS
	DECLARE @ReconciliationIds IdList;

	SELECT @ReconciledCount = COUNT(DISTINCT R.[Id])
	FROM dbo.Reconciliations  R
	JOIN dbo.ReconciliationExternalEntries REE ON R.[Id] = REE.ReconciliationId
	JOIN dbo.ExternalEntries EE ON REE.[ExternalEntryId] = EE.[Id]
	WHERE EE.[AgentId] = @AgentId
	AND EE.[AccountId] = @AccountId
	AND (@ToDate IS NULL OR PostingDate >= @FromDate)
	AND (@ToDate IS NULL OR PostingDate <= @ToDate)
	AND (@FromAmount IS NULL OR EE.[MonetaryValue] >= @FromAmount)
	AND (@ToAmount IS NULL OR EE.[MonetaryValue] <= @ToAmount)
	AND (@ExternalReferenceContains IS NULL OR EE.ExternalReference LIKE N'%' + @ExternalReferenceContains + N'%')

	INSERT INTO @ReconciliationIds
	SELECT DISTINCT [Id] FROM (
	SELECT R.[Id], EE.[PostingDate], EE.[MonetaryValue], EE.[ExternalReference]
	FROM dbo.Reconciliations  R
	JOIN dbo.ReconciliationExternalEntries REE ON R.[Id] = REE.ReconciliationId
	JOIN dbo.ExternalEntries EE ON REE.[ExternalEntryId] = EE.[Id]
	WHERE EE.[AgentId] = @AgentId
	AND EE.[AccountId] = @AccountId
	AND (@ToDate IS NULL OR PostingDate >= @FromDate)
	AND (@ToDate IS NULL OR PostingDate <= @ToDate)
	AND (@FromAmount IS NULL OR EE.[MonetaryValue] >= @FromAmount)
	AND (@ToAmount IS NULL OR EE.[MonetaryValue] <= @ToAmount)
	AND (@ExternalReferenceContains IS NULL OR EE.ExternalReference LIKE N'%' + @ExternalReferenceContains + N'%')
	ORDER BY EE.[PostingDate], EE.[MonetaryValue], EE.[ExternalReference]
	OFFSET (@Skip) ROWS FETCH NEXT (@Top) ROWS ONLY
	) T;

	SELECT *
	FROM dbo.Reconciliations
	WHERE [Id] IN (SELECT [Id] FROM @ReconciliationIds);

	-- Select the Entries
	SELECT R.[ReconciliationId], E.[Id], L.[PostingDate], E.[Direction], E.[MonetaryValue], 
	IIF([Direction] = 1, E.[NotedAgentName], E.[InternalReference]) AS ExternalReference,
	L.[DocumentId], D.[DefinitionId] AS [DocumentDefinitionId], D.[SerialNumber] AS [DocumentSerialNumber]
	FROM dbo.Entries E
	JOIN dbo.Lines L ON E.[LineId] = L.[Id]
	JOIN dbo.Documents D ON L.[DocumentId] = D.[Id]
	INNER JOIN dbo.ReconciliationEntries R ON R.[EntryId] = E.[Id]
	WHERE R.[ReconciliationId] IN (SELECT [Id] FROM @ReconciliationIds)
	AND (@ToDate IS NULL OR L.PostingDate >= @FromDate)
	AND (@ToDate IS NULL OR L.PostingDate <= @ToDate)
	-- Select the External Entries
	SELECT R.[ReconciliationId], E.[Id], E.[PostingDate], E.[Direction], E.[MonetaryValue], E.[ExternalReference], E.[CreatedById], E.[CreatedAt], E.[ModifiedById], E.[ModifiedAt]
	FROM dbo.ExternalEntries E
	INNER JOIN dbo.ReconciliationExternalEntries R ON R.[ExternalEntryId] = E.[Id]
	WHERE R.[ReconciliationId] IN (SELECT [Id] FROM @ReconciliationIds)