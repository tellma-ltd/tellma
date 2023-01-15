CREATE PROCEDURE [wiz].[ToCashFromTradeReceivable]
	@TradeReceivableAccountId INT,
	@DueOnOrBefore DATE,
	@CashAccountId INT,
	@PostingDate DATE = NULL
AS
	DECLARE @CurrencyId0 NCHAR (3) = dal.fn_Agent__CurrencyId(@CashAccountId);
	SET @PostingDate = ISNULL(@PostingDAte, GETDATE());
	
	IF @CashAccountId IS NULL
	BEGIN
		RAISERROR(N'Please specify the cash account in the document header', 16, 1);
		RETURN
	END
	ELSE IF @CurrencyId0 IS NULL
	BEGIN
		RAISERROR(N'Please specify the currency in the cash account', 16, 1);
		RETURN
	END

	DECLARE @WideLines WideLineList;
	INSERT INTO @WideLines([Index], [DocumentIndex],
		[CenterId0], [AgentId1], [MonetaryValue1], [NotedAmount1], [CurrencyId1], [NotedDate1],
		[MonetaryValue0],
		[CurrencyId0])
	SELECT ROW_NUMBER() OVER(ORDER BY SI.[Id], SS.[NotedDate]) - 1, 0,
		SS.[CenterId], SS.[AgentId], SS.[Balance], SS.[Balance], SS.[CurrencyId], [NotedDate],
		bll.fn_ConvertCurrencies(@PostingDate, SS.[CurrencyId], @CurrencyId0, SS.[Balance]) AS [MonetaryValue0], @CurrencyId0
	FROM [dal].[ft_Concept_Center__Agents_Balances](N'CurrentTradeReceivables', NULL) SS
	JOIN dbo.Agents SI ON SI.[Id] = SS.[AgentId]
	WHERE SI.[Agent1Id] = @TradeReceivableAccountId
	AND (@DueOnOrBefore IS NULL OR SI.[ToDate] <= @DueOnOrBefore)
	GROUP BY SI.[Id], SS.[NotedDate], SS.[CenterId], SS.[AgentId], SS.[CurrencyId]
	HAVING SUM(SS.[Balance]) > 0

	SELECT * FROM @WideLines;
GO