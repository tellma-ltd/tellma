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
		L.[Memo],
		E.[Direction],
		E.[AccountId],
		R.[CurrencyId],
		E.[AgentRelationDefinitionId],
		E.[AgentId],
		E.[ResourceId],
		E.[EntryTypeId],
		E.[BatchCode],
		E.[DueDate],
		E.[MonetaryValue], -- normalization is already done in the Value and stored in the entry
		E.[Count], -- we can normalize every measure, but just showing a proof of concept
		E.[Mass],
		E.[Time],
		E.[Volume],
		E.[Value],
		E.[ExternalReference],
		E.[AdditionalReference],
		E.[RelatedAgentId],
		E.[RelatedAmount],
		E.[Time1],
		E.[Time2],
		E.[CreatedAt],
		E.[CreatedById],
		E.[ModifiedAt],
		E.[ModifiedById]
	FROM 
		[dbo].[DocumentLineEntries] E
		JOIN [dbo].[DocumentLines] L ON E.[DocumentLineId] = L.Id
		JOIN [dbo].[Documents] D ON L.[DocumentId] = D.[Id]
		JOIN dbo.[DocumentDefinitions] DT ON D.[DocumentDefinitionId] = DT.[Id]
		JOIN dbo.Resources R ON E.ResourceId = R.Id
	WHERE
		D.[State] = N'Filed'
		AND L.[State] = N'Reviewed';
GO;