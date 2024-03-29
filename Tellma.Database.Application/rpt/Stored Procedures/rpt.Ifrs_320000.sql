﻿CREATE PROCEDURE [rpt].[Ifrs_320000]-- EXEC [rpt].[Ifrs_320000] @fromDate='2019.01.1', @toDate = '2019.03.31'
--[320000] Statement of comprehensive income, profit or loss, by nature of expense
	@fromDate DATE, 
	@toDate DATE,
	@PresentationCurrencyId NCHAR (3) = NULL
AS
BEGIN
	SET NOCOUNT ON;
	IF @PresentationCurrencyId IS NULL
		SET @PresentationCurrencyId = [dal].fn_FunctionalCurrencyId();
	
	CREATE TABLE [dbo].#IfrsDisclosureDetails (
		[Concept]			NVARCHAR (255)		NOT NULL,
		[Value]				DECIMAL
	);
	DECLARE @IfrsDisclosureId NVARCHAR (255) = N'IncomeStatementAbstract';

	INSERT INTO #IfrsDisclosureDetails (
			[Concept],
			[Value]
	)

	SELECT [AT].[Concept], SUM(E.[Direction] * E.[Value]) AS [Value]
	FROM [map].[DetailsEntries] () E
	JOIN dbo.[Accounts] A ON E.AccountId = A.[Id]
	JOIN dbo.[AccountTypes] [AT] ON A.[AccountTypeId] = [AT].[Id]
	JOIN dbo.Lines L ON L.[Id] = E.[LineId]
	WHERE @fromDate <= L.[PostingDate] AND L.[PostingDate] < DATEADD(DAY, 1, @toDate)
	-- TODO: replace with IsDescendantOf, to cater for possible customer extensions
	-- The #Mapping table can be persisted and used to add the column IFRS320000_ConceptId to the fact table.
	AND [AT].[Concept] IN (
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
	GROUP BY [AT].[Concept]
	
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
	-- TODO: consider adding more account types to meet IFRS Income Statement requirements
	(N'ProfitLoss',		N'OtherIncome'),
	(N'ProfitLoss',		N'CostOfSales'),
	(N'ProfitLoss',		N'DistributionCosts'),
	(N'ProfitLoss',		N'AdministrativeExpense'),
	(N'ProfitLoss',		N'OtherExpenseByFunction'),
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