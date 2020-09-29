CREATE PROCEDURE [dal].[Reconciliation__Load_Unreconciled]
	@AccountId	INT, 
	@CustodyId	INT, 
	@AsOfDate	DATE, 
	@Top		INT, 
	@Skip		INT
AS
	SELECT *
	FROM dbo.Entries E
	JOIN dbo.Lines L ON E.[LineId] = L.[Id]
	WHERE E.[CustodyId] = @CustodyId
	AND E.[AccountId] = @AccountId
	AND L.[State] = 4
	AND E.[Id] NOT IN (SELECT [EntryId] FROM dbo.ReconciliationEntries)
	ORDER BY L.[PostingDate], E.[MonetaryValue], E.[ExternalReference]
	OFFSET (@Skip) ROWS FETCH NEXT (@Top) ROWS ONLY;

	SELECT *
	FROM dbo.ExternalEntries E
	WHERE E.[CustodyId] = @CustodyId
	AND E.[AccountId] = @AccountId
	AND E.[Id] NOT IN (SELECT [ExternalEntryId] FROM dbo.ReconciliationExternalEntries)
	ORDER BY E.[PostingDate], E.[MonetaryValue], E.[ExternalReference]
	OFFSET (@Skip) ROWS FETCH NEXT (@Top) ROWS ONLY;