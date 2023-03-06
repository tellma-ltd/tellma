CREATE PROCEDURE [wiz].[ToCashFromTradeReceivable]
	@TradeReceivableAccountId INT,
	@DueOnOrBefore DATE,
	@CashAccountId INT,
	@PostingDate DATE = NULL
AS
	DECLARE @CurrencyId0 NCHAR (3) = dal.fn_Agent__CurrencyId(@CashAccountId);
	SET @PostingDate = ISNULL(@PostingDate, GETDATE());
	
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

	DECLARE @WideLines WidelineList;
	INSERT INTO @WideLines([Index], [DocumentIndex],
		[AccountId1], [CenterId1], [AgentId1], [MonetaryValue1], [NotedAmount1], [CurrencyId1], [NotedDate1],
		[MonetaryValue0], [CurrencyId0], [Value1])
	SELECT ROW_NUMBER() OVER(ORDER BY SI.[Id]) - 1, 0,
		SS.[AccountId], SS.[CenterId], SS.[AgentId], SUM(SS.[Balance]), SUM(SS.[Balance]), SS.[CurrencyId], SI.[ToDate] AS [NotedDate1],
		bll.fn_ConvertCurrencies(@PostingDate, SS.[CurrencyId], @CurrencyId0, SUM(SS.[Balance])) AS [MonetaryValue0], @CurrencyId0,
		bll.fn_ConvertToFunctional(@PostingDate, SS.[CurrencyId], SUM(SS.[Balance]))
	FROM [dal].[ft_Concept_Center__Agents_Balances](N'CurrentTradeReceivables', NULL) SS
	JOIN dbo.Agents SI ON SI.[Id] = SS.[AgentId]
	WHERE SI.[Agent1Id] = @TradeReceivableAccountId
	AND ([SI].[ToDate] IS NULL OR [SI].[ToDate] <= ISNULL(@DueOnOrBefore, @PostingDate))
	GROUP BY SI.[Id], SS.[AccountId], SS.[CenterId], SS.[AgentId], SS.[CurrencyId], SI.[ToDate]
	HAVING SUM(SS.[Balance]) > 0

	SELECT * FROM @WideLines;
GO