CREATE PROCEDURE [bll].[LineDefinitionEntries__Pivot]
	@Index INT,
	@DocumentIndex INT,
	@DefinitionId NVARCHAR (50)
AS
	DECLARE @WideLines dbo.WideLineList;

	INSERT INTO @WideLines([Index], [DocumentIndex],[DefinitionId])
	SELECT					@Index, @DocumentIndex , @DefinitionId 
	FROM dbo.LineDefinitions
	WHERE [Id] = @DefinitionId

	UPDATE WL
	SET
		WL.[Direction0]					= LDE.[Direction]
		--WL.[AgentId0]					= LDE.[AgentId],
		--WL.[ResourceId0]				= LDE.[ResourceId],
		--WL.[ResponsibilityCenterId0]	= LDE.[ResponsibilityCenterId],
		--WL.[AccountIdentifier0]			= LDE.[AccountIdentifier],
		--WL.[ResourceIdentifier0]		= LDE.[ResourceIdentifier],
		--WL.[CurrencyId0]				= LDE.[CurrencyId],
		--WL.[EntryTypeId0]				= LDE.[EntryTypeId],
		--WL.[DueDate0]					= LDE.[DueDate],
		--WL.[MonetaryValue0]				= LDE.[MonetaryValue],
		--WL.[Count0]						= LDE.[Count],
		--WL.[Mass0]						= LDE.[Mass],
		--WL.[Volume0]					= LDE.[Volume],
		--WL.[Time0]						= LDE.[Time],
		--WL.[Value0]						= LDE.[Value],
		--WL.[Time10]						= LDE.[Time1],
		--WL.[Time20]						= LDE.[Time2],
		--WL.[ExternalReference0]			= LDE.[ExternalReference],
		--WL.[AdditionalReference0]		= LDE.[AdditionalReference],
		--WL.[NotedAgentId0]				= LDE.[NotedAgentId],
		--WL.[NotedAgentName0]			= LDE.[NotedAgentName],
		--WL.[NotedAmount0]				= LDE.[NotedAmount],
		--WL.[NotedDate0]					= LDE.[NotedDate]
	FROM @WideLines AS WL JOIN dbo.LineDefinitionEntries LDE ON WL.DefinitionId = LDE.[LineDefinitionId]
	WHERE LDE.[Index] = 0

	UPDATE WL
	SET
		WL.[Direction1]					= LDE.[Direction]
		--WL.[AgentId1]					= LDE.[AgentId],
		--WL.[ResourceId1]				= LDE.[ResourceId],
		--WL.[ResponsibilityCenterId1]	= LDE.[ResponsibilityCenterId],
		--WL.[AccountIdentifier1]			= LDE.[AccountIdentifier],
		--WL.[ResourceIdentifier1]		= LDE.[ResourceIdentifier],
		--WL.[CurrencyId1]				= LDE.[CurrencyId],
		--WL.[EntryTypeId1]				= LDE.[EntryTypeId],
		--WL.[DueDate1]					= LDE.[DueDate],
		--WL.[MonetaryValue1]				= LDE.[MonetaryValue],
		--WL.[Count1]						= LDE.[Count],
		--WL.[Mass1]						= LDE.[Mass],
		--WL.[Volume1]					= LDE.[Volume],
		--WL.[Time1]						= LDE.[Time],
		--WL.[Value1]						= LDE.[Value],
		--WL.[Time11]						= LDE.[Time1],
		--WL.[Time21]						= LDE.[Time2],
		--WL.[ExternalReference1]			= LDE.[ExternalReference],
		--WL.[AdditionalReference1]		= LDE.[AdditionalReference],
		--WL.[NotedAgentId1]				= LDE.[NotedAgentId],
		--WL.[NotedAgentName1]			= LDE.[NotedAgentName],
		--WL.[NotedAmount1]				= LDE.[NotedAmount],
		--WL.[NotedDate1]					= LDE.[NotedDate]
	FROM @WideLines AS WL JOIN dbo.LineDefinitionEntries LDE ON WL.DefinitionId = LDE.[LineDefinitionId]
	WHERE LDE.[Index] = 1

	UPDATE WL
	SET
		WL.[Direction2]					= LDE.[Direction]
		--WL.[AgentId2]					= LDE.[AgentId],
		--WL.[ResourceId2]				= LDE.[ResourceId],
		--WL.[ResponsibilityCenterId2]	= LDE.[ResponsibilityCenterId],
		--WL.[AccountIdentifier2]			= LDE.[AccountIdentifier],
		--WL.[ResourceIdentifier2]		= LDE.[ResourceIdentifier],
		--WL.[CurrencyId2]				= LDE.[CurrencyId],
		--WL.[EntryTypeId2]				= LDE.[EntryTypeId],
		--WL.[DueDate2]					= LDE.[DueDate],
		--WL.[MonetaryValue2]				= LDE.[MonetaryValue],
		--WL.[Count2]						= LDE.[Count],
		--WL.[Mass2]						= LDE.[Mass],
		--WL.[Volume2]					= LDE.[Volume],
		--WL.[Time2]						= LDE.[Time],
		--WL.[Value2]						= LDE.[Value],
		--WL.[Time12]						= LDE.[Time2],
		--WL.[Time22]						= LDE.[Time2],
		--WL.[ExternalReference2]			= LDE.[ExternalReference],
		--WL.[AdditionalReference2]		= LDE.[AdditionalReference],
		--WL.[NotedAgentId2]				= LDE.[NotedAgentId],
		--WL.[NotedAgentName2]			= LDE.[NotedAgentName],
		--WL.[NotedAmount2]				= LDE.[NotedAmount],
		--WL.[NotedDate2]					= LDE.[NotedDate]
	FROM @WideLines AS WL JOIN dbo.LineDefinitionEntries LDE ON WL.DefinitionId = LDE.[LineDefinitionId]
	WHERE LDE.[Index] = 2

	SELECT * FROM @WideLines;