CREATE PROCEDURE [wiz].[InventoryItem__PopulatePrice]
	@DocumentIndex	INT = 0,
	@PostingDate	DATE,
	@ResourceId		INT,
	@Quantity		DECIMAL (19,4) = NULL,
	@UnitId			INT = NULL,
	@PromotionCode	NVARCHAR (50) = NULL
AS
-- TODO: This SProc is not used anywhere in Tellma
-- Furthermore, poulating prices is done in the PreProcess script, by focing the price to be read only
-- And it will look into a line with state = 2 (agreed, approved) and compare all the parameters
-- Resource Id (or Resource1 for batches), POS Center (or null for all), Relation (for a customer or null for all), 
-- where Posting date falls in [Time, Time2], and with a promotion code (Text1 or null for all)
-- and when there are multiple options, use the cheapest, and scale by ratio of quantities.
	DECLARE @LineDefinitionId INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'RevenueFromInventoryWithPointInvoiceTemplate');
	
	IF @UnitId IS NULL
		SELECT @UnitId = [UnitId] FROM dbo.Resources WHERE [Id] = @ResourceId;
	
	SELECT @Quantity = ISNULL(@Quantity, 1);

	DECLARE @Lines LineList;
	DECLARE @Entries EntryList;

	DECLARE @LineId INT, @TemplateLineId INT, @Multiplier DECIMAL (19,4), @UnitScale DECIMAL (19,4);
	
	INSERT INTO @Lines([Index],
			[DefinitionId],		[PostingDate])
	SELECT 0,L.[DefinitionId], @PostingDate
	FROM dbo.Lines L
	JOIN dbo.Entries E ON L.[Id] = E.[LineId]
	WHERE L.[DefinitionId] = @LineDefinitionId
	AND E.ResourceId = @ResourceId
	AND E.[Index] = 1 AND E.[Quantity] <> 0
	-- AND ValidOf is correct, and Line is Approved
	
	INSERT INTO @Entries([Index],[LineIndex],[DocumentIndex],
		[Direction],
		[AccountId],
		[CurrencyId],
		[RelationId],
		[NotedRelationId],
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
		[InternalReference],
		[NotedAgentName],
		[NotedAmount],
		[NotedDate])
	SELECT [Index], 0 AS [LineIndex], 0 [DocumentIndex],
		[Direction],
		[AccountId],
		[CurrencyId],
		[RelationId],
		[NotedRelationId],
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
		[InternalReference],
		[NotedAgentName],
		[NotedAmount] * @Multiplier AS [NotedAmount],
		[NotedDate]
	FROM dbo.Entries
	WHERE LineId = @TemplateLineId

	EXEC [bll].[Lines__Pivot] @Lines = @Lines, @Entries = @Entries;