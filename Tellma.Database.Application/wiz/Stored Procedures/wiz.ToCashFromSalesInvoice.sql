CREATE PROCEDURE [wiz].[ToCashFromSalesInvoice]
	@SalesInvoiceId INT,
	@DueOnOrBefore DATE,
	@CashAccountId INT,
	@PostingDate DATE = NULL,
	@DueOnOrAfter DATE = NULL,
	@ReceivedAmount DECIMAL (19, 6) = NULL
AS
	DECLARE @CurrencyId0 NCHAR (3) = dal.fn_Agent__CurrencyId(@CashAccountId);
	SET @PostingDate = ISNULL(@PostingDate, GETDATE());
	
	IF @CashAccountId IS NULL
		THROW 50000, N'Please specify the cash account in the document header', 1;
	IF @CurrencyId0 IS NULL
		THROW 50000, N'Please specify the currency in the cash account', 1;

	DECLARE @WideLines WidelineList;
	INSERT INTO @WideLines([Index], [DocumentIndex],
		[AccountId1], [CenterId1], [AgentId1], [MonetaryValue1], [NotedAmount1], [CurrencyId1], [NotedDate1],
		[MonetaryValue0], [CurrencyId0], [Value1])
	SELECT ROW_NUMBER() OVER(ORDER BY SI.[ToDate], SI.[Id]) - 1, 0,
		SS.[AccountId], SS.[CenterId], SS.[AgentId], SUM(SS.[Balance]), SUM(SS.[Balance]), SS.[CurrencyId], SI.[ToDate] AS [NotedDate1],
		bll.fn_ConvertCurrencies(@PostingDate, SS.[CurrencyId], @CurrencyId0, SUM(SS.[Balance])) AS [MonetaryValue0], @CurrencyId0,
		bll.fn_ConvertToFunctional(@PostingDate, SS.[CurrencyId], SUM(SS.[Balance]))
	FROM [dal].[ft_Concept_Center__Agents_Balances](N'CurrentTradeReceivables', NULL) SS
	JOIN dbo.Agents SI ON SI.[Id] = SS.[AgentId]
	WHERE SI.[Id] = @SalesInvoiceId
	AND ([SI].[ToDate] IS NULL OR [SI].[ToDate] <= ISNULL(@DueOnOrBefore, @PostingDate))
	AND ([SI].[ToDate] IS NULL OR [SI].[ToDate] >= ISNULL(@DueOnOrAfter, N'1753-01-01'))
	GROUP BY SI.[Id], SS.[AccountId], SS.[CenterId], SS.[AgentId], SS.[CurrencyId], SI.[ToDate]
	HAVING SUM(SS.[Balance]) > 0

	-- The above returns one line always.
	-- MonetaryValue0 = Received Amount in Cash account currency
	-- MonetaryValue1 = Equivalent to Received Amount but in customer account currency.
	-- NotedAmount1 = Due Amount in Customer account currency, 
	IF @ReceivedAmount IS NOT NULL
	BEGIN
		UPDATE @WideLines
		SET 
			[MonetaryValue1] = CASE WHEN [MonetaryValue0] <> 0 THEN [MonetaryValue1] * (@ReceivedAmount / [MonetaryValue0]) ELSE 0 END,
			[Value1]         = CASE WHEN [MonetaryValue0] <> 0 THEN [Value1] * (@ReceivedAmount / [MonetaryValue0]) ELSE 0 END,
			[MonetaryValue0] = @ReceivedAmount
		WHERE @ReceivedAmount < [MonetaryValue0];
	END

	SELECT * FROM @WideLines;
GO