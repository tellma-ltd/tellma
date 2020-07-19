CREATE PROCEDURE [rpt].[Ifrs_510000] -- EXEC [rpt].[Ifrs_510000] @fromDate='2019.01.1', @toDate = '2019.03.31'
--[510000] Statement of cash flows, direct method
	@fromDate DATE, 
	@toDate DATE,
	@PresentationCurrencyId NCHAR (3) = NULL
AS
BEGIN
	SET NOCOUNT ON;
	IF @PresentationCurrencyId IS NULL
		SET @PresentationCurrencyId = dbo.fn_FunctionalCurrencyId();
	
	CREATE TABLE [dbo].#IfrsDisclosureDetails (
		[Concept]			NVARCHAR (255)		NOT NULL,
		[Value]				DECIMAL
	);
	DECLARE @IfrsDisclosureId NVARCHAR (255) = N'StatementOfCashFlowsAbstract';

	INSERT INTO #IfrsDisclosureDetails (
			[Concept],
			[Value]
	)

	SELECT E.[EntryTypeId] AS [Concept], SUM(E.[AlgebraicValue]) AS [Value]
	FROM [map].[DetailsEntries2] (@PresentationCurrencyId) E
	JOIN dbo.[Accounts] A ON E.AccountId = A.[Id]
	JOIN dbo.[AccountTypes] [AT] ON A.[AccountTypeId] = [AT].[Id]
	JOIN dbo.Lines L ON L.[Id] = E.[LineId]
	WHERE (@fromDate <= L.[PostingDate]) AND (L.[PostingDate] < DATEADD(DAY, 1, @toDate))
	
	AND [AT].[Concept]  = N'CashAndCashEquivalents' -- TODO: Or below
	AND E.[EntryTypeId] <> (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'InternalCashTransfer')
	GROUP BY E.[EntryTypeId]
	
	CREATE TABLE #Rollups (
		[ParentConcept]	NVARCHAR (255),
		[ChildConcept]	NVARCHAR (255)
	--	[Multiplier]	SMALLINT CHECK ([Multiplier] IN (-1, 1)) DEFAULT +1
	)
	--TODO: adapt Rollup for Cash and Cash Equivalent 
	INSERT INTO #Rollups
	([ParentConcept],	[ChildConcept]) VALUES
	(N'IncreaseDecreaseInCashAndCashEquivalents',		N'Revenue'),
	-- TODO: consider adding more account types to meet IFRS Income Statement requirements
	(N'IncreaseDecreaseInCashAndCashEquivalents',		N'OtherIncome'),
	(N'IncreaseDecreaseInCashAndCashEquivalents',		N'CostOfSales'),
	(N'IncreaseDecreaseInCashAndCashEquivalents',		N'DistributionCosts'),
	(N'IncreaseDecreaseInCashAndCashEquivalents',		N'AdministrativeExpense'),
	(N'IncreaseDecreaseInCashAndCashEquivalents',		N'OtherExpenseByFunction'),
	(N'ProfitLoss',		N'OtherGainsLosses'),
	(N'ProfitLoss',		N'FinanceIncome'),
	(N'ProfitLoss',		N'FinanceCosts');
	INSERT INTO #IfrsDisclosureDetails (
			[Concept],
			[Value]
	)
	SELECT R.[ParentConcept], SUM([Value])
	FROM #IfrsDisclosureDetails C
	JOIN #Rollups R
	ON R.[ChildConcept] = C.[Concept]
	GROUP BY R.[ParentConcept]

	SELECT 	@IfrsDisclosureId, [Concept], [Value]
	FROM #IfrsDisclosureDetails;
	
	DROP TABLE #IfrsDisclosureDetails;
END