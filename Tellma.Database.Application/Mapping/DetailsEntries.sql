CREATE FUNCTION [map].[DetailsEntries] (
	@CountUnitId INT = NULL,
	@MassUnitId INT = NULL,
	@VolumeUnitId INT = NULL
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
		E.[LineId],
		E.[ResponsibilityCenterId],
		E.[Direction],
		E.[AccountId],
		--E.[AccountIdentifier]
		E.[AgentId],
		E.[EntryTypeId],
		E.[ResourceId],
		--E.[ResourceIdentifier],
		E.[DueDate],

		E.[MonetaryValue],
		E.[Direction] * E.[MonetaryValue] AS [AlgebraicMonetaryValue],
		E.[CurrencyId],

		E.[Count],
		E.[Count] * ISNULL(CR.[Ratio], 0) AS [NormalizedCount],
		E.[Direction] * E.[Count] AS [AlgebraicCount],
		E.[Direction] * E.[Count] * ISNULL(CR.[Ratio], 0) AS [AlgebraicNormalizedCount],

		E.[Mass],
		E.[Mass] * ISNULL(MR.[Ratio], 0) AS [NormalizedMass],
		E.[Direction] * E.[Mass] AS [AlgebraicMass],
		E.[Direction] * E.[Mass] * ISNULL(MR.[Ratio], 0) AS [AlgebraicNormalizedMass],

		E.[Volume],
		E.[Volume] * ISNULL(MR.[Ratio], 0) AS [NormalizedVolume],
		E.[Direction] * E.[Volume] AS [AlgebraicVolume],
		E.[Direction] * E.[Volume] * ISNULL(MR.[Ratio], 0) AS [AlgebraicNormalizedVolume],
		
		E.[Time],
		E.[Direction] * E.[Time] AS [AlgebraicTime],

		E.[Value],
		E.[Direction] * E.[Value] AS [AlgebraicValue],
		
		E.[ExternalReference],
		E.[AdditionalReference],
		E.[NotedAgentId],
		E.[NotedAgentName],
		E.[NotedAmount],
		E.[NotedDate]
	FROM
		[dbo].[Entries] E
		LEFT JOIN dbo.Resources R ON E.ResourceId = R.Id
		LEFT JOIN UnitRatios CR ON R.CountUnitId = CR.Id
		LEFT JOIN UnitRatios MR ON R.MassUnitId = MR.Id
		LEFT JOIN UnitRatios CV ON R.VolumeUnitId = CV.Id