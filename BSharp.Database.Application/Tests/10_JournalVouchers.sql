BEGIN -- Cleanup & Declarations

	DECLARE @LineType NVARCHAR (255), @DocsIdList dbo.[IdList];
END
-- get acceptable document types; and user permissions and general settings;
-- Journal Vouchers
DECLARE @VR1_2 VTYPE, @VRU_3 VTYPE, @Frequency NVARCHAR (255), @P1_2 int, @P1_U int, @PU_3 int, @P2_3 int,
		@date1 date = '2017.02.01', @date2 date = '2022.02.01', @dU datetime = '2018.02.01', @date3 datetime = '2023.02.01';
		:r .\11_Financing.sql
		:r .\12_ManualMisc.sql
		:r .\13_HRCycle.sql
		--:r .\12_Purchasing.sql
		--:r .\21_Financing.sql

		--:r .\40_PurchasingCycle.sql
		--:r .\13_ProductionCycle.sql
		--:r .\14_SalesCycle.sql

SELECT @fromDate = '2017.01.01', @toDate = '2017.01.31'

--SELECT count(*) FROM dbo.Documents;-- SELECT * FROM dbo.DocumentLines; SELECT * FROM dbo.DocumentLineEntries;

SELECT
	[Id], [AccountClassificationId], [Name], [Code],
	[IsMultiEntryClassification], [IfrsEntryClassificationId],
	[IsMultiAgent], [AgentId],[IsMultiResponsibilityCenter], [ResponsibilityCenterId], [IsMultiResource], [ResourceId]
FROM dbo.Accounts;
INSERT INTO @D11Ids([Id]) SELECT [Id] FROM dbo.Documents;
--SELECT * FROM rpt.Documents(@D11Ids) ORDER BY [SortKey], [EntryNumber];
WITH Docs AS (
		SELECT 	
			CAST(D.[Id] AS NVARCHAR(30)) AS [Id],
			CAST(D.[DocumentDate] AS NVARCHAR(30)) AS [DocumentDate],
			D.[DocumentDefinitionId],
			DT.Prefix + 
			REPLICATE(N'0', DT.[NumericalLength] - 1 - FLOOR(LOG10(D.SerialNumber))) +
			CAST(D.SerialNumber AS NVARCHAR(30)) AS [S/N],
			D.[State],
			ISNULL(VB.[StringPrefix], '') +
			ISNULL(CAST(D.[VoucherNumericReference] AS NVARCHAR(30)), '') AS [VoucherRef],
			AG.[Name] AS [AssignedTo],
			D.[SortKey] As [DocumentSortKey],
			D.[Memo],
			DL.[Id] As [LineId],
			DL.[SortKey] AS [LineSortKey],
			DL.[LineTypeId],
			DLE.[Direction],
			DLE.[EntryNumber], A.[Name] AS [Account], DLE.[IfrsEntryClassificationId],
			RC.[Name] AS [ResponsibilityCenter],
			[AGC].[Name ] AS [Agent],
			R.[Name] + ISNULL(N': ' + RP.[Name], N'') AS [Resource],
			CAST(DLE.[Value] AS MONEY) AS [Value],
			-- TODO: Add other unittypes
			CAST(DLE.[MonetaryValue] AS MONEY) AS [MonetaryValue],
			C.[Name] AS Currency,
			CAST(DLE.[Mass] AS MONEY) AS [Mass],
			MUM.[Name] AS [MassUnit],
			CAST(DLE.[Length] AS MONEY) AS [Length],
			MUL.[Name] AS [LengthUnit],
			CAST(DLE.[Time] AS MONEY) AS [Time],
			MUT.[Name] AS [TimeUnit],
			CAST(DLE.[Count] AS INT) AS [Count],
			MUC.[Name] AS [CountUnit]
		FROM dbo.Documents D
		JOIN dbo.[DocumentDefinitions] DT ON D.[DocumentDefinitionId] = DT.[Id]
		LEFT JOIN dbo.VoucherBooklets VB ON D.VoucherBookletId = VB.Id
		LEFT JOIN dbo.DocumentAssignments DA ON D.[Id] = DA.[DocumentId]
		LEFT JOIN dbo.Agents AG ON DA.AssigneeId = AG.Id
		LEFT JOIN dbo.DocumentLines DL ON D.[Id] = DL.[DocumentId]
		LEFT JOIN dbo.DocumentLineEntries DLE ON DL.[Id] = DLE.[DocumentLineId]
		JOIN dbo.Accounts A ON DLE.AccountId = A.[Id]
		LEFT JOIN dbo.ResponsibilityCenters RC ON DLE.[ResponsibilityCenterId] = RC.[Id]
		LEFT JOIN dbo.Agents AGC ON DLE.[AgentId] = AGC.[Id]
		LEFT JOIN dbo.Resources R ON DLE.[ResourceId] = R.[Id]
		LEFT JOIN dbo.ResourcePicks RP ON DLE.[ResourcePickId] = RP.[Id]
		LEFT JOIN dbo.Currencies C ON R.[CurrencyId] = C.[Id]
		LEFT JOIN dbo.MeasurementUnits MUM ON R.[MassUnitId] = MUM.[Id]
		LEFT JOIN dbo.MeasurementUnits MUL ON R.[LengthUnitId] = MUL.[Id]
		LEFT JOIN dbo.MeasurementUnits MUT ON R.[TimeUnitId] = MUT.[Id]
		LEFT JOIN dbo.MeasurementUnits MUC ON R.[CountUnitId] = MUC.[Id]
		WHERE D.[Id] IN (SELECT [Id] FROM @D11Ids)
	)
	SELECT 
		(CASE WHEN [LineSortKey] = 1 AND [EntryNumber] = 1 THEN [Id] ELSE '' END) AS [Id],
		(CASE WHEN [LineSortKey] = 1 AND [EntryNumber] = 1 THEN [DocumentDate] ELSE '' END) AS [DocumentDate],
		--(CASE WHEN [LineSortKey] = 1 AND [EntryNumber] = 1 THEN [DocumentDefinitionId] ELSE '' END) AS [DocumentTypeId],
		(CASE WHEN [LineSortKey] = 1 AND [EntryNumber] = 1 THEN [S/N] ELSE '' END) AS [S/N],
		(CASE WHEN [LineSortKey] = 1 AND [EntryNumber] = 1 THEN [State] ELSE '' END) AS [State],
		--(CASE WHEN [LineSortKey] = 1 AND [EntryNumber] = 1 THEN [VoucherRef] ELSE '' END) AS [VoucherRef],
		(CASE WHEN [LineSortKey] = 1 AND [EntryNumber] = 1 THEN [Memo] ELSE '' END) AS [Memo],
		--(CASE WHEN [LineSortKey] = 1 AND [EntryNumber] = 1 THEN [AssignedTo] ELSE '' END) AS [AssignedTo],
		--CAST([DocumentSortKey] AS TINYINT) AS [DocumentSortKey],
		--CAST([LineSortKey] AS TINYINT) AS [LineSortKey],
		--[LineId], [LineTypeId],
		--[EntryNumber], 
		[Account], [IfrsEntryClassificationId],[ResponsibilityCenter], [Agent], [Resource], 
		[Direction], [Value], [MonetaryValue], [Currency]
		, [Mass], [MassUnit], [Length], [LengthUnit], [Time], [TimeUnit], [Count], [CountUnit]
	FROM Docs
	ORDER BY [DocumentSortKey], [LineSortKey];

--SELECT * from [fi_Journal](@fromDate, @toDate) ORDER BY [Id], [EntryId];
--EXEC rpt_TrialBalance @fromDate = @fromDate, @toDate = @toDate, @PrintQuery=1;
--SELECT * FROM dbo.Documents;
--EXEC rpt_TrialBalance @fromDate = @fromDate, @toDate = @toDate, @ByCustody = 1, @ByResource = 1, @PrintQuery = 0;

--select * FROM Documents where  Id in (Select Id from @Docs);
--SElect * from lines where DocumentId in (Select Id from @Docs);
--select * from entries where lineid in (select id from lines where DocumentId in (Select Id from @Docs));