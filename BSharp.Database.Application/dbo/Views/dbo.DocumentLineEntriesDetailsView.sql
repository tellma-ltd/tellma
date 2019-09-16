CREATE VIEW [dbo].[DocumentLineEntriesDetailsView]
AS
	SELECT
		E.[Id],
		E.[DocumentLineId],
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
		L.[LineTypeId],
		E.[Direction],
		E.[AccountId],
		A.[AccountClassificationId],
		A.[IfrsAccountClassificationId],
		A.[AgentId],
		A.[ResponsibilityCenterId],
		E.[IfrsEntryClassificationId],
		E.[ResourceId],
		E.[ResourcePickId],
		E.[BatchCode],
		E.[DueDate],
		E.[MonetaryValue], -- normalization is already done in the Value and stored in the entry
		E.[Mass],
		E.[Volume],
		E.[Area],
		E.[Length],
		E.[Time],
		E.[Count], -- we can normalize every measure, but just showing a proof of concept
		E.[Value],
		E.[Memo],
		E.[ExternalReference],
		E.[AdditionalReference],
		E.[RelatedResourceId],
		E.[RelatedAccountId],
		E.[RelatedQuantity],
		E.[RelatedMoneyAmount],

		E.[CreatedAt],
		E.[CreatedById],
		E.[ModifiedAt],
		E.[ModifiedById]
	FROM 
		[dbo].[DocumentLineEntries] E
		JOIN [dbo].[DocumentLines] L ON E.[DocumentLineId] = L.Id
		JOIN [dbo].[Documents] D ON L.[DocumentId] = D.[Id]
		JOIN dbo.[DocumentDefinitions] DT ON D.[DocumentDefinitionId] = DT.[Id]
		JOIN [dbo].[Accounts] A ON E.[AccountId] = A.[Id]
	WHERE
		D.[State] = N'Posted';
GO;