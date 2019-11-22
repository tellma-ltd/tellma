CREATE FUNCTION [rpt].[Documents] (
	@Ids dbo.[IdList] READONLY
) RETURNS TABLE
AS 
RETURN
	WITH Docs AS (
		SELECT 	
			CAST(D.[Id] AS NVARCHAR(30)) AS [Id],
			CAST(D.[DocumentDate] AS NVARCHAR(30)) AS [DocumentDate],
			D.[DocumentDefinitionId],
			DT.Prefix + 
			REPLICATE(N'0', DT.[CodeWidth] - 1 - FLOOR(LOG10(D.SerialNumber))) +
			CAST(D.SerialNumber AS NVARCHAR(30)) AS [S/N],
			D.[State],
			ISNULL(VB.[StringPrefix], '') +
			ISNULL(CAST(D.[VoucherNumericReference] AS NVARCHAR(30)), '') AS [VoucherRef],
			AG.[Name] AS [AssignedTo],
			D.[Memo],
			DL.[Id] As [LineId],
			DL.[SortKey],
			DL.[LineDefinitionId],
			DLE.[Direction],
			DLE.[EntryNumber], A.[Name] AS [Account], DLE.[EntryTypeId], 
			R.[Name] AS [Resource],
			CAST(DLE.[Value] AS MONEY) AS [Value],
			-- TODO: Add other unittypes
			CAST(DLE.[MonetaryValue] AS MONEY) AS [MonetaryValue],
			C.[Name] AS Currency,
			CAST(DLE.[Mass] AS MONEY) AS [Mass],
			MUM.[Name] AS [MassUnit]
		FROM dbo.Documents D
		JOIN dbo.[DocumentDefinitions] DT ON D.[DocumentDefinitionId] = DT.[Id]
		LEFT JOIN dbo.VoucherBooklets VB ON D.VoucherBookletId = VB.Id
		LEFT JOIN dbo.DocumentAssignments DA ON D.[Id] = DA.[DocumentId]
		LEFT JOIN dbo.Agents AG ON DA.AssigneeId = AG.Id
		LEFT JOIN dbo.DocumentLines DL ON D.[Id] = DL.[DocumentId]
		LEFT JOIN dbo.DocumentLineEntries DLE ON DL.[Id] = DLE.[DocumentLineId]
		JOIN dbo.[Accounts] A ON DLE.AccountId = A.[Id]
		LEFT JOIN dbo.Resources R ON A.[ResourceId] = R.[Id]
		LEFT JOIN dbo.Currencies C ON R.[MonetaryValueCurrencyId] = C.[Id]
		LEFT JOIN dbo.MeasurementUnits MUM ON R.[MassUnitId] = MUM.[Id]
		WHERE D.[Id] IN (SELECT [Id] FROM @Ids)
	)
	SELECT 
		(CASE WHEN [SortKey] = 1 THEN [Id] ELSE '' END) AS [Id],
		(CASE WHEN [SortKey] = 1 THEN [DocumentDate] ELSE '' END) AS [DocumentDate],
		(CASE WHEN [SortKey] = 1 THEN [DocumentDefinitionId] ELSE '' END) AS [DocumentTypeId],
		(CASE WHEN [SortKey] = 1 THEN [S/N] ELSE '' END) AS [S/N],
		(CASE WHEN [SortKey] = 1 THEN [State] ELSE '' END) AS [State],
		(CASE WHEN [SortKey] = 1 THEN [VoucherRef] ELSE '' END) AS [VoucherRef],
		(CASE WHEN [SortKey] = 1 THEN [Memo] ELSE '' END) AS [Memo],
		(CASE WHEN [SortKey] = 1 THEN [AssignedTo] ELSE '' END) AS [AssignedTo],
		CAST([SortKey] AS TINYINT) AS [SortKey],
		[LineId], [LineDefinitionId],
		[EntryNumber], [Account], [EntryTypeId],[Resource], --[ResourceInstanceId],
		[Direction], [Value], [MonetaryValue], [Currency], [Mass], [MassUnit]
	FROM Docs;
GO