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
		L.[LineDefinitionId],
		E.[Direction],
		E.[AccountId],
		E.[EntryTypeId],
	--	E.[ResourceInstanceId],
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
		E.[RelatedAgentId],
		E.[RelatedQuantity],
		E.[RelatedMonetaryAmount],

		E.[CreatedAt],
		E.[CreatedById],
		E.[ModifiedAt],
		E.[ModifiedById]
	FROM 
		[dbo].[DocumentLineEntries] E
		JOIN [dbo].[DocumentLines] L ON E.[DocumentLineId] = L.Id
		JOIN [dbo].[Documents] D ON L.[DocumentId] = D.[Id]
		JOIN dbo.[DocumentDefinitions] DT ON D.[DocumentDefinitionId] = DT.[Id]
	WHERE
		D.[State] = N'Posted';
GO;