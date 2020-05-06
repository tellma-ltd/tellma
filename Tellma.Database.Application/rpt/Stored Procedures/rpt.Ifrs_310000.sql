CREATE PROCEDURE [rpt].[Ifrs_310000]
--[310000] Statement of comprehensive income, profit or loss, by function of expense
	@fromDate DATE, 
	@toDate DATE
AS
BEGIN
	SET NOCOUNT ON;
	
	CREATE TABLE [dbo].#IfrsDisclosureDetails (
		[Concept]			NVARCHAR (255)		NOT NULL,
		[Value]				DECIMAL
	);
	DECLARE @IfrsDisclosureId NVARCHAR (255) = N'IncomeStatementAbstract';

	INSERT INTO #IfrsDisclosureDetails (
			[Concept],
			[Value]
	)

	SELECT COALESCE([ET].[Code], [AT].[Code]) AS [Concept], SUM(E.[AlgebraicValue]) AS [Value]
	FROM [map].[DetailsEntries] () E
	JOIN dbo.[Accounts] A ON E.AccountId = A.[Id]
	JOIN dbo.[AccountTypes] [AT] ON A.[IfrsTypeId] = [AT].[Id]
	JOIN dbo.Lines L ON L.[Id] = E.[LineId]
	LEFT JOIN dbo.EntryTypes [ET] ON [ET].[Id] = E.[EntryTypeId]
	WHERE @fromDate <= L.[PostingDate] AND L.[PostingDate] < DATEADD(DAY, 1, @toDate)
	-- The #Mapping table can be persisted and used to add the column IFRS310000_ConceptId to the fact table.
	AND [AT].[Code] IN (
		N'Revenue',
		--N'InterestRevenueCalculatedUsingEffectiveInterestMethod',
		--N'InsuranceRevenue',
		N'OtherIncome',
		--N'ChangesInInventoriesOfFinishedGoodsAndWorkInProgress',
		--N'OtherWorkPerformedByEntityAndCapitalised',
		N'RawMaterialsAndConsumablesUsed',
		N'EmployeeBenefitsExpense',
		N'DepreciationAndAmortisationExpense',
		N'ImpairmentLossReversalOfImpairmentLossRecognisedInProfitOrLoss',
		N'OtherExpenseByNature',
		N'OtherGainsLosses',
		N'InsuranceServiceExpensesFromInsuranceContractsIssued',
		N'IncomeExpensesFromReinsuranceContractsHeldOtherThanFinanceIncomeExpenses',
		N'DifferenceBetweenCarryingAmountOfDividendsPayableAndCarryingAmountOfNoncashAssetsDistributed',
		N'GainsLossesOnNetMonetaryPosition',
		N'GainLossArisingFromDerecognitionOfFinancialAssetsMeasuredAtAmortisedCost',
		N'FinanceIncome',
		N'FinanceCosts',
		N'ImpairmentLossImpairmentGainAndReversalOfImpairmentLossDeterminedInAccordanceWithIFRS9',
		N'InsuranceFinanceIncomeExpensesFromInsuranceContractsIssuedRecognisedInProfitOrLoss',
		N'FinanceIncomeExpensesFromReinsuranceContractsHeldRecognisedInProfitOrLoss',
		N'ShareOfProfitLossOfAssociatesAndJointVenturesAccountedForUsingEquityMethod',
		N'OtherIncomeExpenseFromSubsidiariesJointlyControlledEntitiesAndAssociates',
		N'GainsLossesArisingFromDifferenceBetweenPreviousCarryingAmountAndFairValueOfFinancialAssetsReclassifiedAsMeasuredAtFairValue',
		N'CumulativeGainLossPreviouslyRecognisedInOtherComprehensiveIncomeArisingFromReclassificationOfFinancialAssetsOutOfFairValueThroughOtherComprehensiveIncomeIntoFairValueThroughProfitOrLossMeasurementCategory',
		N'HedgingGainsLossesForHedgeOfGroupOfItemsWithOffsettingRiskPositions',
		N'IncomeTaxExpenseContinuingOperations',
		N'ProfitLossFromDiscontinuedOperations'
	)
	GROUP BY COALESCE([ET].[Code], [AT].[Code])
	
	-- TODO: Calculate ProfitLossFromDiscontinuedOperations by considering centers who are discontinued during period
/*
	INSERT INTO #IfrsDisclosureDetails (
			[Concept],
			[Value]
	)
	SELECT N'ProfitLossFromDiscontinuedOperations', SUM([Value]) AS [Value]
	FROM dbo.IfrsDisclosureDetails
	WHERE [[DiscontinuedOn] > @fromDate
	AND [AccountType] IN (
		N'Revenue',
		--N'InterestRevenueCalculatedUsingEffectiveInterestMethod',
		--N'InsuranceRevenue',
		N'OtherIncome',
		--N'ChangesInInventoriesOfFinishedGoodsAndWorkInProgress',
		--N'OtherWorkPerformedByEntityAndCapitalised',
		N'RawMaterialsAndConsumablesUsed',
		N'EmployeeBenefitsExpense',
		N'DepreciationAndAmortisationExpense',
		N'ImpairmentLossReversalOfImpairmentLossRecognisedInProfitOrLoss',
		N'OtherExpenseByNature',
		N'OtherGainsLosses',
		N'InsuranceServiceExpensesFromInsuranceContractsIssued',
		N'IncomeExpensesFromReinsuranceContractsHeldOtherThanFinanceIncomeExpenses',
		N'DifferenceBetweenCarryingAmountOfDividendsPayableAndCarryingAmountOfNoncashAssetsDistributed',
		N'GainsLossesOnNetMonetaryPosition',
		N'GainLossArisingFromDerecognitionOfFinancialAssetsMeasuredAtAmortisedCost',
		N'FinanceIncome',
		N'FinanceCosts',
		N'ImpairmentLossImpairmentGainAndReversalOfImpairmentLossDeterminedInAccordanceWithIFRS9',
		N'InsuranceFinanceIncomeExpensesFromInsuranceContractsIssuedRecognisedInProfitOrLoss',
		N'FinanceIncomeExpensesFromReinsuranceContractsHeldRecognisedInProfitOrLoss',
		N'ShareOfProfitLossOfAssociatesAndJointVenturesAccountedForUsingEquityMethod',
		N'OtherIncomeExpenseFromSubsidiariesJointlyControlledEntitiesAndAssociates',
		N'GainsLossesArisingFromDifferenceBetweenPreviousCarryingAmountAndFairValueOfFinancialAssetsReclassifiedAsMeasuredAtFairValue',
		N'CumulativeGainLossPreviouslyRecognisedInOtherComprehensiveIncomeArisingFromReclassificationOfFinancialAssetsOutOfFairValueThroughOtherComprehensiveIncomeIntoFairValueThroughProfitOrLossMeasurementCategory',
		N'HedgingGainsLossesForHedgeOfGroupOfItemsWithOffsettingRiskPositions',
		N'IncomeTaxExpenseContinuingOperations',
		N'ProfitLossFromDiscontinuedOperations'
*/
	CREATE TABLE #Rollups (
		[ParentConcept]	NVARCHAR (255),
		[ChildConcept]	NVARCHAR (255)
	--	[Multiplier]	SMALLINT CHECK ([Multiplier] IN (-1, 1)) DEFAULT +1
	)
	INSERT INTO #Rollups
	([ParentConcept],	[ChildConcept]) VALUES
	(N'ProfitLoss',		N'Revenue'),
	-- continue fixing here
	(N'ProfitLoss',		N'OtherIncome'),
	(N'ProfitLoss',		N'RawMaterialsAndConsumablesUsed'),
	(N'ProfitLoss',		N'EmployeeBenefitsExpense'),
	(N'ProfitLoss',		N'DepreciationAndAmortisationExpense'),
	(N'ProfitLoss',		N'ImpairmentLossReversalOfImpairmentLossRecognisedInProfitOrLoss'),
	(N'ProfitLoss',		N'OtherExpenseByNature'),
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