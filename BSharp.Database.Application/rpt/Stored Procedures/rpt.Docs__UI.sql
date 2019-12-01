CREATE PROCEDURE [rpt].[Docs__UI]
	@DIds dbo.IdList READONLY
AS
BEGIN
	WITH Docs AS (
		SELECT 	
			CAST(D.[Id] AS NVARCHAR(30)) AS [Id],
			CAST(D.[DocumentDate] AS NVARCHAR(30)) AS [DocumentDate],
			D.[DefinitionId] AS DocumentDefinitionId,
			DT.Prefix + 
			REPLICATE(N'0', DT.[CodeWidth] - 1 - FLOOR(LOG10(D.SerialNumber))) +
			CAST(D.SerialNumber AS NVARCHAR(30)) AS [S/N],
			D.[State],
			ISNULL(VB.[StringPrefix], '') +
			ISNULL(CAST(D.[VoucherNumericReference] AS NVARCHAR(30)), '') AS [VoucherRef],
			AG.[Name] AS [AssignedTo],
			D.[SortKey] As [DocumentSortKey],
			D.[Memo],
			DL.[Id] As [LineId],
			DL.[SortKey] AS [LineSortKey],
			DL.[DefinitionId] AS LineDefinitionId,
			DL.[State] AS [LineState],
			DLE.SortKey,
			DLE.[Direction],
			DLE.[EntryNumber], A.[Name] AS [Account], DLE.[EntryClassificationId],
			--CAST(DLE.[Value] AS MONEY) AS 
			DLE.[Value]
		FROM dbo.Documents D
		JOIN dbo.[DocumentDefinitions] DT ON D.[DefinitionId] = DT.[Id]
		LEFT JOIN dbo.VoucherBooklets VB ON D.VoucherBookletId = VB.Id
		LEFT JOIN dbo.DocumentAssignments DA ON D.[Id] = DA.[DocumentId]
		LEFT JOIN dbo.Agents AG ON DA.AssigneeId = AG.Id
		LEFT JOIN dbo.DocumentLines DL ON D.[Id] = DL.[DocumentId]
		LEFT JOIN dbo.DocumentLineEntries DLE ON DL.[Id] = DLE.[DocumentLineId]
		LEFT JOIN dbo.[Accounts] A ON DLE.AccountId = A.[Id]
		WHERE D.[Id] IN (SELECT [Id] FROM @DIds)
	),
	DocsFirst AS (
		SELECT DL.DocumentId, MIN(DLE.SortKey) AS SortKey
		FROM DocumentLineEntries DLE
		JOIN dbo.DocumentLines DL ON DLE.DocumentLineId = DL.Id
		WHERE DL.DocumentId IN (SELECT [Id] FROM @DIds)
		GROUP BY DL.DocumentId
	)
	SELECT 
		(CASE WHEN Docs.[SortKey] = DocsFirst.SortKey THEN [Id] ELSE '' END) AS [Id],
		(CASE WHEN Docs.[SortKey] = DocsFirst.SortKey THEN [DocumentDate] ELSE '' END) AS [DocumentDate],
		(CASE WHEN Docs.[SortKey] = DocsFirst.SortKey THEN [DocumentDefinitionId] ELSE '' END) AS [DocumentDefinitionId],
		(CASE WHEN Docs.[SortKey] = DocsFirst.SortKey THEN [S/N] ELSE '' END) AS [S/N],
		(CASE WHEN Docs.[SortKey] = DocsFirst.SortKey THEN [State] ELSE '' END) AS [State],
		(CASE WHEN Docs.[SortKey] = DocsFirst.SortKey THEN [VoucherRef] ELSE '' END) AS [V. Ref],
		(CASE WHEN Docs.[SortKey] = DocsFirst.SortKey THEN [Memo] ELSE '' END) AS [Memo],
		(CASE WHEN Docs.[SortKey] = DocsFirst.SortKey THEN [AssignedTo] ELSE '' END) AS [AssignedTo],
		(CASE WHEN Docs.[SortKey] = DocsFirst.SortKey THEN CAST([DocumentSortKey] AS TINYINT) ELSE '' END) AS [DSortKey],
		CAST([LineSortKey] AS TINYINT) AS [SortKey],
		[LineId], [LineDefinitionId],
		[EntryNumber] AS [E/N], 
		[Account], [EntryClassificationId],-- [Direction], 
		FORMAT([Direction] * [Value], '##,#.00;-;-', 'en-us') AS Debit,
		FORMAT(-[Direction] * [Value], '##,#.00;-;-', 'en-us') AS Credit,
		[LineState]
	FROM Docs
	LEFT JOIN DocsFirst ON Docs.Id = DocsFirst.DocumentId
	ORDER BY [DocumentSortKey], [LineSortKey];
END