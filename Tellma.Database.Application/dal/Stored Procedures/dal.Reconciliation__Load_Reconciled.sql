CREATE PROCEDURE [dal].[Reconciliation__Load_Reconciled]
	@AccountId					INT, 
	@CustodyId					INT, 
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

	SELECT @ReconciledCount = COUNT(DISTINCT RE.[ReconciliationId])
	FROM dbo.ReconciliationEntries RE
	JOIN dbo.Entries E ON RE.[EntryId] = E.[Id]
	JOIN dbo.Lines L ON E.[LineId] = L.[Id]
	WHERE E.[CustodyId] = @CustodyId
	AND E.[AccountId] = @AccountId
	AND L.[State] = 4
	AND E.[Id] IN (SELECT [EntryId] FROM dbo.ReconciliationEntries)
	AND (@ToDate IS NULL OR L.PostingDate >= @FromDate)
	AND (@ToDate IS NULL OR L.PostingDate <= @ToDate)
	AND (@FromAmount IS NULL OR E.[MonetaryValue] >= @FromAmount)
	AND (@ToAmount IS NULL OR E.[MonetaryValue] <= @ToAmount)
	AND (@ExternalReferenceContains IS NULL OR E.ExternalReference LIKE N'%' + @ExternalReferenceContains + N'%')

	INSERT INTO @ReconciliationIds
	SELECT RE.[ReconciliationId]
	FROM dbo.ReconciliationEntries RE
	JOIN dbo.Entries E ON RE.[EntryId] = E.[Id]
	JOIN dbo.Lines L ON E.[LineId] = L.[Id]
	WHERE E.[CustodyId] = @CustodyId
	AND E.[AccountId] = @AccountId
	AND L.[State] = 4
	AND E.[Id] IN (SELECT [EntryId] FROM dbo.ReconciliationEntries)
	AND (@ToDate IS NULL OR L.PostingDate >= @FromDate)
	AND (@ToDate IS NULL OR L.PostingDate <= @ToDate)
	AND (@FromAmount IS NULL OR E.[MonetaryValue] >= @FromAmount)
	AND (@ToAmount IS NULL OR E.[MonetaryValue] <= @ToAmount)
	AND (@ExternalReferenceContains IS NULL OR E.ExternalReference LIKE N'%' + @ExternalReferenceContains + N'%')
	ORDER BY L.[PostingDate], E.[MonetaryValue], E.[ExternalReference]
	OFFSET (@Skip) ROWS FETCH NEXT (@Top) ROWS ONLY;

	SELECT *
	FROM dbo.Reconciliations
	WHERE [Id] IN (SELECT [Id] FROM @ReconciliationIds);

	-- Select the Entries
	SELECT R.[ReconciliationId], E.[Id], L.[PostingDate], E.[Direction], E.[MonetaryValue], E.[ExternalReference]
	FROM dbo.Entries E
	JOIN dbo.Lines L ON E.[LineId] = L.[Id]
	INNER JOIN dbo.ReconciliationEntries R ON R.[EntryId] = E.[Id]
	WHERE R.[ReconciliationId] IN (SELECT [Id] FROM @ReconciliationIds)

	-- Select the External Entries
	SELECT R.[ReconciliationId], E.[Id], E.[PostingDate], E.[Direction], E.[MonetaryValue], E.[ExternalReference], E.[CreatedById], E.[CreatedAt], E.[ModifiedById], E.[ModifiedAt]
	FROM dbo.ExternalEntries E
	INNER JOIN dbo.ReconciliationExternalEntries R ON R.[ExternalEntryId] = E.[Id]
	WHERE R.[ReconciliationId] IN (SELECT [Id] FROM @ReconciliationIds)