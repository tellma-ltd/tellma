CREATE PROCEDURE [rpt].[Ifrs_220000]
--[220000] Statement of financial position, order of liquidity
	@toDate DATE
AS
BEGIN
	SET NOCOUNT ON;
	
	CREATE TABLE [dbo].#IfrsDisclosureDetails (
		[Concept]			NVARCHAR (255)		NOT NULL,
		[Value]				DECIMAL
	);
	DECLARE @IfrsDisclosureId NVARCHAR (255) = N'StatementOfFinancialPositionAbstract';

	INSERT INTO #IfrsDisclosureDetails (
			[Concept],
			[Value]
	)
	SELECT [AT].[Code] , SUM(E.[Value]) AS [Value]
	FROM [map].[DetailsEntries] (NULL, NULL, NULL) E
	JOIN dbo.Lines L ON L.[Id] = E.[LineId]
	JOIN dbo.Documents D ON D.[Id] = L.[DocumentId]
	JOIN dbo.[Accounts] A ON E.[AccountId] = A.[Id]
	JOIN dbo.[AccountTypes] [AT] ON A.[AccountTypeId] = [AT].[Id]
	WHERE D.DocumentDate < DATEADD(DAY, 1, @toDate)
	AND [AT].[Code] IN (
		N'PropertyPlantAndEquipment',
		N'InvestmentProperty',
		N'Goodwill',
		N'IntangibleAssetsOtherThanGoodwill',
		N'OtherFinancialAssets',
		N'OtherNonfinancialAssets',
		N'InsuranceContractsIssuedThatAreAssets',
		N'ReinsuranceContractsHeldThatAreAssets',
		N'InvestmentAccountedForUsingEquityMethod',
		N'InvestmentsInSubsidiariesJointVenturesAndAssociates',
		N'BiologicalAssets',
		N'NoncurrentAssetsOrDisposalGroupsClassifiedAsHeldForSaleOrAsHeldForDistributionToOwners',
		N'InventoriesTotal',
		N'CurrentTaxAssets',
		N'DeferredTaxAssets',
		N'TradeAndOtherReceivables',
		N'CashAndCashEquivalents',	
		N'NoncashAssetsPledgedAsCollateralForWhichTransfereeHasRightByContractOrCustomToSellOrRepledgeCollateral',
		N'IssuedCapital',		
		N'RetainedEarnings',
		N'SharePremium',
		N'TreasuryShares',
		N'OtherEquityInterest',
		N'OtherReserves',
		N'TradeAndOtherPayables',
		N'ProvisionsForEmployeeBenefits',
		N'OtherProvisions',
		N'OtherFinancialLiabilities',
		N'OtherNonfinancialLiabilities',
		N'InsuranceContractsIssuedThatAreLiabilities',
		N'ReinsuranceContractsHeldThatAreLiabilities',
		N'CurrentTaxLiabilities',
		N'DeferredTaxLiabilities',
		N'LiabilitiesIncludedInDisposalGroupsClassifiedAsHeldForSale'
	)
	GROUP BY [AT].[Code] 
	
	-- TODO: Calculate NoncontrollingInterests by adding weighted average of Equity for tenants
/*
	INSERT INTO #IfrsDisclosureDetails (
			[Concept],
			[Value]
	)
	SELECT N'NoncontrollingInterests', SUM([Value]) AS [Value]
	FROM dbo.IfrsDisclosureDetails
	WHERE [IsControllingInterest] = 0
	AND [Concept] = N'Equity'
*/
	CREATE TABLE #Rollups (
		[ParentConcept]	NVARCHAR (255),
		[ChildConcept]	NVARCHAR (255)

	)
	INSERT INTO #Rollups
	([ParentConcept],	[ChildConcept]) VALUES
	(N'Assets',			N'PropertyPlantAndEquipment'),
	(N'Assets',			N'InvestmentProperty'),
	(N'Assets',			N'Goodwill'),
	(N'Assets',			N'IntangibleAssetsOtherThanGoodwill'),
	(N'Assets',			N'OtherFinancialAssets'),
	(N'Assets',			N'OtherNonfinancialAssets'),
	(N'Assets',			N'InsuranceContractsIssuedThatAreAssets'),
	(N'Assets',			N'ReinsuranceContractsHeldThatAreAssets'),
	(N'Assets',			N'InvestmentAccountedForUsingEquityMethod'),
	(N'Assets',			N'InvestmentsInSubsidiariesJointVenturesAndAssociates'),
	(N'Assets',			N'BiologicalAssets'),
	(N'Assets',			N'NoncurrentAssetsOrDisposalGroupsClassifiedAsHeldForSaleOrAsHeldForDistributionToOwners'),
	(N'Assets',			N'InventoriesTotal'),
	(N'Assets',			N'CurrentTaxAssets'),
	(N'Assets',			N'DeferredTaxAssets'),
	(N'Assets',			N'TradeAndOtherReceivables'),
	(N'Assets',			N'CashAndCashEquivalents'),

	(N'Equity',			N'NoncashAssetsPledgedAsCollateralForWhichTransfereeHasRightByContractOrCustomToSellOrRepledgeCollateral'),
	(N'Equity',			N'IssuedCapital'),		
	(N'Equity',			N'RetainedEarnings'),
	(N'Equity',			N'SharePremium'),
	(N'Equity',			N'TreasuryShares'),
	(N'Equity',			N'OtherEquityInterest'),
	(N'Equity',			N'OtherReserves'),

	(N'Liability',		N'TradeAndOtherPayables'),
	(N'Liability',		N'ProvisionsForEmployeeBenefits'),
	(N'Liability',		N'OtherProvisions'),
	(N'Liability',		N'OtherFinancialLiabilities'),
	(N'Liability',		N'OtherNonfinancialLiabilities'),
	(N'Liability',		N'InsuranceContractsIssuedThatAreLiabilities'),
	(N'Liability',		N'ReinsuranceContractsHeldThatAreLiabilities'),
	(N'Liability',		N'CurrentTaxLiabilities'),
	(N'Liability',		N'DeferredTaxLiabilities'),
	(N'Liability',		N'LiabilitiesIncludedInDisposalGroupsClassifiedAsHeldForSale');
	
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