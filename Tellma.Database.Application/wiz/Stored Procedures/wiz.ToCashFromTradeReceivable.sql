CREATE PROCEDURE [wiz].[ToCashFromTradeReceivable]
	@TradeReceivableAccountId INT,
	@DueOnOrBefore DATE,
	@CashAccountId INT,
	@PostingDate DATE = NULL,
	@DueOnOrAfter DATE = NULL,
	@ResourceId INT = NULL,
	@ReceivedAmount DECIMAL (19, 6) = NULL
AS
	DECLARE @CurrencyId0 NCHAR (3) = dal.fn_Agent__CurrencyId(@CashAccountId);
	SET @PostingDate = ISNULL(@PostingDate, GETDATE());
	
	IF @CashAccountId IS NULL
		THROW 50000, N'Please specify the cash account in the document header', 1;
	IF @CurrencyId0 IS NULL
		THROW 50000, N'Please specify the currency in the cash account', 1;

	DECLARE @WideLines WidelineList;
	WITH ResourceInvoices AS (
		SELECT E.NotedAgentId AS SI_Id
		FROM dbo.Lines L
		JOIN dbo.Entries E ON E.[LineId] = L.[Id]
		JOIN dbo.Accounts A ON A.[Id] = E.[AccountId]
		JOIN dbo.AccountTypes AC ON AC.[Id] = A.[AccountTypeId]
		JOIN dbo.Agents SI ON SI.[Id] = E.[NotedAgentId]
		WHERE L.[State] = 4
		AND AC.[Concept] = N'CurrentValueAddedTaxPayables'
		AND SI.[Agent1Id] = @TradeReceivableAccountId
		AND ([SI].[ToDate] IS NULL OR [SI].[ToDate] <= ISNULL(@DueOnOrBefore, @PostingDate))
		AND ([SI].[ToDate] IS NULL OR [SI].[ToDate] >= ISNULL(@DueOnOrAfter, N'1753-01-01'))
		AND E.[NotedResourceId] = @ResourceId
	)
	INSERT INTO @WideLines([Index], [DocumentIndex],
		[AccountId1], [CenterId1], [AgentId1], [MonetaryValue1], [NotedAmount1], [CurrencyId1], [NotedDate1],
		[MonetaryValue0], [CurrencyId0], [Value1])
	SELECT ROW_NUMBER() OVER(ORDER BY SI.[ToDate], SI.[Id]) - 1, 0,
		SS.[AccountId], SS.[CenterId], SS.[AgentId], SUM(SS.[Balance]), SUM(SS.[Balance]), SS.[CurrencyId], SI.[ToDate] AS [NotedDate1],
		bll.fn_ConvertCurrencies(@PostingDate, SS.[CurrencyId], @CurrencyId0, SUM(SS.[Balance])) AS [MonetaryValue0], @CurrencyId0,
		bll.fn_ConvertToFunctional(@PostingDate, SS.[CurrencyId], SUM(SS.[Balance]))
	FROM [dal].[ft_Concept_Center__Agents_Balances](N'CurrentTradeReceivables', NULL) SS
	JOIN dbo.Agents SI ON SI.[Id] = SS.[AgentId]
	WHERE SI.[Agent1Id] = @TradeReceivableAccountId
	AND ([SI].[ToDate] IS NULL OR [SI].[ToDate] <= ISNULL(@DueOnOrBefore, @PostingDate))
	AND ([SI].[ToDate] IS NULL OR [SI].[ToDate] >= ISNULL(@DueOnOrAfter, N'1753-01-01'))
	AND (@ResourceId IS NULL OR SI.[Id] IN (SELECT SI_Id FROM ResourceInvoices))
	GROUP BY SI.[Id], SS.[AccountId], SS.[CenterId], SS.[AgentId], SS.[CurrencyId], SI.[ToDate]
	HAVING SUM(SS.[Balance]) > 0

	-- The above may return multiple lines
	-- MonetaryValue0 = Received Amount in Cash account currency for the given sales invoice
	-- MonetaryValue1 = Equivalent to Received Amount but in customer account currency for the given sales invoice
	-- NotedAmount1 = Due Amount in Customer account currency for the given sales invoice
	-- SI.[ToDate] = Due date of the sales invoice
	IF @ReceivedAmount IS NOT NULL
	BEGIN
		DECLARE @LastIndex INT;
		DECLARE @Excess DECIMAL(19, 6);

		-- Calculate running total to locate @LastIndex and the excess over @ReceivedAmount
		WITH RunningTotals AS (
			SELECT 
				[Index],
				[MonetaryValue0],
				SUM([MonetaryValue0]) OVER (ORDER BY [Index]) AS [CumulativeTotal]
			FROM @WideLines
		)
		SELECT TOP 1 
			@LastIndex = [Index],
			@Excess = [CumulativeTotal] - @ReceivedAmount
		FROM RunningTotals
		WHERE [CumulativeTotal] >= @ReceivedAmount
		ORDER BY [Index];

		IF @LastIndex IS NOT NULL
		BEGIN
			-- DELETE @Widelines WHERE [Index] > @LastIndex
			DELETE FROM @WideLines 
			WHERE [Index] > @LastIndex;

			-- UPDATE @Widelines[@LastIndex] SO that SUM([MonetaryValue0]) = @ReceivedAmount
			UPDATE @WideLines
			SET 
				[MonetaryValue1] = CASE WHEN [MonetaryValue0] <> 0 THEN [MonetaryValue1] * ([MonetaryValue0] - @Excess) / [MonetaryValue0] ELSE 0 END,
				[Value1]         = CASE WHEN [MonetaryValue0] <> 0 THEN [Value1] * ([MonetaryValue0] - @Excess) / [MonetaryValue0] ELSE 0 END,
				[MonetaryValue0] = [MonetaryValue0] - @Excess
			WHERE [Index] = @LastIndex;
		END
	END

	SELECT * FROM @WideLines;
GO