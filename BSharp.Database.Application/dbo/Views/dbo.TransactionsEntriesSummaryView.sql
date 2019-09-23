CREATE VIEW [dbo].[TransactionsEntriesSummaryView]
AS
	SELECT
		ROW_NUMBER() OVER(ORDER BY L.[DocumentId],
			SUM(E.[Direction] * E.[Value]) DESC) AS [Id],
		L.[DocumentId],
		D.[DocumentDefinitionId],
		D.[SerialNumber],
		D.[DocumentDate],
		D.[VoucherNumericReference],
		D.[DocumentLookup1Id],
		D.[DocumentLookup2Id],
		D.[DocumentLookup3Id],
		D.[DocumentText1],
		D.[DocumentText2],
		D.[Frequency],
		D.[Repetitions],
		D.[EndDate],
		D.[CreatedAt],
		D.[CreatedById],
		D.[ModifiedAt],
		D.[ModifiedById],
		CASE WHEN SUM(E.[Direction] * E.[Value]) > 0 THEN 1 ELSE -1 END AS [Direction],
		E.[AccountId],
		E.[IfrsEntryClassificationId],
		E.[ResponsibilityCenterId],
		E.[ResourceId],
		E.[ResourcePickId],
		E.[BatchCode],
		SUM(E.[Direction] * E.[MonetaryValue]) AS [MonetaryValue],
		SUM(E.[Direction] * E.[Mass]) AS [Mass],
		SUM(E.[Direction] * E.[Volume]) AS [Volume],
		SUM(E.[Direction] * E.[Area]) AS [Area],
		SUM(E.[Direction] * E.[Length]) AS [Length],
		SUM(E.[Direction] * E.[Time]) AS [Time],
		SUM(E.[Direction] * E.[Count]) AS [Count], -- we can normalize every measure, but just showing a proof of concept
		SUM(E.[Direction] * E.[Value]) AS [Value],
		L.[Memo],
		L.[ExternalReference],
		L.[AdditionalReference],
		L.[RelatedResourceId],
		L.[RelatedAccountId],
		SUM(E.[Direction] * L.[RelatedQuantity]) AS [RelatedQuantity],
		SUM(E.[Direction] * L.[RelatedMoneyAmount]) AS [RelatedMoneyAmount]
	FROM 
		[dbo].[DocumentLineEntries] E
		JOIN [dbo].[DocumentLines] L ON E.[DocumentLineId] = L.Id
		JOIN [dbo].[Documents] D ON L.[DocumentId] = D.[Id]
		JOIN dbo.[DocumentDefinitions] DT ON D.[DocumentDefinitionId] = DT.[Id]
		JOIN dbo.[Accounts] A ON E.AccountId = A.[Id]
	WHERE
		D.[State] = N'Posted'
	GROUP BY
		L.[DocumentId],
		D.[DocumentDefinitionId],
		D.[SerialNumber],
		D.[DocumentDate],
		D.[VoucherNumericReference],
		D.[DocumentLookup1Id],
		D.[DocumentLookup2Id],
		D.[DocumentLookup3Id],
		D.[DocumentText1],
		D.[DocumentText2],
		D.[Frequency],
		D.[Repetitions],
		D.[EndDate],
		D.[CreatedAt],
		D.[CreatedById],
		D.[ModifiedAt],
		D.[ModifiedById],
		E.[AccountId],
		E.[IfrsEntryClassificationId],
		E.[ResponsibilityCenterId],
		E.[ResourceId],
		E.[ResourcePickId],
		E.[BatchCode],
		L.[Memo],
		L.[ExternalReference],
		L.[AdditionalReference],
		L.[RelatedResourceId],
		L.[RelatedAccountId]
	HAVING
		SUM(E.[Direction] * E.[Value]) <> 0
	;
GO;