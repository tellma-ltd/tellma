DECLARE @AccountsNotes AS TABLE (
	[AccountId]		NVARCHAR (255)		NOT NULL,
	[NoteId]		NVARCHAR (255)		NOT NULL,
	[Direction]		SMALLINT			NOT NULL,
  PRIMARY KEY ([AccountId], [NoteId], [Direction])
);
INSERT INTO @AccountsNotes([AccountId], [NoteId], [Direction])
SELECT A.[Node] As AccountId, N.[Id] AS [NoteId], N.Direction
FROM (
	SELECT [Node], [Node]
	FROM dbo.[IfrsAccountClassifications]
	WHERE [IsLeaf] = 1
) A	CROSS JOIN (
	SELECT [Code], [Id], [Direction] FROM dbo.[IfrsEntryClassifications]
	WHERE Direction <> 0 AND IsExtensible = 1
	UNION
	SELECT [Code], [Id], 1 FROM dbo.[IfrsEntryClassifications]
	WHERE Direction = 0 AND IsExtensible = 1
	UNION
	SELECT [Code], [Id], -1 FROM dbo.[IfrsEntryClassifications]
	WHERE Direction = 0 AND IsExtensible = 1
) N
WHERE (
--	(A.Code LIKE dbo.fn_Account__Code(N'PropertyPlantAndEquipment') +'%' AND N.Code LIKE dbo.fn_Note__Code(N'PropertyPlantAndEquipment') + '%') OR
	(A.[Node] LIKE N'1101%'												AND N.Code LIKE N'11%') OR
--	(A.Code LIKE dbo.fn_Account__Code(N'InvestmentProperty') +'%' AND N.Code LIKE dbo.fn_Note__Code(N'InvestmentProperty') + '%') OR
	(A.[Node] LIKE N'1102%'												AND N.Code LIKE N'12%') OR
--	(A.Code LIKE dbo.fn_Account__Code(N'Goodwill') +'%' AND N.Code LIKE dbo.fn_Note__Code(N'Goodwill') + '%') OR
	(A.[Node] LIKE N'1103%'												AND N.Code LIKE N'13%') OR
--	(A.Code LIKE dbo.fn_Account__Code(N'IntangibleAssetsOtherThanGoodwill') +'%' AND N.Code LIKE dbo.fn_Note__Code(N'IntangibleAssetsOtherThanGoodwill') + '%') OR
	(A.[Node] LIKE N'1104%'												AND N.Code LIKE N'14%') OR
--	(A.Code LIKE dbo.fn_Account__Code(N'NoncurrentBiologicalAssets') +'%' AND N.Code LIKE dbo.fn_Note__Code(N'BiologicalAssets') + '%') OR
	(A.[Node] LIKE N'1107%'												AND N.Code LIKE N'15%') OR
--	(A.Code LIKE dbo.fn_Account__Code(N'CurrentBiologicalAssets') +'%' AND N.Code LIKE dbo.fn_Note__Code(N'BiologicalAssets') + '%') OR
	(A.[Node] LIKE N'1214%'												AND N.Code LIKE N'15%') OR
--	(A.Code LIKE dbo.fn_Account__Code(N'CashAndCashEquivalents') +'%' AND N.Code LIKE dbo.fn_Note__Code(N'CashAndCashEquivalents') + '%') OR
	(A.[Node] LIKE N'1217%'												AND N.Code LIKE N'16%') OR
--	(A.Code LIKE dbo.fn_Account__Code(N'Equity') +'%' AND N.Code LIKE dbo.fn_Note__Code(N'Equity') + '%') OR
	(A.[Node] LIKE N'2%'													AND N.Code LIKE N'2%') OR
--	(A.Code LIKE dbo.fn_Account__Code(N'OtherLongtermProvisions') +'%' AND N.Code LIKE dbo.fn_Note__Code(N'OtherLongtermProvisions') + '%') OR
	(A.[Node] LIKE N'3112%'												AND N.Code LIKE N'3%') OR
--	(A.Code LIKE dbo.fn_Account__Code(N'CostOfSales') +'%' AND N.Code LIKE dbo.fn_Note__Code(N'ExpenseByNature') + '%') OR
	(A.[Node] LIKE N'410112%'												AND N.Code LIKE N'4%') OR
--	(A.Code LIKE dbo.fn_Account__Code(N'DistributionCosts') +'%' AND N.Code LIKE dbo.fn_Note__Code(N'ExpenseByNature') + '%') OR
	(A.[Node] LIKE N'41013%'												AND N.Code LIKE N'4%') OR
--	(A.Code LIKE dbo.fn_Account__Code(N'AdministrativeExpense') +'%' AND N.Code LIKE dbo.fn_Note__Code(N'ExpenseByNature') + '%') OR
	(A.[Node] LIKE N'41014%'												AND N.Code LIKE N'4%') OR
--	(A.Code LIKE dbo.fn_Account__Code(N'OtherExpenseByFunction') +'%' AND N.Code LIKE dbo.fn_Note__Code(N'ExpenseByNature') + '%') OR
	(A.[Node] LIKE N'41015%'												AND N.Code LIKE N'4%')
);
MERGE [dbo].[IfrsAccountClassificationsEntryClassifications] AS t
USING @AccountsNotes AS s
ON (s.[AccountId] = t.[AccountId] AND s.[NoteId] = s.[NoteId] AND s.[Direction] = s.[Direction])
WHEN NOT MATCHED BY SOURCE THEN
    DELETE
WHEN NOT MATCHED BY TARGET THEN
    INSERT ([IfrsAccountClassificationId], [IfrsEntryClassificationId], [Direction])
    VALUES (s.[AccountId], s.[NoteId], s.[Direction])
OPTION (RECOMPILE);