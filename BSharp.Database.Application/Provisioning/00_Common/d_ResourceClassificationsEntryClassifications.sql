DECLARE @ResourceClassificationsEntryClassificationsParents AS TABLE (
	[ResourceClassificationParentCode]		NVARCHAR (200)		NOT NULL,
	[EntryClassificationParentCode]			NVARCHAR (200)		NOT NULL,
  PRIMARY KEY ([ResourceClassificationParentCode], [EntryClassificationParentCode])
);
INSERT INTO @ResourceClassificationsEntryClassificationsParents
([ResourceClassificationParentCode],	[EntryClassificationParentCode]) VALUES
(N'PropertyPlantAndEquipment',			N'ChangesInPropertyPlantAndEquipment'), --1/1/1
(N'InvestmentProperty',					N'ChangesInInvestmentProperty'), --1/1/2
(N'Goodwill',							N'ChangesInGoodwill'), --1/1/3
(N'IntangibleAssetsOtherThanGoodwill',	N'ChangesInIntangibleAssetsOtherThanGoodwill'), --1/1/4
--N'InvestmentAccountedForUsingEquityMethod'. 1/1/5
-- InvestmentsInSubsidiariesJointVenturesAndAssociates, 1/1/6
(N'BiologicalAssets',					N'ChangesInBiologicalAssets'), --1/1/7
--  N'Receivables',1/1/8
(N'Inventories',						N'ChangesInInventories'),	
-- N'TaxAssets'
-- OtherFinancialAssets
-- OtherNonFinancialAssets
-- TradeAndOtherReceivables
(N'CashAndCashEquivalents',				N'IncreaseDecreaseInCashAndCashEquivalents'), --1/2/1/7
(N'Equity',								N'ChangesInEquity'), 
(N'OtherLongtermProvisions',			N'ChangesInOtherProvisions'), 
(N'RawMaterialsAndConsumablesUsed',		N'ExpenseByFunctionExtension'),
(N'CostOfMerchandiseSold',				N'CostOfSales'),
(N'ServicesExpense',					N'ExpenseByFunctionExtension'),
(N'EmployeeBenefitsExpense',			N'ExpenseByFunctionExtension'),
(N'DepreciationAmortisationAndImpairmentLossReversalOfImpairmentLossRecognisedInProfitOrLoss',
										N'ExpenseByFunctionExtension'),
(N'ImpairmentLossReversalOfImpairmentLossRecognisedInProfitOrLoss',
										N'ExpenseByFunctionExtension'),
(N'TaxExpenseOtherThanIncomeTaxExpense',N'ExpenseByFunctionExtension'),
(N'OtherExpenseByNature',				N'ExpenseByFunctionExtension');

WITH ResourceClassificationsEntryClassificationsParents AS
(
	SELECT
		dbo.fn_RCCode__Id([ResourceClassificationParentCode]) AS ResourceClassificationId,
		dbo.fn_ECCode__Id([EntryClassificationParentCode]) AS EntryClassificationId
	FROM @ResourceClassificationsEntryClassificationsParents
)
--select RCEC.*, EC.Code  from ResourceClassificationsEntryClassificationsParents RCEC 
--join dbo.EntryClassifications EC ON RCEC.EntryClassificationId = EC.Id;
MERGE [ResourceClassificationsEntryClassifications] AS t
USING ResourceClassificationsEntryClassificationsParents AS s
ON (s.[ResourceClassificationId] = t.[ResourceClassificationId] AND s.[EntryClassificationId] = t.[EntryClassificationId])
WHEN NOT MATCHED BY SOURCE THEN
    DELETE
WHEN NOT MATCHED BY TARGET THEN
    INSERT ([ResourceClassificationId], [EntryClassificationId])
    VALUES (s.[ResourceClassificationId], s.[EntryClassificationId]);

--WITH
--ResourceClassificationsDescendants(ResourceClassificationParentCode, ResourceClassificationChildId) AS
--(
--	SELECT  RCParent.[Code], RCChild.[Id]
--	FROM dbo.[ResourceClassifications] RCChild
--	JOIN dbo.[ResourceClassifications] RCParent ON RCChild.[Node].IsDescendantOf(RCParent.[Node]) = 1
--	WHERE RCParent.[Code] IN (SELECT [ResourceClassificationParentCode] FROM @ResourceClassificationsEntryClassificationsParents)
--	AND RCChild.IsAssignable = 1
--),
--EntryClassificationsDescendants(EntryClassificationParentCode, EntryClassificationChildId) AS
--(
--	SELECT ETParent.[Code], ETChild.[Id]
--	FROM dbo.[EntryClassifications] ETChild
--	JOIN dbo.[EntryClassifications] ETParent ON ETChild.[Node].IsDescendantOf(ETParent.[Node]) = 1 
--	WHERE ETParent.[Code] IN (SELECT [EntryClassificationParentCode] FROM @ResourceClassificationsEntryClassificationsParents)
--	AND ETChild.IsAssignable = 1
--),
--ResourceClassificationsEntryClassificationsExpanded([ResourceClassificationId], [EntryClassificationId]) AS
--(
--	SELECT TA.ResourceClassificationChildId, TE.EntryClassificationChildId
--	FROM ResourceClassificationsDescendants TA
--	JOIN @ResourceClassificationsEntryClassificationsParents TM ON TA.[ResourceClassificationParentCode] = TM.[ResourceClassificationParentCode]
--	JOIN EntryClassificationsDescendants TE ON TM.[EntryClassificationParentCode] = TE.[EntryClassificationParentCode]
--)
--MERGE [dbo].[ResourceClassificationsEntryClassifications] AS t
--USING ResourceClassificationsEntryClassificationsExpanded AS s
--ON (s.[ResourceClassificationId] = t.[ResourceClassificationId] AND s.[EntryClassificationId] = t.[EntryClassificationId])
--WHEN NOT MATCHED BY SOURCE THEN
--    DELETE
--WHEN NOT MATCHED BY TARGET THEN
--    INSERT ([ResourceClassificationId], [EntryClassificationId])
--    VALUES (s.[ResourceClassificationId], s.[EntryClassificationId]);

