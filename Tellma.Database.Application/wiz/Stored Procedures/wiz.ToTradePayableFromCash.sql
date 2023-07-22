CREATE PROCEDURE [wiz].[ToTradePayableFromCash]
	@TradePayableAccountId INT,
	@DueOnOrBefore DATE,
	@CashAccountId INT,
	@PostingDate DATE = NULL
AS
	DECLARE @CurrencyId1 NCHAR (3) = dal.fn_Agent__CurrencyId(@CashAccountId);
	SET @PostingDate = ISNULL(@PostingDAte, GETDATE());

	IF @CashAccountId IS NULL
	BEGIN
		RAISERROR(N'Please specify the cash account in the document header', 16, 1);
		RETURN
	END
	ELSE IF @CurrencyId1 IS NULL
	BEGIN
		RAISERROR(N'Please specify the currency in the cash account', 16, 1);
		RETURN
	END

	DECLARE @WideLines WidelineList;
	INSERT INTO @WideLines([Index], [DocumentIndex],
		[AccountId0], [CenterId0], [AgentId0], [MonetaryValue0], [NotedAmount0], [CurrencyId0], [NotedDate0],
		[MonetaryValue1], [CurrencyId1], [Value0], [EntryTypeId0])
	SELECT ROW_NUMBER() OVER(ORDER BY [PI].[Id], SS.[EntryTypeId]) - 1, 0,
		SS.[AccountId], SS.[CenterId], SS.[AgentId], -SUM(SS.[Balance]), -SUM(SS.[Balance]), SS.[CurrencyId], [PI].[ToDate] AS [NotedDate0],
		-bll.fn_ConvertCurrencies(@PostingDate, SS.[CurrencyId], @CurrencyId1, SUM(SS.[Balance])) AS [MonetaryValue1], @CurrencyId1,
		bll.fn_ConvertToFunctional(@PostingDate, SS.[CurrencyId], -SUM(SS.[Balance])),
		SS.[EntryTypeId]
	FROM [dal].[ft_Concept_Center__TradeAgents_Balances](N'TradeAndOtherCurrentPayablesToTradeSuppliers', NULL) SS
	JOIN dbo.Agents [PI] ON [PI].[Id] = SS.[AgentId]
	WHERE [PI].[Agent1Id] = @TradePayableAccountId
	AND ([PI].[ToDate] IS NULL OR [PI].[ToDate] <= ISNULL(@DueOnOrBefore, @PostingDate))
	GROUP BY [PI].[Id], SS.[AccountId], SS.[CenterId], SS.[AgentId], SS.[CurrencyId], [PI].ToDate, SS.[EntryTypeId]
	HAVING SUM(SS.[Balance]) < 0

	SELECT * FROM @WideLines;
GO