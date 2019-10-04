DECLARE @AccountTypesEntryTypesParents AS TABLE (
	[ParentAccountTypeId]		NVARCHAR (255)		NOT NULL,
	[ParentEntryTypeId]		NVARCHAR (255)		NOT NULL,
  PRIMARY KEY ([ParentAccountTypeId], [ParentEntryTypeId])
);
INSERT INTO @AccountTypesEntryTypesParents
([ParentAccountTypeId],					[ParentEntryTypeId]) VALUES
(N'PropertyPlantAndEquipment',			N'ChangesInPropertyPlantAndEquipment'), 
(N'InvestmentProperty',					N'ChangesInInvestmentProperty'), 
(N'Goodwill',							N'ChangesInGoodwill'), 
(N'IntangibleAssetsOtherThanGoodwill',	N'ChangesInIntangibleAssetsOtherThanGoodwill'), 
(N'NoncurrentBiologicalAssets',			N'ChangesInBiologicalAssets'), 
(N'CurrentBiologicalAssets',			N'ChangesInBiologicalAssets'), 
(N'Inventories',						N'ChangesInInventories'),		
(N'CashAndCashEquivalents',				N'IncreaseDecreaseInCashAndCashEquivalents'), 
(N'Equity',								N'ChangesInEquity'), 
(N'OtherLongtermProvisions',			N'ChangesInOtherProvisions'), 
(N'OperatingExpense',					N'ExpenseByNature');

WITH
AccountTypesDescendants(AccountTypeId, ParentAccountTypeId) AS
(
	SELECT AT1.[Id], AT2.[Id]
	FROM dbo.AccountTypes AT1
	JOIN dbo.AccountTypes AT2 ON AT1.[Node].IsDescendantOf(AT2.[Node]) = 1
	WHERE AT2.[Id] IN (SELECT [ParentAccountTypeId] FROM @AccountTypesEntryTypesParents)
	AND AT1.IsAssignable = 1
),
EntryTypesDescendants(EntryTypeId, ParentEntryTypeId) AS
(
	SELECT ET1.[Id], ET2.[Id]
	FROM dbo.EntryTypes ET1
	JOIN dbo.EntryTypes ET2 ON ET1.[Node].IsDescendantOf(ET2.[Node]) = 1 
	WHERE ET2.[Id] IN (SELECT [ParentEntryTypeId] FROM @AccountTypesEntryTypesParents)
	AND ET1.IsAssignable = 1
),
AccountTypesEntryTypesExpanded([AccountTypeId], [EntryTypeId]) AS
(
	SELECT TA.AccountTypeId , TE.EntryTypeId
	FROM AccountTypesDescendants TA
	JOIN @AccountTypesEntryTypesParents TM ON TA.[ParentAccountTypeId] = TM.[ParentAccountTypeId]
	JOIN EntryTypesDescendants TE ON TM.[ParentEntryTypeId] = TE.[ParentEntryTypeId]
)
MERGE [dbo].[AccountTypesEntryTypes] AS t
USING AccountTypesEntryTypesExpanded AS s
ON (s.[AccountTypeId] = t.[AccountTypeId] AND s.[EntryTypeId] = t.[EntryTypeId])
WHEN NOT MATCHED BY SOURCE THEN
    DELETE
WHEN NOT MATCHED BY TARGET THEN
    INSERT ([AccountTypeId], [EntryTypeId])
    VALUES (s.[AccountTypeId], s.[EntryTypeId]);