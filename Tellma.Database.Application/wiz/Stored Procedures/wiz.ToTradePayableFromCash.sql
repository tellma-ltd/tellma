CREATE PROCEDURE [wiz].[ToTradePayableFromCash]
	@TradePayableAccountId INT,
	@DueOnOrBefore DATE,
	@CashAccountId INT
AS
	DECLARE @CurrencyId NCHAR (3) = dal.fn_Agent__CurrencyId(@CashAccountId);
	
	IF @CashAccountId IS NULL
	BEGIN
		RAISERROR(N'Please specify the cash account in the document header', 16, 1);
		RETURN
	END
	ELSE IF @CurrencyId IS NULL
	BEGIN
		RAISERROR(N'Please specify the currency in the cash account', 16, 1);
		RETURN
	END

	DECLARE @WideLines WideLineList;
	INSERT INTO @WideLines([Index], [DocumentIndex],
		[CenterId0], [AgentId0],  [NotedAmount0], [CurrencyId0], [NotedDate0],
		[MonetaryValue1], [CurrencyId1])
	SELECT ROW_NUMBER() OVER(ORDER BY SI.[Id], SS.[NotedDate]) - 1, 0,
		SS.[CenterId], SS.[AgentId], -SS.[Balance], SS.[CurrencyId], [NotedDate], -SS.[Balance], @CurrencyId
	FROM [dal].[ft_Concept_Center__Agents_Balances](N'TradeAndOtherCurrentPayablesToTradeSuppliers', NULL) SS
	JOIN dbo.Agents SI ON SI.[Id] = SS.[AgentId]
	WHERE SI.[Agent1Id] = @TradePayableAccountId
	AND SS.[Balance] < 0
	AND (@DueOnOrBefore IS NULL OR [NotedDate] <= @DueOnOrBefore);

	SELECT * FROM @WideLines;