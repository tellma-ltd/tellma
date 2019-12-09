CREATE VIEW [dbo].[DocumentLineEntriesDetailsView]
AS
	SELECT
		E.[Id],
		E.[LineId],
		L.[DocumentId],
		D.[DefinitionId] As DocumentDefinitionId,
		D.[SerialNumber],
		D.[DocumentDate],
		D.[VoucherNumericReference],
		D.[DocumentLookup1Id],
		D.[DocumentLookup2Id],
		D.[DocumentLookup3Id],
		D.[DocumentText1],
		D.[DocumentText2],
		--D.[Frequency],
		--D.[Repetitions],
		--D.[EndDate],
		L.[DefinitionId] As LineDefinitionId,
		L.[Memo],
		E.[EntryNumber],
		E.[Direction],
		E.[AccountId],		
		E.[ContractType],
		E.[AgentDefinitionId],
		E.[ResourceClassificationId],
		E.[IsCurrent],		
		E.[CurrencyId],
		E.[AgentId],
		E.[ResourceId],
		E.[ResponsibilityCenterId],
		E.[AccountIdentifier],
		E.[ResourceIdentifier],
		E.[DueDate],		
		E.[EntryClassificationId],
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
		[dbo].[Entries] E
		JOIN [dbo].[Lines] L ON E.[LineId] = L.Id
		JOIN [dbo].[Documents] D ON L.[DocumentId] = D.[Id]
		JOIN dbo.[DocumentDefinitions] DT ON D.[DefinitionId] = DT.[Id]
	WHERE
		D.[State] = N'Filed'
		AND L.[State] = +4; -- N'Reviewed';
GO;