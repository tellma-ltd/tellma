CREATE PROCEDURE [wiz].[InventoryItem__PopulatePrice]
	@DocumentIndex	INT = 0,
	@PostingDate	DATE,
	@ResourceId		INT,
	@Quantity		DECIMAL (19,4) = NULL,
	@UnitId			INT = NULL,
	@PromotionCode	NVARCHAR (50) = NULL
AS
	DECLARE @LineDefinitionId INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'RevenueFromInventoryWithPointInvoiceTemplate');
	
	IF @UnitId IS NULL
		SELECT @UnitId = [UnitId] FROM dbo.Resources WHERE [Id] = @ResourceId;
	
	SELECT @Quantity = ISNULL(@Quantity, 1);

	DECLARE @Lines LineList;
	DECLARE @Entries EntryList;

	DECLARE @LineId INT, @TemplateLineId INT, @Multiplier DECIMAL (19,4), @UnitScale DECIMAL (19,4);
	
	INSERT INTO @Lines([Index],
			[DefinitionId],		[PostingDate], [TemplateLineId],[Multiplier])
	SELECT 0,L.[DefinitionId], @PostingDate,	L.[Id],			@Quantity / E.Quantity * [bll].[fn_ConvertUnits](@UnitId, E.[UnitId])
	FROM dbo.Lines L
	JOIN dbo.Entries E ON L.[Id] = E.[LineId]
	WHERE L.[DefinitionId] = @LineDefinitionId
	AND E.ResourceId = @ResourceId
	AND E.[Index] = 1 AND E.[Quantity] <> 0
	-- AND ValidOf is correct, and Line is Approved

	SELECT @TemplateLineId = [TemplateLineId], @Multiplier = [Multiplier] FROM @Lines WHERE [Index] = 0;
	
	INSERT INTO @Entries([Index],[LineIndex],[DocumentIndex],
		[Direction],
		[AccountId],
		[CurrencyId],
		[CustodianId],
		[CustodyId],
		[ParticipantId],
		[ResourceId],
		[CenterId],
		[EntryTypeId],
		[MonetaryValue],
		[Quantity],
		[UnitId],
		[Value],
		[Time1],
		[Time2],
		[ExternalReference],
		[AdditionalReference],
		[NotedRelationId],
		[NotedAgentName],
		[NotedAmount],
		[NotedDate])
	SELECT [Index], 0 AS [LineIndex], 0 [DocumentIndex],
		[Direction],
		[AccountId],
		[CurrencyId],
		[CustodianId],
		[CustodyId],
		[ParticipantId],
		[ResourceId],
		[CenterId],
		[EntryTypeId],
		[MonetaryValue] * @Multiplier AS [MonetaryValue],
		@Quantity AS [Quantity],
		@UnitId AS [UnitId],
		[Value] * @Multiplier AS [Value],
		[Time1],
		[Time2],
		[ExternalReference],
		[AdditionalReference],
		[NotedRelationId],
		[NotedAgentName],
		[NotedAmount] * @Multiplier AS [NotedAmount],
		[NotedDate]
	FROM dbo.Entries
	WHERE LineId = @TemplateLineId

	EXEC [bll].[Lines__Pivot] @Lines = @Lines, @Entries = @Entries;