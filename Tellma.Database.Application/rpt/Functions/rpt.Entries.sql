CREATE FUNCTION [rpt].[Entries] (
-- TODO: This is actually only needed for SQL prototyping. map.Entries() is good enough for reporting.
	@fromDate Date = '2000.01.01', 
	@toDate Date = '2100.01.01',
	@CountUnitId INT,
	@MassUnitId INT,
	@VolumeUnitId INT
) RETURNS TABLE
AS
RETURN
	WITH UnitRatios AS (
		SELECT [Id], [UnitAmount] * (SELECT [BaseAmount] FROM  dbo.MeasurementUnits WHERE [Id] = @CountUnitId)
		/ ([BaseAmount] * (SELECT [UnitAmount] FROM  dbo.MeasurementUnits WHERE [Id] = @CountUnitId)) AS [Ratio]
		FROM dbo.MeasurementUnits
		WHERE UnitType = N'Count'
		UNION
		SELECT [Id], [UnitAmount] * (SELECT [BaseAmount] FROM  dbo.MeasurementUnits WHERE [Id] = @MassUnitId)
		/ ([BaseAmount] * (SELECT [UnitAmount] FROM  dbo.MeasurementUnits WHERE [Id] = @MassUnitId)) As [Ratio]
		FROM dbo.MeasurementUnits
		WHERE UnitType = N'Mass'
		UNION
		SELECT [Id], [UnitAmount] * (SELECT [BaseAmount] FROM  dbo.MeasurementUnits WHERE [Id] = @MassUnitId)
		/ ([BaseAmount] * (SELECT [UnitAmount] FROM  dbo.MeasurementUnits WHERE [Id] = @MassUnitId)) As [Ratio]
		FROM dbo.MeasurementUnits
		WHERE UnitType = N'Volume'
	)
	SELECT
		E.[Id],
		L.[Id] AS [LineId],
		D.[Id] AS [DocumentId],
		D.[DocumentDate],
		D.[SerialNumber],
		D.[VoucherNumericReference],
		D.[DocumentLookup1Id],
		D.[DocumentLookup2Id],
		D.[DocumentLookup3Id],
		D.[DocumentText1],
		D.[DocumentText2],
		D.[State] AS DocumentState,
		D.[DefinitionId] AS [DocumentDefinitionId],
		L.[DefinitionId] AS [LineDefinitionId],
		L.[State] AS [LineState],
		E.[ResponsibilityCenterId],
		E.[EntryNumber],
		E.[Direction],
		E.[AccountId],
		A.[LegacyTypeId],
		A.[IsCurrent],
		A.[LegacyClassificationId],
		A.[AccountTypeId],
		A.[AgentDefinitionId],
		--E.[AccountIdentifier]
		E.[AgentId],
		E.[EntryTypeId],
		E.[ResourceId],
		--E.[ResourceIdentifier],
		E.[DueDate],
		E.[MonetaryValue],
		E.[CurrencyId],
		E.[Count],
		R.[CountUnitId],
		E.[Count] * ISNULL(CR.[Ratio], 0) AS [NormalizedCount],
		E.[Mass],
		R.[MassUnitId],
		E.[Mass] * ISNULL(MR.[Ratio], 0) AS [NormalizedMass],
		E.[Volume],
		R.[VolumeUnitId],
		E.[Volume] * ISNULL(MR.[Ratio], 0) AS [NormalizedVolume],
		E.[Time],
		R.[TimeUnitId],
		E.[Value],
		L.[Memo],
		E.[ExternalReference],
		E.[AdditionalReference],
		E.[NotedAgentId],
		E.[NotedAgentName],
		E.[NotedAmount]
	FROM
		[dbo].[Entries] E
		JOIN [dbo].[Lines] L ON E.[LineId] = L.Id
		JOIN [dbo].[Documents] D ON L.[DocumentId] = D.[Id]
		JOIN dbo.Accounts A ON E.AccountId = A.Id
		LEFT JOIN dbo.Resources R ON E.ResourceId = R.Id
		LEFT JOIN UnitRatios CR ON R.CountUnitId = CR.Id
		LEFT JOIN UnitRatios MR ON R.MassUnitId = MR.Id
		LEFT JOIN UnitRatios CV ON R.VolumeUnitId = CV.Id

	WHERE
		(@fromDate IS NULL OR [DocumentDate] >= @fromDate)
	AND (@toDate IS NULL OR [DocumentDate] < DATEADD(DAY, 1, @toDate));