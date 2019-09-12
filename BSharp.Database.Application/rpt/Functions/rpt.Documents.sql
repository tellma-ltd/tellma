CREATE FUNCTION [rpt].[Documents] (
	@Ids dbo.[IdList] READONLY
) RETURNS TABLE
AS 
RETURN
	WITH Docs AS (
		SELECT 	
			CAST(D.[Id] AS NVARCHAR(30)) AS [Id],
			CAST(D.[DocumentDate] AS NVARCHAR(30)) AS [DocumentDate],
			D.[DocumentTypeId],
			DT.Prefix + 
			REPLICATE(N'0', DT.[NumericalLength] - 1 - FLOOR(LOG10(D.SerialNumber))) +
			CAST(D.SerialNumber AS NVARCHAR(30)) AS [S/N],
			D.[State],
			ISNULL(VB.[StringPrefix], '') +
			ISNULL(CAST(D.[VoucherNumericReference] AS NVARCHAR(30)), '') AS [VoucherRef],
			AG.[Name] AS [AssignedTo],
			D.[Memo],
			DL.[Id] As [LineId],
			DL.[SortKey],
			DL.[LineTypeId],
			DLE.[Direction],
			DLE.[EntryNumber], A.[Name] AS [Account], DLE.[IfrsEntryClassificationId], 
			R.[Name] AS [Resource], MU.[Name] AS [Unit], DLE.[ResourcePickId],
			CAST(DLE.[Quantity] AS MONEY) AS [Quantity],
			CAST(DLE.[Value] AS MONEY) AS [Value]
		FROM dbo.Documents D
		JOIN dbo.DocumentTypes DT ON D.[DocumentTypeId] = DT.[Id]
		LEFT JOIN dbo.VoucherBooklets VB ON D.VoucherBookletId = VB.Id
		LEFT JOIN dbo.DocumentAssignments DA ON D.[Id] = DA.[DocumentId]
		LEFT JOIN dbo.Agents AG ON DA.AssigneeId = AG.Id
		LEFT JOIN dbo.DocumentLines DL ON D.[Id] = DL.[DocumentId]
		LEFT JOIN dbo.DocumentLineEntries DLE ON DL.[Id] = DLE.[DocumentLineId]
		JOIN dbo.Accounts A ON DLE.AccountId = A.[Id]
		LEFT JOIN dbo.Resources R ON DLE.[ResourceId] = R.[Id]
		LEFT JOIN dbo.MeasurementUnits MU ON R.[UnitId] = MU.[Id]
		WHERE D.[Id] IN (SELECT [Id] FROM @Ids)
	)
	SELECT 
		(CASE WHEN [SortKey] = 1 THEN [Id] ELSE '' END) AS [Id],
		(CASE WHEN [SortKey] = 1 THEN [DocumentDate] ELSE '' END) AS [DocumentDate],
		(CASE WHEN [SortKey] = 1 THEN [DocumentTypeId] ELSE '' END) AS [DocumentTypeId],
		(CASE WHEN [SortKey] = 1 THEN [S/N] ELSE '' END) AS [S/N],
		(CASE WHEN [SortKey] = 1 THEN [State] ELSE '' END) AS [State],
		(CASE WHEN [SortKey] = 1 THEN [VoucherRef] ELSE '' END) AS [VoucherRef],
		(CASE WHEN [SortKey] = 1 THEN [Memo] ELSE '' END) AS [Memo],
		(CASE WHEN [SortKey] = 1 THEN [AssignedTo] ELSE '' END) AS [AssignedTo],
		CAST([SortKey] AS TINYINT) AS [SortKey],
		[LineId], [LineTypeId],
		[EntryNumber], [Account], [IfrsEntryClassificationId],[Resource], [ResourcePickId],
		[Direction], [Quantity], [Unit], [Value]
	FROM Docs;
GO