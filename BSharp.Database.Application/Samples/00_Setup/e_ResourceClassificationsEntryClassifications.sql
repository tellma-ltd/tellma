DECLARE @ResourceClassificationsEntryClassificationsParents AS TABLE (
	[ResourceClassificationParentCode]		NVARCHAR (200)		NOT NULL,
	[EntryClassificationParentCode]			NVARCHAR (200)		NOT NULL,
  PRIMARY KEY ([ResourceClassificationParentCode], [EntryClassificationParentCode])
);
INSERT INTO @ResourceClassificationsEntryClassificationsParents
([ResourceClassificationParentCode],	[EntryClassificationParentCode]) VALUES
(N'PropertyPlantAndEquipment',			N'ChangesInPropertyPlantAndEquipment'),
(N'InvestmentProperty',					N'ChangesInInvestmentProperty'), 
(N'Goodwill',							N'ChangesInGoodwill'), 
(N'IntangibleAssetsOtherThanGoodwill',	N'ChangesInIntangibleAssetsOtherThanGoodwill'), 
(N'BiologicalAssets',					N'ChangesInBiologicalAssets'), 
(N'Inventories',						N'ChangesInInventories'),		
(N'CashAndCashEquivalents',				N'IncreaseDecreaseInCashAndCashEquivalents'), 
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

WITH
ResourceClassificationsDescendants(ResourceClassificationParentCode, ResourceClassificationChildId) AS
(
	SELECT  RCParent.[Code], RCChild.[Id]
	FROM dbo.[ResourceClassifications] RCChild
	JOIN dbo.[ResourceClassifications] RCParent ON RCChild.[Node].IsDescendantOf(RCParent.[Node]) = 1
	WHERE RCParent.[Code] IN (SELECT [ResourceClassificationParentCode] FROM @ResourceClassificationsEntryClassificationsParents)
	AND RCChild.IsAssignable = 1
),
EntryClassificationsDescendants(EntryClassificationParentCode, EntryClassificationChildId) AS
(
	SELECT ETParent.[Code], ETChild.[Id]
	FROM dbo.[EntryClassifications] ETChild
	JOIN dbo.[EntryClassifications] ETParent ON ETChild.[Node].IsDescendantOf(ETParent.[Node]) = 1 
	WHERE ETParent.[Code] IN (SELECT [EntryClassificationParentCode] FROM @ResourceClassificationsEntryClassificationsParents)
	AND ETChild.IsAssignable = 1
),
ResourceClassificationsEntryClassificationsExpanded([ResourceClassificationId], [EntryClassificationId]) AS
(
	SELECT TA.ResourceClassificationChildId, TE.EntryClassificationChildId
	FROM ResourceClassificationsDescendants TA
	JOIN @ResourceClassificationsEntryClassificationsParents TM ON TA.[ResourceClassificationParentCode] = TM.[ResourceClassificationParentCode]
	JOIN EntryClassificationsDescendants TE ON TM.[EntryClassificationParentCode] = TE.[EntryClassificationParentCode]
)
MERGE [dbo].[ResourceClassificationsEntryClassifications] AS t
USING ResourceClassificationsEntryClassificationsExpanded AS s
ON (s.[ResourceClassificationId] = t.[ResourceClassificationId] AND s.[EntryClassificationId] = t.[EntryClassificationId])
WHEN NOT MATCHED BY SOURCE THEN
    DELETE
WHEN NOT MATCHED BY TARGET THEN
    INSERT ([ResourceClassificationId], [EntryClassificationId])
    VALUES (s.[ResourceClassificationId], s.[EntryClassificationId]);

IF @DebugResourceClassificationsEntryClassifications = 1
	SELECT RC.[Name] AS [Resource Classification], EC.[Name] AS [Entry Classification]
	FROM dbo.ResourceClassifications RC
	JOIN dbo.[ResourceClassificationsEntryClassifications] RCET ON RC.Id = RCET.ResourceClassificationId
	JOIN dbo.[EntryClassifications] EC ON RCET.EntryClassificationId = EC.[Id]
	ORDER BY RC.[Node], EC.[Node]