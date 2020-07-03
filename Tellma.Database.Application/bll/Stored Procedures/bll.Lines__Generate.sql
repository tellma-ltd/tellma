CREATE PROCEDURE [bll].[Lines__Generate]
	@LineDefinitionId INT,
	@GenerateArguments [GenerateArgumentList] READONLY
AS
	SET NOCOUNT ON;
	DECLARE @Script NVARCHAR (MAX);
	DECLARE @WideLines WideLineList;
	DECLARE @Lines LineList, @Entries EntryList;
	SELECT @Script = [GenerateScript] FROM dbo.LineDefinitions WHERE [Id] = @LineDefinitionId;

	INSERT INTO @WideLines
	EXECUTE	sp_executesql @Script, N'@GenerateArguments [GenerateArgumentList] READONLY',
			@GenerateArguments = @GenerateArguments;

	UPDATE @WideLines SET DefinitionId =  @LineDefinitionId
	
	INSERT INTO @Lines([Index], [DefinitionId], [PostingDate], [Memo])
	SELECT [Index], @LineDefinitionId, [PostingDate], [Memo] FROM @WideLines

	INSERT INTO @Entries
	EXEC [bll].[WideLines__Unpivot] @WideLines;

	SELECT
		[L].[DefinitionId],
		[L].[PostingDate],
		--[L].[TemplateLineId],
		--[L].[Multiplier],
		[L].[Memo],
		[L].[Index]
	FROM @Lines AS [L] -- LineList
	ORDER BY [L].[Index] ASC

	SELECT
	-- Entry
	[E].[AccountId],
	[E].[CurrencyId],
	[E].[ResourceId],
	[E].[ContractId],
	[E].[EntryTypeId],
	[E].[NotedContractId],
	[E].[CenterId],
	[E].[UnitId],
	[E].[IsSystem],
	[E].[Direction],
	[E].[DueDate],
	[E].[MonetaryValue],
	[E].[Quantity],
	[E].[Value],
	[E].[Time1],
	[E].[Time2],
	[E].[ExternalReference],
	[E].[AdditionalReference],
	[E].[NotedAgentName],
	[E].[NotedAmount],
	[E].[NotedDate],
	[E].[Index],
	[E].[LineIndex],
	-- Related Stuff
	[A].[Name], [A].[Id], [A].[AccountTypeId], [A].[CenterId], [A].[EntryTypeId], [A].[CurrencyId], [A].[ContractId], [A].[ResourceId],
	[A].[Name2], [A].[Name3], [A].[Code], [A].[ContractDefinitionId], [A].[NotedContractDefinitionId], [A].[ResourceDefinitionId], [AT].[Name], [AT].[Id], [AT].[EntryTypeParentId], [AT].[Name2], [AT].[Name3],
	[AT].[DueDateLabel], [AT].[DueDateLabel2], [AT].[DueDateLabel3], [AT].[Time1Label], [AT].[Time1Label2], [AT].[Time1Label3], [AT].[Time2Label], [AT].[ExternalReferenceLabel], [AT].[AdditionalReferenceLabel],
	[AT].[NotedAgentNameLabel], [AT].[NotedAmountLabel], [AT].[NotedDateLabel], [P3].[IsActive], [P3].[Id], [P4].[Name], [P4].[Id], [P4].[Name2], [P4].[Name3], [P5].[Name], [P5].[Id], [P5].[Name2], [P5].[Name3],
	[P5].[IsActive], [P6].[Name], [P6].[Id], [P6].[Name2], [P6].[Name3], [P6].[E], [P7].[Name], [P7].[Id], [P7].[Name2], [P7].[Name3], [P7].[DefinitionId], [P8].[Name], [P8].[Id], [P8].[Name2], [P8].[Name3],
	[P8].[DefinitionId], [P9].[Name], [P9].[Id], [P9].[Name2], [P9].[Name3], [P9].[E], [P10].[Name], [P10].[Id], [P10].[CurrencyId], [P10].[CenterId], [P10].[Name2], [P10].[Name3], [P10].[DefinitionId], [P11].[Name],
	[P11].[Id], [P11].[Name2], [P11].[Name3], [P11].[E], [P12].[Name], [P12].[Id], [P12].[Name2], [P12].[Name3], [P13].[Name], [P13].[Id], [P13].[CurrencyId], [P13].[CenterId], [P13].[Name2], [P13].[Name3],
	[P13].[DefinitionId], [P14].[Name], [P14].[Id], [P14].[Name2], [P14].[Name3], [P14].[E], [P15].[Name], [P15].[Id], [P15].[Name2], [P15].[Name3], [P16].[Name], [P16].[Id], [P16].[Name2], [P16].[Name3],
	[P16].[IsActive], [P17].[Name], [P17].[Id], [P17].[Name2], [P17].[Name3], [P17].[DefinitionId], [P18].[Name], [P18].[Id], [P18].[Name2], [P18].[Name3], [U].[Name], [U].[Id], [U].[Name2], [U].[Name3]
	FROM @Entries AS [E]
	LEFT JOIN [map].[Accounts]() [A] ON [E].[AccountId] = [A].[Id]
	LEFT JOIN [map].[AccountTypes]() [AT] ON [A].[AccountTypeId] = [AT].[Id]
	LEFT JOIN [map].[EntryTypes]() [P3] ON [AT].[EntryTypeParentId] = [P3].[Id]
	LEFT JOIN [map].[Centers]() [P4] ON [A].[CenterId] = [P4].[Id]
	LEFT JOIN [map].[EntryTypes]() [P5] ON [A].[EntryTypeId] = [P5].[Id]
	LEFT JOIN [map].[Currencies]() [P6] ON [A].[CurrencyId] = [P6].[Id]
	LEFT JOIN [map].[Contracts]() [P7] ON [A].[ContractId] = [P7].[Id]
	LEFT JOIN [map].[Resources]() [P8] ON [A].[ResourceId] = [P8].[Id]
	LEFT JOIN [map].[Currencies]() [P9] ON [E].[CurrencyId] = [P9].[Id]
	LEFT JOIN [map].[Resources]() [P10] ON [E].[ResourceId] = [P10].[Id]
	LEFT JOIN [map].[Currencies]() [P11] ON [P10].[CurrencyId] = [P11].[Id]
	LEFT JOIN [map].[Centers]() [P12] ON [P10].[CenterId] = [P12].[Id]
	LEFT JOIN [map].[Contracts]() [P13] ON [E].[ContractId] = [P13].[Id]
	LEFT JOIN [map].[Currencies]() [P14] ON [P13].[CurrencyId] = [P14].[Id]
	LEFT JOIN [map].[Centers]() [P15] ON [P13].[CenterId] = [P15].[Id]
	LEFT JOIN [map].[EntryTypes]() [P16] ON [E].[EntryTypeId] = [P16].[Id]
	LEFT JOIN [map].[Contracts]() [P17] ON [E].[NotedContractId] = [P17].[Id]
	LEFT JOIN [map].[Centers]() [P18] ON [E].[CenterId] = [P18].[Id]
	LEFT JOIN [map].[Units]() [U] ON [E].[UnitId] = [U].[Id]
	ORDER BY [E].[Index] ASC

	-- Resource Units from Entries/Account/Resource/Units
	SELECT [RU].[Multiplier], [RU].[Id], [RU].[UnitId], [U].[Name], [U].[Id], [U].[Name2], [U].[Name3], [RU].[ResourceId]
	FROM [map].[ResourceUnits]() [RU]
	LEFT JOIN [map].[Units]() [U] ON [RU].[UnitId] = [U].[Id]
	WHERE [RU].[ResourceId] IN (
		SELECT [R].[Id]
		FROM @Entries AS [E] -- EntryList
		LEFT JOIN [map].[Accounts]() [A] ON [E].[AccountId] = [A].[Id]
		LEFT JOIN [map].[Resources]() [R] ON [A].[ResourceId] = [R].[Id]
	)
	ORDER BY [RU].[Id] ASC

	-- Resource Units from Entries/Resource/Units
	SELECT [RU].[Multiplier], [RU].[Id], [RU].[UnitId], [U].[Name], [U].[Id], [U].[Name2], [U].[Name3], [RU].[ResourceId]
	FROM [map].[ResourceUnits]() [RU]
	LEFT JOIN [map].[Units]() [U] ON [RU].[UnitId] = [U].[Id]
	WHERE [RU].[ResourceId] IN (
		SELECT [R].[Id]
		FROM @Entries AS [E] -- EntryList
		LEFT JOIN [map].[Resources]() [R] ON [E].[ResourceId] = [R].[Id]
	)
	ORDER BY [RU].[Id] ASC