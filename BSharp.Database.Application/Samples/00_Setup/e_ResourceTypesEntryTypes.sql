DECLARE @ResourceTypesEntryTypesParents AS TABLE (
	[ParentResourceTypeId]		NVARCHAR (255)		NOT NULL,
	[ParentEntryTypeId]		NVARCHAR (255)		NOT NULL,
  PRIMARY KEY ([ParentResourceTypeId], [ParentEntryTypeId])
);
INSERT INTO @ResourceTypesEntryTypesParents
([ParentResourceTypeId],					[ParentEntryTypeId]) VALUES
(N'PropertyPlantAndEquipment',			N'ChangesInPropertyPlantAndEquipment'), 
(N'InvestmentProperty',					N'ChangesInInvestmentProperty'), 
--(N'Goodwill',							N'ChangesInGoodwill'), 
(N'IntangibleAssetsOtherThanGoodwill',	N'ChangesInIntangibleAssetsOtherThanGoodwill'), 
(N'BiologicalAssets',					N'ChangesInBiologicalAssets'), 
(N'Inventories',						N'ChangesInInventories'),		
(N'CashAndCashEquivalents',				N'IncreaseDecreaseInCashAndCashEquivalents'), 
(N'Equity',								N'ChangesInEquity'), 
(N'OtherLongtermProvisions',			N'ChangesInOtherProvisions'), 
(N'ExpenseByNature',					N'ExpenseByFunctionExtension');

WITH
ResourceTypesDescendants(ResourceTypeId, ParentResourceTypeId) AS
(
	SELECT RT1.[Id], RT2.[Id]
	FROM dbo.[ResourceTypes] RT1
	JOIN dbo.[ResourceTypes] RT2 ON RT1.[Node].IsDescendantOf(RT2.[Node]) = 1
	WHERE RT2.[Id] IN (SELECT [ParentResourceTypeId] FROM @ResourceTypesEntryTypesParents)
	AND RT1.IsAssignable = 1
),
EntryTypesDescendants(EntryTypeId, ParentEntryTypeId) AS
(
	SELECT ET1.[Id], ET2.[Id]
	FROM dbo.EntryTypes ET1
	JOIN dbo.EntryTypes ET2 ON ET1.[Node].IsDescendantOf(ET2.[Node]) = 1 
	WHERE ET2.[Id] IN (SELECT [ParentEntryTypeId] FROM @ResourceTypesEntryTypesParents)
	AND ET1.IsAssignable = 1
),
ResourceTypesEntryTypesExpanded([ResourceTypeId], [EntryTypeId]) AS
(
	SELECT TA.ResourceTypeId, TE.EntryTypeId
	FROM ResourceTypesDescendants TA
	JOIN @ResourceTypesEntryTypesParents TM ON TA.[ParentResourceTypeId] = TM.[ParentResourceTypeId]
	JOIN EntryTypesDescendants TE ON TM.[ParentEntryTypeId] = TE.[ParentEntryTypeId]
)
MERGE [dbo].[ResourceTypesEntryTypes] AS t
USING ResourceTypesEntryTypesExpanded AS s
ON (s.[ResourceTypeId] = t.[ResourceTypeId] AND s.[EntryTypeId] = t.[EntryTypeId])
WHEN NOT MATCHED BY SOURCE THEN
    DELETE
WHEN NOT MATCHED BY TARGET THEN
    INSERT ([ResourceTypeId], [EntryTypeId])
    VALUES (s.[ResourceTypeId], s.[EntryTypeId]);

IF @DebugResourceTypesEntryTypes = 1
	SELECT * FROM dbo.ResourceTypesEntryTypes;