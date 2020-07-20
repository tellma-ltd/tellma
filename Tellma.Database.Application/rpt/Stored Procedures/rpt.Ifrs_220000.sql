CREATE PROCEDURE [rpt].[Ifrs_220000]
--[220000] Statement of financial position, order of liquidity
--EXEC [rpt].[Ifrs_220000] @toDate = '2019.03.31'
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
	DECLARE @IfrsDisclosureId NVARCHAR (255) = N'StatementOfFinancialPositionAbstract';
	CREATE TABLE  #Mapping (
		[AccountType]	NVARCHAR (255),
		[Concept]		NVARCHAR (255)
	);
	-- The #Mapping table can be persisted and used to add the column IFRS210000_ConceptId to the fact table.
	INSERT INTO #Mapping VALUES
	(N'PropertyPlantAndEquipment',				N'PropertyPlantAndEquipment'),
	(N'InvestmentProperty',						N'InvestmentProperty'),
	(N'Goodwill',								N'Goodwill'),
	(N'IntangibleAssetsOtherThanGoodwill',		N'IntangibleAssetsOtherThanGoodwill'),
	(N'InvestmentAccountedForUsingEquityMethod',N'InvestmentAccountedForUsingEquityMethod'),
	(N'InvestmentsInSubsidiariesJointVenturesAndAssociates',
												N'InvestmentsInSubsidiariesJointVenturesAndAssociates'),
	(N'NoncurrentBiologicalAssets',				N'BiologicalAssets'		 ),
	(N'NoncurrentReceivables',					N'TradeAndOtherReceivables'),
	(N'NoncurrentInventories',					N'InventoriesTotal' ),
	(N'DeferredTaxAssets',						N'DeferredTaxAssets'),
	(N'CurrentTaxAssetsNoncurrent',				N'CurrentTaxAssets'),
	(N'OtherNoncurrentFinancialAssets',			N'OtherFinancialAssets'),
	(N'OtherNoncurrentNonfinancialAssets',		N'OtherNonfinancialAssets'),
	(N'NoncurrentNoncashAssetsPledgedAsCollateralForWhichTransfereeHasRightByContractOrCustomToSellOrRepledgeCollateral',
												N'NoncashAssetsPledgedAsCollateralForWhichTransfereeHasRightByContractOrCustomToSellOrRepledgeCollateral'),

	(N'Inventories',							N'InventoriesTotal'),
	(N'TradeAndOtherCurrentReceivables',		N'TradeAndOtherReceivables'),
	(N'CurrentTaxAssetsCurrent',				N'CurrentTaxAssets'),
	(N'CurrentBiologicalAssets',				N'BiologicalAssets'),
	(N'OtherCurrentFinancialAssets',			N'OtherFinancialAssets'),
	(N'OtherCurrentNonfinancialAssets',			N'OtherNonfinancialAssets'),
	(N'CashAndCashEquivalents',					N'CashAndCashEquivalents'),
	(N'CurrentNoncashAssetsPledgedAsCollateralForWhichTransfereeHasRightByContractOrCustomToSellOrRepledgeCollateral',
												N'NoncashAssetsPledgedAsCollateralForWhichTransfereeHasRightByContractOrCustomToSellOrRepledgeCollateral'),
	(N'NoncurrentAssetsOrDisposalGroupsClassifiedAsHeldForSaleOrAsHeldForDistributionToOwners',
												N'NoncurrentAssetsOrDisposalGroupsClassifiedAsHeldForSaleOrAsHeldForDistributionToOwners'),

	(N'IssuedCapital',							N'IssuedCapital'),
	(N'RetainedEarnings',						N'RetainedEarnings'),
	(N'SharePremium',							N'SharePremium'),
	(N'TreasuryShares',							N'TreasuryShares'),
	(N'OtherEquityInterest',					N'OtherEquityInterest'),
	(N'OtherReserves',							N'OtherReserves'),

	(N'NoncurrentProvisionsForEmployeeBenefits',N'ProvisionsForEmployeeBenefits'),
	(N'OtherLongtermProvisions',				N'OtherProvisions'),
	(N'NoncurrentPayables',						N'TradeAndOtherPayables'),
	(N'DeferredTaxLiabilities',					N'DeferredTaxLiabilities'),
	(N'CurrentTaxLiabilitiesNoncurrent',		N'CurrentTaxLiabilities'),
	(N'OtherNoncurrentFinancialLiabilities',	N'OtherFinancialLiabilities'),
	(N'OtherNoncurrentNonfinancialLiabilities',	N'OtherNonfinancialLiabilities'),

	(N'CurrentProvisionsForEmployeeBenefits',	N'ProvisionsForEmployeeBenefits'),
	(N'OtherShorttermProvisions',				N'OtherProvisions'),
	(N'TradeAndOtherCurrentPayables',			N'TradeAndOtherPayables'),
	(N'CurrentTaxLiabilitiesCurrent',			N'CurrentTaxLiabilities'),
	(N'OtherCurrentFinancialLiabilities',		N'OtherFinancialLiabilities'),
	(N'OtherCurrentNonfinancialLiabilities',	N'OtherCurrentNonfinancialLiabilities'),
	('LiabilitiesIncludedInDisposalGroupsClassifiedAsHeldForSale',
												 N'LiabilitiesIncludedInDisposalGroupsClassifiedAsHeldForSale')

	INSERT INTO #IfrsDisclosureDetails (
			[Concept],
			[Value]
	)
	SELECT M.[Concept],	SUM(E.[AlgebraicValue]) AS [Value]
	FROM [map].[DetailsEntries] () E
	JOIN dbo.Lines L ON L.[Id] = E.[LineId]
	JOIN dbo.[Accounts] A ON E.[AccountId] = A.[Id]
	JOIN dbo.[AccountTypes] [AT] ON A.[AccountTypeId] = [AT].[Id]
	JOIN #Mapping M --ON [AT].[Code] = M.[AccountType]
	ON [AT].[Concept] COLLATE SQL_Latin1_General_CP1_CI_AS = M.[AccountType] COLLATE SQL_Latin1_General_CP1_CI_AS
	WHERE L.[PostingDate] < DATEADD(DAY, 1, @toDate)
	GROUP BY M.[Concept]
	
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
	-- Table #Rollups can be persisted as IFRS220000_Concepts, with ParentId to allow rollup of values in the tree
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