CREATE PROCEDURE [rpt].[Ifrs_510000]
--[510000] Statement of cash flows, direct method
	@fromDate DATE, 
	@toDate DATE
AS
BEGIN
	SET NOCOUNT ON;
	
	CREATE TABLE [dbo].#IfrsDisclosureDetails (
		[Concept]			NVARCHAR (255)		NOT NULL,
		[Value]				DECIMAL
	);
	DECLARE @IfrsDisclosureId NVARCHAR (255) = N'StatementOfCashFlowsAbstract';

	INSERT INTO #IfrsDisclosureDetails (
			[Concept],
			[Value]
	)

	SELECT E.[EntryTypeId] AS [Concept], SUM(E.[Direction] * E.[Value]) AS [Value]
	FROM [map].[DetailsEntries] (@fromDate, @toDate, NULL, NULL, NULL) E
	JOIN dbo.[AccountTypes] [AT] ON E.[AccountTypeId] = [AT].[Id]
	WHERE [AT].[Code]  = N'CashAndCashEquivalents' -- TODO: Or below
	AND E.[EntryTypeId] <> (SELECT [Id] FROM dbo.EntryTypes WHERE [Code] = N'InternalCashTransfer')
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