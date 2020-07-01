CREATE PROCEDURE [rpt].[Ifrs_210000]
--[210000] Statement of financial position, current/non-current
--EXEC [rpt].[Ifrs_210000] @toDate = '2019.03.31'
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
	SELECT [AT].[Concept] , SUM(E.[AlgebraicValue]) AS [Value]
	FROM [map].[DetailsEntries] () E
	JOIN dbo.Lines L ON L.[Id] = E.[LineId]
	JOIN dbo.[Accounts] A ON E.[AccountId] = A.[Id]
	JOIN dbo.[AccountTypes] [AT] ON A.[AccountTypeId] = [AT].[Id]
	WHERE L.[PostingDate] < DATEADD(DAY, 1, @toDate)
	-- TODO: consider subtypes of the ones below as well
	-- The #Mapping table can be persisted and used to add the column IFRS220000_ConceptId to the fact table.
	AND [AT].[Concept] IN (
		N'PropertyPlantAndEquipment',				
		N'InvestmentProperty',						
		N'Goodwill',								
		N'IntangibleAssetsOtherThanGoodwill',		
		N'InvestmentAccountedForUsingEquityMethod',
		N'InvestmentsInSubsidiariesJointVenturesAndAssociates',
												
		N'NoncurrentBiologicalAssets',
		N'NoncurrentReceivables',					
		N'NoncurrentInventories',
		N'DeferredTaxAssets',						
		N'CurrentTaxAssetsNoncurrent',				
		N'OtherNoncurrentFinancialAssets',			
		N'OtherNoncurrentNonfinancialAssets',		
		N'NoncurrentNoncashAssetsPledgedAsCollateralForWhichTransfereeHasRightByContractOrCustomToSellOrRepledgeCollateral',								

		N'Inventories',							
		N'TradeAndOtherCurrentReceivables',		
		N'CurrentTaxAssetsCurrent',				
		N'CurrentBiologicalAssets',				
		N'OtherCurrentFinancialAssets',			
		N'OtherCurrentNonfinancialAssets',			
		N'CashAndCashEquivalents',					
		N'CurrentNoncashAssetsPledgedAsCollateralForWhichTransfereeHasRightByContractOrCustomToSellOrRepledgeCollateral',										
		N'NoncurrentAssetsOrDisposalGroupsClassifiedAsHeldForSaleOrAsHeldForDistributionToOwners',

		N'IssuedCapital',							
		N'RetainedEarnings',						
		N'SharePremium',							
		N'TreasuryShares',							
		N'OtherEquityInterest',					
		N'OtherReserves',							

		N'NoncurrentProvisionsForEmployeeBenefits',
		N'OtherLongtermProvisions',				
		N'NoncurrentPayables',						
		N'DeferredTaxLiabilities',					
		N'CurrentTaxLiabilitiesNoncurrent',		
		N'OtherNoncurrentFinancialLiabilities',	
		N'OtherNoncurrentNonfinancialLiabilities',	

		N'CurrentProvisionsForEmployeeBenefits',	
		N'OtherShorttermProvisions',				
		N'TradeAndOtherCurrentPayables',			
		N'CurrentTaxLiabilitiesCurrent',			
		N'OtherCurrentFinancialLiabilities',		
		N'OtherCurrentNonfinancialLiabilities',	
		N'LiabilitiesIncludedInDisposalGroupsClassifiedAsHeldForSale'
	)
	GROUP BY [AT].[Concept] 
	
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