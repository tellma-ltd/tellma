CREATE PROCEDURE [rpt].[Ifrs_210000]
--[210000] Statement of financial position, current/non-current
	@toDate DATE
AS
BEGIN
	SET NOCOUNT ON;
	
	CREATE TABLE [dbo].#IfrsDisclosureDetails (
		[Concept]			NVARCHAR (255)		NOT NULL,
		[Value]				DECIMAL
	);
	DECLARE @IfrsDisclosureId NVARCHAR (255) = N'StatementOfFinancialPositionAbstract';
	CREATE TABLE  #Mapping (
		[AccountType]	NVARCHAR (255),
		[IsCurrent]		BIT,
		[Concept]		NVARCHAR (255)
	);
	INSERT INTO #Mapping VALUES
	(N'PropertyPlantAndEquipment',				0, N'PropertyPlantAndEquipment'),
	(N'InvestmentProperty',						0, N'InvestmentProperty'),
	(N'Goodwill',								0, N'Goodwill'),
	(N'IntangibleAssetsOtherThanGoodwill',		0, N'IntangibleAssetsOtherThanGoodwill'),
	(N'InvestmentAccountedForUsingEquityMethod',0, N'InvestmentAccountedForUsingEquityMethod'),
	(N'InvestmentsInSubsidiariesJointVenturesAndAssociates',
												0, N'InvestmentsInSubsidiariesJointVenturesAndAssociates'),
	(N'BiologicalAssets',						0, N'NoncurrentBiologicalAssets'),
	(N'TradeAndOtherReceivables',				0, N'NoncurrentReceivables'),
	(N'InventoriesTotal',						0, N'NoncurrentInventories'),
	(N'DeferredTaxAssets',						0, N'DeferredTaxAssets'),
	(N'CurrentTaxAssets',						0, N'CurrentTaxAssetsNoncurrent'),
	(N'OtherFinancialAssets',					0, N'OtherNoncurrentFinancialAssets'),
	(N'OtherNonfinancialAssets',				0, N'OtherNoncurrentNonfinancialAssets'),
	(N'NoncashAssetsPledgedAsCollateralForWhichTransfereeHasRightByContractOrCustomToSellOrRepledgeCollateral',
												0, N'NoncurrentNoncashAssetsPledgedAsCollateralForWhichTransfereeHasRightByContractOrCustomToSellOrRepledgeCollateral'),

	(N'InventoriesTotal',						1, N'Inventories'),
	(N'TradeAndOtherReceivables',				1, N'TradeAndOtherCurrentReceivables'),
	(N'CurrentTaxAssets',						1, N'CurrentTaxAssetsCurrent'),
	(N'BiologicalAssets',						1, N'CurrentBiologicalAssets'),
	(N'OtherFinancialAssets',					1, N'OtherCurrentFinancialAssets'),
	(N'OtherNonfinancialAssets',				1, N'OtherCurrentNonfinancialAssets'),
	(N'CashAndCashEquivalents',					1, N'CashAndCashEquivalents'),
	(N'NoncashAssetsPledgedAsCollateralForWhichTransfereeHasRightByContractOrCustomToSellOrRepledgeCollateral',
												1, N'CurrentNoncashAssetsPledgedAsCollateralForWhichTransfereeHasRightByContractOrCustomToSellOrRepledgeCollateral'),
	(N'NoncurrentAssetsOrDisposalGroupsClassifiedAsHeldForSaleOrAsHeldForDistributionToOwners',
												1, N'NoncurrentAssetsOrDisposalGroupsClassifiedAsHeldForSaleOrAsHeldForDistributionToOwners'),

	(N'IssuedCapital',							0, N'IssuedCapital'),
	(N'RetainedEarnings',						0, N'RetainedEarnings'),
	(N'SharePremium',							0, N'SharePremium'),
	(N'TreasuryShares',							0, N'TreasuryShares'),
	(N'OtherEquityInterest',					0, N'OtherEquityInterest'),
	(N'OtherReserves',							0, N'OtherReserves'),

	(N'ProvisionsForEmployeeBenefits',			0, N'NoncurrentProvisionsForEmployeeBenefits'),
	(N'OtherProvisions',						0, N'OtherLongtermProvisions'),
	(N'TradeAndOtherPayables',					0, N'NoncurrentPayables'),
	(N'DeferredTaxLiabilities',					0, N'DeferredTaxLiabilities'),
	(N'CurrentTaxLiabilities',					0, N'CurrentTaxLiabilitiesNoncurrent'),
	(N'OtherFinancialLiabilities',				0, N'OtherNoncurrentFinancialLiabilities'),
	(N'OtherNonfinancialLiabilities',			0, N'OtherNoncurrentNonfinancialLiabilities'),

	(N'ProvisionsForEmployeeBenefits',			1, N'CurrentProvisionsForEmployeeBenefits'),
	(N'OtherProvisions',						1, N'OtherShorttermProvisions'),
	(N'TradeAndOtherPayables',					1, N'TradeAndOtherCurrentPayables'),
	(N'CurrentTaxLiabilities',					1, N'CurrentTaxLiabilitiesCurrent'),
	(N'OtherFinancialLiabilities',				1, N'OtherCurrentFinancialLiabilities'),
	(N'OtherCurrentNonfinancialLiabilities',	1, N'OtherCurrentNonfinancialLiabilities'),
	('LiabilitiesIncludedInDisposalGroupsClassifiedAsHeldForSale',
												1, N'LiabilitiesIncludedInDisposalGroupsClassifiedAsHeldForSale')

	INSERT INTO #IfrsDisclosureDetails (
			[Concept],
			[Value]
	)
	SELECT M.[Concept],	SUM(E.[Value]) AS [Value]
	FROM [map].[DetailsEntries] (NULL, NULL, NULL) E
	JOIN dbo.Lines L ON L.[Id] = E.[LineId]
	JOIN dbo.Documents D ON D.[Id] = L.[DocumentId]
	JOIN dbo.[Accounts] A ON E.[AccountId] = A.[Id]
	JOIN dbo.[AccountTypes] [AT] ON A.[AccountTypeId] = [AT].[Id]
	JOIN #Mapping M ON [AT].[Code] = M.[AccountType]
	--ON [AT].[Code] COLLATE SQL_Latin1_General_CP1_CI_AS = M.[AccountType] COLLATE SQL_Latin1_General_CP1_CI_AS
	AND E.[IsCurrent] = M.[IsCurrent]
	WHERE D.DocumentDate < DATEADD(DAY, 1, @toDate)
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
	CREATE TABLE #Rollups (
		[ParentConcept]	NVARCHAR (255),
		[ChildConcept]	NVARCHAR (255)

	)
	INSERT INTO #Rollups
	([ParentConcept],		[ChildConcept]) VALUES
	(N'NoncurrentAssets',	N'PropertyPlantAndEquipment'),
	(N'NoncurrentAssets',	N'InvestmentProperty'),
	(N'NoncurrentAssets',	N'Goodwill'),
	(N'NoncurrentAssets',	N'IntangibleAssetsOtherThanGoodwill'),
	(N'NoncurrentAssets',	N'InvestmentAccountedForUsingEquityMethod'),
	(N'NoncurrentAssets',	N'InvestmentsInSubsidiariesJointVenturesAndAssociates'),
	(N'NoncurrentAssets',	N'NoncurrentBiologicalAssets'),
	(N'NoncurrentAssets',	N'NoncurrentReceivables'),
	(N'NoncurrentAssets',	N'NoncurrentInventories'),
	(N'NoncurrentAssets',	N'DeferredTaxAssets'),
	(N'NoncurrentAssets',	N'CurrentTaxAssetsNoncurrent'),
	(N'NoncurrentAssets',	N'OtherNoncurrentFinancialAssets'),
	(N'NoncurrentAssets',	N'OtherNoncurrentNonfinancialAssets'),
	(N'NoncurrentAssets',	N'NoncurrentNoncashAssetsPledgedAsCollateralForWhichTransfereeHasRightByContractOrCustomToSellOrRepledgeCollateral'),
	
	(N'CurrentAssets',		N'Inventories'),
	(N'CurrentAssets',		N'TradeAndOtherCurrentReceivables'),
	(N'CurrentAssets',		N'CurrentTaxAssetsCurrent'),
	(N'CurrentAssets',		N'CurrentBiologicalAssets'),
	(N'CurrentAssets',		N'OtherCurrentFinancialAssets'),
	(N'CurrentAssets',		N'OtherCurrentNonfinancialAssets'),
	(N'CurrentAssets',		N'CashAndCashEquivalents'),
	(N'CurrentAssets',		N'CurrentNoncashAssetsPledgedAsCollateralForWhichTransfereeHasRightByContractOrCustomToSellOrRepledgeCollateral'),
	(N'CurrentAssets',		N'NoncurrentAssetsOrDisposalGroupsClassifiedAsHeldForSaleOrAsHeldForDistributionToOwners'),

	(N'Equity',				N'IssuedCapital'),
	(N'Equity',				N'RetainedEarnings'),
	(N'Equity',				N'SharePremium'),
	(N'Equity',				N'TreasuryShares'),
	(N'Equity',				N'OtherEquityInterest'),
	(N'Equity',				N'OtherReserves'),
	(N'Equity',				N'NoncontrollingInterests');

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