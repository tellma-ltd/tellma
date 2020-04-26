CREATE PROCEDURE [rpt].[Docs__UI]
	@DIds dbo.IdList READONLY
AS
BEGIN
WITH Docs AS (
		SELECT 	
			CAST(D.[Id] AS NVARCHAR(30)) AS [Id],
			CAST(D.[PostingDate] AS NVARCHAR(30)) AS [PostingDate],
			D.[DefinitionId] AS DocumentDefinitionId,
			[bll].[fn_Prefix_CodeWidth_SN__Code](DD.[Prefix], DD.[CodeWidth], D.[SerialNumber]) AS [S/N],
			D.[State],
			ISNULL(DD.[Prefix], '') +
			ISNULL(CAST(D.[SerialNumber] AS NVARCHAR(30)), '') AS [VoucherRef],
			U.[Name] AS [AssignedTo],
			--D.[SortKey] As [DocumentSortKey],
			D.[Memo],
			L.[Id] As [LineId],
			L.[DefinitionId] AS LineDefinitionId,
			L.[State] AS [LineState],
			E.[Direction],
			E.[Index], A.[Name] AS [Account], IIF(AC.[ResourceAssignment] = N'A',1,0) AS [R?],
			E.[CurrencyId], E.[MonetaryValue], E.[EntryTypeId],
			--CAST(E.[Value] AS DECIMAL (19,4)) AS 
			E.[Value]
		FROM dbo.Documents D
		JOIN dbo.[DocumentDefinitions] DD ON D.[DefinitionId] = DD.[Id]
		LEFT JOIN dbo.DocumentAssignments DA ON D.[Id] = DA.[DocumentId]
		LEFT JOIN dbo.[Users] U ON DA.AssigneeId = U.Id
		LEFT JOIN dbo.[Lines] L ON D.[Id] = L.[DocumentId]
		LEFT JOIN dbo.[Entries] E ON L.[Id] = E.[LineId]
		LEFT JOIN dbo.[Accounts] A ON E.AccountId = A.[Id]
		LEFT JOIN dbo.[AccountTypes] AC ON A.[IfrsTypeId] = AC.[Id]
		WHERE D.[Id] IN (SELECT [Id] FROM @DIds)
	)-- select * from Docs
	,
	DocsFirst AS (
		SELECT L.DocumentId, MIN(E.[LineId]) AS [LineId]
		FROM dbo.[Entries] E
		LEFT JOIN dbo.[Lines] L ON E.[LineId] = L.Id
		WHERE L.DocumentId IN (SELECT [Id] FROM @DIds)
		GROUP BY L.DocumentId
	)
	SELECT 
		(CASE WHEN Docs.[LineId] = DocsFirst.LineId THEN Docs.[Id] ELSE '' END) AS [Id],
		(CASE WHEN Docs.[LineId] = DocsFirst.LineId THEN [PostingDate] ELSE '' END) AS [PostingDate],
		(CASE WHEN Docs.[LineId] = DocsFirst.LineId THEN [DocumentDefinitionId] ELSE '' END) AS [DocumentDefinitionId],
		(CASE WHEN Docs.[LineId] = DocsFirst.LineId THEN [S/N] ELSE '' END) AS [S/N],
		(CASE WHEN Docs.[LineId] = DocsFirst.LineId THEN [State] ELSE '' END) AS [DocumentState],
		(CASE WHEN Docs.[LineId] = DocsFirst.LineId THEN [VoucherRef] ELSE '' END) AS [V. Ref],
		(CASE WHEN Docs.[LineId] = DocsFirst.LineId THEN [Memo] ELSE '' END) AS [Memo],
		(CASE WHEN Docs.[LineId] = DocsFirst.LineId THEN [AssignedTo] ELSE '' END) AS [AssignedTo],
	--	(CASE WHEN Docs.[SortKey] = DocsFirst.SortKey THEN CAST([DocumentSortKey] AS TINYINT) ELSE '' END) AS [DSortKey],
		Docs.[LineId], [LineDefinitionId],
		[Index] AS [E/N], 
		[Account], [R?], [CurrencyId],
		FORMAT([Direction] * [MonetaryValue], '##,#;(##,#);-', 'en-us') AS [MonetaryValue],
		EC.[Name] AS [EntryClassification],-- [Direction], 
		FORMAT([Direction] * [Value], '##,#.00;-;-', 'en-us') AS Debit,
		FORMAT(-[Direction] * [Value], '##,#.00;-;-', 'en-us') AS Credit,
		[LineState]
	FROM Docs
	LEFT JOIN DocsFirst ON Docs.Id = DocsFirst.DocumentId
	LEFT JOIN dbo.[EntryTypes] EC ON [EntryTypeId] = EC.[Id]
	ORDER BY Docs.[LineId];
END;