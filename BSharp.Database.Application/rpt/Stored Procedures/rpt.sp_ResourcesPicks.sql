CREATE PROCEDURE [rpt].[sp_ResourcesPicks]
	@Ids dbo.[IdList] READONLY
AS
	SELECT 	R.[ResourceDefinitionId], R.ResourceTypeId, R.[Id], RC.[Name] AS Classification, R.[Name], R.[IsActive], R.[Code],
		[UnitMonetaryValueMean], C.[Name] AS [Currency], [UnitMassMean], MM.[Name] AS [MassUnit], [UnitVolumeMean], MV.[Name] AS [VolumeUnit],
		[UnitAreaMean], MA.[Name] AS [AreaUnit], [UnitLengthMean], ML.[Name] AS [LengthUnit], [UnitTimeMean], MT.[Name] AS [TimeUnit], [UnitCountMean], MC.[Name] AS [CountUnit]
	FROM dbo.Resources R
	LEFT JOIN dbo.ResourceClassifications RC ON R.[ResourceClassificationId] = RC.[Id]
	LEFT JOIN dbo.Currencies C ON R.[MonetaryValueCurrencyId] = C.[Id]
	LEFT JOIN dbo.MeasurementUnits MM ON R.[MassUnitId] = MM.[Id]
	LEFT JOIN dbo.MeasurementUnits MV ON R.[VolumeUnitId] = MV.[Id]
	LEFT JOIN dbo.MeasurementUnits MA ON R.[AreaUnitId] = MA.[Id]
	LEFT JOIN dbo.MeasurementUnits ML ON R.[LengthUnitId] = ML.[Id]
	LEFT JOIN dbo.MeasurementUnits MT ON R.[TimeUnitId] = MT.[Id]
	LEFT JOIN dbo.MeasurementUnits MC ON R.[CountUnitId] = MC.[Id]
	WHERE R.[Id] IN (SELECT [Id] FROM @Ids);

	SELECT R.[Name], RP.[Id] As ResourcePickId, RP.[Code], RP.[AvailableSince], 
			RP.[MonetaryValue], RP.[Mass], RP.[Volume], RP.[Area], RP.[Length], RP.[Time], RP.[Count]
	FROM dbo.[ResourcePicks] RP 
	JOIN dbo.Resources R ON R.Id = RP.ResourceId
	WHERE R.[Id] IN (SELECT [Id] FROM @Ids);