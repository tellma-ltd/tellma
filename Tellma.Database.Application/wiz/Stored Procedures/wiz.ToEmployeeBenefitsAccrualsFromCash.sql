CREATE PROCEDURE [wiz].[ToEmployeeBenefitsAccrualsFromCash]
	@EmployeeId INT,
	@DueOnOrBefore DATE,
	@CashAccountId INT,
	@DutyStation INT,
	@PaymentMechanism INT,
	@TruncatePayment INT,
	@PaymentSignificantDigits INT,
	@ShowBalancesInPaymentCurrency BIT,
	@PostingDate DATE
AS
	DECLARE @CurrencyId1 NCHAR (3) = dal.fn_Agent__CurrencyId(@CashAccountId);
	--SET @PostingDate = ISNULL(@PostingDate, GETDATE());

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

	SET @TruncatePayment= ISNULL(@TruncatePayment, 0); -- Round By Default
	SET @ShowBalancesInPaymentCurrency = ISNULL(@ShowBalancesInPaymentCurrency, 0);
	SET @PaymentSignificantDigits = ISNULL(@PaymentSignificantDigits, dal.fn_Currency__E(@CurrencyId1));

	DECLARE @WideLines WidelineList;

	INSERT INTO @WideLines([Index], [DocumentIndex],
		[CenterId0], [AgentId0], [MonetaryValue0],
		[NotedAmount0], [CurrencyId0], [NotedDate0],
		[MonetaryValue1], [CurrencyId1])
	SELECT ROW_NUMBER() OVER(ORDER BY SUM(SS.[Balance]), Emp.[Code], SS.[CurrencyId]--, SS.[NotedDate]
	) - 1, 0,
		SS.[CenterId], SS.[AgentId], -ROUND(SUM(SS.[Balance]), @PaymentSignificantDigits, @TruncatePayment) AS [MonetaryValue0],
		-SUM(SS.[Balance]) AS [NotedAmount0], SS.[CurrencyId], ISNULL(@DueOnOrBefore, @PostingDate), --SS.[NotedDate],
		-bll.fn_ConvertCurrencies(@PostingDate, SS.[CurrencyId], @CurrencyId1, SUM(SS.[Balance])) AS [MonetaryValue1], @CurrencyId1
	FROM [dal].[ft_Concept_Center__Agents_Balances](N'ShorttermEmployeeBenefitsAccruals', NULL) SS
	JOIN dbo.Agents Emp ON Emp.[Id] = SS.[AgentId]
	WHERE (@EmployeeId IS NULL OR SS.[AgentId] = @EmployeeId)
	AND (@ShowBalancesInPaymentCurrency = 0 OR SS.[CurrencyId] = @CurrencyId1)
	AND (@DueOnOrBefore IS NULL OR SS.[NotedDate] <= @DueOnOrBefore)
	AND (@DutyStation IS NULL OR Emp.[Agent2Id] = @DutyStation)
	AND (@PaymentMechanism IS NULL OR Emp.[Lookup8Id] = @PaymentMechanism)
	GROUP BY SS.[CenterId], SS.[AgentId], SS.[CurrencyId], --SS.[NotedDate], 
		Emp.[Code]
	HAVING (SUM(SS.[Balance]) < 0)

	SELECT * FROM @WideLines;
GO